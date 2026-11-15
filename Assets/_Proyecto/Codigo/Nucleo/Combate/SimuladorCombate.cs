using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Paso puro de la simulación: mismo estado + mismas entradas = mismo resultado, siempre.
    /// No lee el reloj, no genera números aleatorios y no asigna memoria.
    /// </summary>
    /// <remarks>
    /// Orden de cada frame: 1) cada luchador actúa según su acción y sus entradas;
    /// 2) se aplican las paredes; 3) se separan las cajas de empuje; 4) se resuelven los golpes
    /// (caja de golpe contra caja de daño); 5) los luchadores que pueden girar se orientan hacia
    /// el rival. Un luchador en hitstop no actúa: solo descuenta un frame de congelamiento.
    /// Antes de actuar, cada ataque recién pulsado se guarda en el búfer de entradas.
    /// Al final se descuenta el reloj y se decide la ronda: por K.O. o por tiempo. Con la ronda
    /// terminada la simulación sigue avanzando, pero ignora las entradas y ya no hay golpes.
    /// La resolución de los golpes vive en <see cref="ResolucionGolpes"/>.
    /// </remarks>
    public static class SimuladorCombate
    {
        public static EstadoPartida Avanzar(in EstadoPartida estado, in EntradasFrame entradasRecibidas, in ConfiguracionCombate configuracion)
        {
            var entradas = estado.RondaTerminada ? EntradasFrame.Vacias : entradasRecibidas;

            var luchador1 = LimitarAlEscenario(
                AvanzarLuchador(estado.Luchador1, entradas.Jugador1, configuracion.Jugador1, configuracion),
                configuracion.Jugador1,
                configuracion);

            var luchador2 = LimitarAlEscenario(
                AvanzarLuchador(estado.Luchador2, entradas.Jugador2, configuracion.Jugador2, configuracion),
                configuracion.Jugador2,
                configuracion);

            var (empujado1, empujado2) = ResolverEmpuje(luchador1, luchador2, configuracion);
            var (golpeado1, golpeado2) = estado.RondaTerminada
                ? (empujado1, empujado2)
                : ResolucionGolpes.Resolver(empujado1, empujado2, entradas, configuracion);
            var (orientado1, orientado2) = Orientar(golpeado1, golpeado2);

            if (estado.RondaTerminada)
            {
                return new EstadoPartida(estado.Frame + 1, orientado1, orientado2, estado.FramesRestantes, estado.Resultado);
            }

            var framesRestantes = estado.FramesRestantes > 0 ? estado.FramesRestantes - 1 : 0;
            var resultado = DecidirRonda(orientado1, orientado2, framesRestantes, configuracion);

            return new EstadoPartida(estado.Frame + 1, orientado1, orientado2, framesRestantes, resultado);
        }

        /// <summary>
        /// K.O. si alguien se queda sin vida (doble K.O. es empate). Con el tiempo agotado gana
        /// quien conserve mayor proporción de su vida máxima.
        /// </summary>
        private static ResultadoRonda DecidirRonda(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            int framesRestantes,
            in ConfiguracionCombate configuracion)
        {
            if (luchador1.SinVida || luchador2.SinVida)
            {
                if (luchador1.SinVida && luchador2.SinVida)
                {
                    return ResultadoRonda.Empate;
                }

                return luchador2.SinVida ? ResultadoRonda.GanaJugador1 : ResultadoRonda.GanaJugador2;
            }

            if (framesRestantes > 0)
            {
                return ResultadoRonda.EnCurso;
            }

            var proporcion1 = (long)luchador1.Vida * configuracion.Jugador2.VidaMaxima;
            var proporcion2 = (long)luchador2.Vida * configuracion.Jugador1.VidaMaxima;

            if (proporcion1 == proporcion2)
            {
                return ResultadoRonda.Empate;
            }

            return proporcion1 > proporcion2 ? ResultadoRonda.GanaJugador1 : ResultadoRonda.GanaJugador2;
        }

        private static EstadoLuchador AvanzarLuchador(
            in EstadoLuchador luchador,
            BotonesEntrada botones,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            var conBufer = ActualizarBufer(luchador, botones, configuracion.FramesBuferEntradas);

            var avanzado = conBufer.EnHitstop
                ? conBufer.ConHitstop(conBufer.FramesHitstop - 1)
                : Actuar(conBufer, botones, parametros, configuracion);

            return avanzado.ConBotonesAnteriores(botones);
        }

        /// <summary>
        /// Una pulsación nueva reemplaza lo guardado y dura los frames configurados, contando este.
        /// Si no hay pulsación, lo guardado envejece un frame, salvo durante el hitstop.
        /// </summary>
        private static EstadoLuchador ActualizarBufer(in EstadoLuchador luchador, BotonesEntrada botones, int framesBufer)
        {
            var pulsado = LecturaAtaques.RecienPulsado(botones, luchador.BotonesAnteriores);

            if (pulsado != TipoAtaque.Ninguno)
            {
                return luchador.ConBufer(pulsado, framesBufer);
            }

            if (luchador.EnHitstop || luchador.FramesBufer <= 0)
            {
                return luchador;
            }

            return luchador.FramesBufer == 1
                ? luchador.ConBufer(TipoAtaque.Ninguno, 0)
                : luchador.ConBufer(luchador.AtaqueEnBufer, luchador.FramesBufer - 1);
        }

        /// <param name="accion"><see cref="AccionLuchador.Atacando"/> en tierra o <see cref="AccionLuchador.AtacandoEnAire"/> en el aire.</param>
        private static EstadoLuchador IniciarAtaque(in EstadoLuchador luchador, AccionLuchador accion) =>
            luchador
                .ConAccionNueva(accion)
                .ConAtaque(luchador.AtaqueEnBufer)
                .ConGolpeConectado(false)
                .ConBufer(TipoAtaque.Ninguno, 0);

        private static EstadoLuchador SinAtaque(in EstadoLuchador luchador) =>
            luchador.ConGolpeConectado(false).ConAtaque(TipoAtaque.Ninguno);

        /// <summary>
        /// Cancelación: un ataque que ya conectó (golpeado o bloqueado) se interrumpe para lanzar
        /// otro más fuerte guardado en el búfer. Un ataque que falla no se puede cancelar.
        /// </summary>
        private static bool PuedeCancelar(in EstadoLuchador luchador) =>
            luchador.GolpeConectado && luchador.TieneAtaqueEnBufer && luchador.AtaqueEnBufer > luchador.Ataque;

        private static bool TieneAtaqueAereoEnBufer(in EstadoLuchador luchador) =>
            luchador.TieneAtaqueEnBufer && MovimientosLuchador.TieneVersionAerea(luchador.AtaqueEnBufer);

        /// <summary>Decide la acción de este frame para un luchador que salta: empezar, encadenar, seguir o terminar un ataque aéreo.</summary>
        private static EstadoLuchador DecidirAccionAerea(in EstadoLuchador luchador, in ParametrosLuchador parametros)
        {
            if (luchador.Accion == AccionLuchador.EnAire)
            {
                return TieneAtaqueAereoEnBufer(luchador)
                    ? IniciarAtaque(luchador, AccionLuchador.AtacandoEnAire)
                    : luchador.ConAccion(AccionLuchador.EnAire);
            }

            if (PuedeCancelar(luchador) && TieneAtaqueAereoEnBufer(luchador))
            {
                return IniciarAtaque(luchador, AccionLuchador.AtacandoEnAire);
            }

            return luchador.FramesEnAccion + 1 >= parametros.MovimientoEnCurso(luchador).DuracionTotal
                ? SinAtaque(luchador).ConAccionNueva(AccionLuchador.EnAire)
                : luchador.ConAccion(AccionLuchador.AtacandoEnAire);
        }

        /// <summary>
        /// Ataque en tierra: se cancela en otro más fuerte, o en un salto si el movimiento lo
        /// permite, golpeó sin ser bloqueado y el jugador mantiene arriba; si no, sigue hasta terminar.
        /// </summary>
        private static EstadoLuchador ContinuarAtaqueEnTierra(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            if (PuedeCancelar(luchador))
            {
                return IniciarAtaque(luchador, AccionLuchador.Atacando);
            }

            var movimiento = parametros.MovimientoEnCurso(luchador);

            var golpeoSinBloqueo = luchador.GolpeConectado && !luchador.GolpeBloqueado;

            if (golpeoSinBloqueo && movimiento.CancelableConSalto && InterpretacionEntradas.DireccionVertical(botones) > 0)
            {
                return SinAtaque(luchador).ConAccionNueva(AccionLuchador.PreSalto);
            }

            return luchador.FramesEnAccion + 1 >= movimiento.DuracionTotal
                ? ControlarEnTierra(SinAtaque(luchador), botones, parametros)
                : luchador.ConAccion(AccionLuchador.Atacando);
        }

        private static EstadoLuchador Actuar(
            in EstadoLuchador luchador,
            BotonesEntrada botones,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            switch (luchador.Accion)
            {
                case AccionLuchador.Atacando:
                    return ContinuarAtaqueEnTierra(luchador, botones, parametros);

                case AccionLuchador.Hitstun:
                case AccionLuchador.Blockstun:
                    return luchador.FramesAturdimiento <= 0
                        ? ControlarEnTierra(luchador.ConVelocidad(VectorFijo.Cero).ConGolpesEnCombo(0), botones, parametros)
                        : Deslizar(luchador, configuracion.FriccionRetroceso).ConAturdimiento(luchador.FramesAturdimiento - 1);

                case AccionLuchador.Noqueado:
                    return Deslizar(luchador, configuracion.FriccionRetroceso);

                case AccionLuchador.EnAire:
                case AccionLuchador.AtacandoEnAire:
                    return MoverEnAire(DecidirAccionAerea(luchador, parametros), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);

                case AccionLuchador.GolpeadoEnAire:
                    return MoverEnAire(
                        luchador.ConAccion(AccionLuchador.GolpeadoEnAire),
                        configuracion.Juggle.GravedadCon(luchador.GolpesEnCombo),
                        parametros.Salto.FramesAterrizaje);

                case AccionLuchador.Derribado:
                    return luchador.FramesEnAccion + 1 >= configuracion.Juggle.FramesDerribado
                        ? ControlarEnTierra(luchador.ConGolpesEnCombo(0), botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Derribado);

                case AccionLuchador.PreSalto:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesPreSalto
                        ? Despegar(luchador, botones, parametros.Salto)
                        : luchador.ConAccion(AccionLuchador.PreSalto);

                case AccionLuchador.Aterrizaje:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesAterrizaje
                        ? ControlarEnTierra(luchador, botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Aterrizaje);

                default:
                    return ControlarEnTierra(luchador, botones, parametros);
            }
        }

        private static EstadoLuchador ControlarEnTierra(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            if (luchador.TieneAtaqueEnBufer)
            {
                return IniciarAtaque(luchador, AccionLuchador.Atacando);
            }

            var vertical = InterpretacionEntradas.DireccionVertical(botones);

            if (vertical > 0)
            {
                return luchador.ConAccion(AccionLuchador.PreSalto);
            }

            if (vertical < 0)
            {
                return luchador.ConAccion(AccionLuchador.Agachado);
            }

            var direccion = InterpretacionEntradas.DireccionHorizontal(botones);

            if (direccion == 0)
            {
                return luchador.ConAccion(AccionLuchador.Quieto);
            }

            var haciaAdelante = direccion == luchador.SentidoAdelante;
            var velocidad = haciaAdelante ? parametros.VelocidadAdelante : parametros.VelocidadAtras;
            var accion = haciaAdelante ? AccionLuchador.CaminandoAdelante : AccionLuchador.CaminandoAtras;

            return luchador
                .ConAccion(accion)
                .ConX(luchador.Posicion.X + velocidad * direccion);
        }

        /// <summary>Un frame sin control en el suelo: el luchador se desliza con su velocidad de retroceso y la fricción la frena.</summary>
        private static EstadoLuchador Deslizar(in EstadoLuchador luchador, Fijo friccion)
        {
            var velocidadX = luchador.Velocidad.X;
            var frenada = Fijo.Max(Fijo.Abs(velocidadX) - friccion, Fijo.Cero) * Fijo.Signo(velocidadX);

            return luchador
                .ConAccion(luchador.Accion)
                .ConX(luchador.Posicion.X + velocidadX)
                .ConVelocidad(new VectorFijo(frenada, Fijo.Cero));
        }

        /// <summary>
        /// La dirección del salto se lee en el frame de despegue, no al pulsar arriba: así el
        /// jugador puede corregirla durante el presalto. Una vez en el aire ya no cambia.
        /// </summary>
        private static EstadoLuchador Despegar(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosSalto salto)
        {
            var direccion = InterpretacionEntradas.DireccionHorizontal(botones);
            var velocidadHorizontal = direccion == 0
                ? Fijo.Cero
                : (direccion == luchador.SentidoAdelante ? salto.VelocidadAdelante : salto.VelocidadAtras) * direccion;

            return luchador
                .ConAccionNueva(AccionLuchador.EnAire)
                .ConVelocidad(new VectorFijo(velocidadHorizontal, salto.VelocidadVertical));
        }

        /// <summary>
        /// Integración explícita: primero se mueve con la velocidad actual y después la gravedad la
        /// reduce. La acción del frame ya viene decidida; aquí solo cambia si toca el suelo.
        /// </summary>
        private static EstadoLuchador MoverEnAire(in EstadoLuchador luchador, Fijo gravedad, int framesAterrizaje)
        {
            var posicion = luchador.Posicion + luchador.Velocidad;

            if (posicion.Y <= Fijo.Cero)
            {
                return Aterrizar(luchador.ConPosicion(posicion.ConY(Fijo.Cero)), framesAterrizaje);
            }

            return luchador
                .ConPosicion(posicion)
                .ConVelocidad(luchador.Velocidad.ConY(luchador.Velocidad.Y - gravedad));
        }

        /// <summary>
        /// Quien cae sin vida queda noqueado; quien cae golpeado, derribado; quien salta por su
        /// cuenta aterriza y pierde el ataque aéreo que estuviera haciendo.
        /// </summary>
        private static EstadoLuchador Aterrizar(in EstadoLuchador luchador, int framesAterrizaje)
        {
            if (luchador.SinVida)
            {
                return SinAtaque(luchador)
                    .ConVelocidad(luchador.Velocidad.ConY(Fijo.Cero))
                    .ConAccionNueva(AccionLuchador.Noqueado);
            }

            var enReposo = SinAtaque(luchador).ConVelocidad(VectorFijo.Cero);

            if (luchador.Accion == AccionLuchador.GolpeadoEnAire)
            {
                return enReposo.ConAccionNueva(AccionLuchador.Derribado);
            }

            return enReposo.ConAccionNueva(framesAterrizaje > 0 ? AccionLuchador.Aterrizaje : AccionLuchador.Quieto);
        }

        private static EstadoLuchador LimitarAlEscenario(in EstadoLuchador luchador, in ParametrosLuchador parametros, in ConfiguracionCombate configuracion)
        {
            var medioAncho = parametros.CajaEmpuje(luchador).MedioAncho;
            var minimo = configuracion.LimiteIzquierdo + medioAncho;
            var maximo = configuracion.LimiteDerecho - medioAncho;

            return luchador.ConX(Fijo.Limitar(luchador.Posicion.X, minimo, maximo));
        }

        /// <summary>
        /// Separa las cajas de empuje en horizontal repartiendo el solapamiento a partes iguales.
        /// Si uno queda contra la pared, el otro absorbe todo el desplazamiento: nadie atraviesa a nadie.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) ResolverEmpuje(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in ConfiguracionCombate configuracion)
        {
            var caja1 = configuracion.Jugador1.CajaEmpuje(luchador1);
            var caja2 = configuracion.Jugador2.CajaEmpuje(luchador2);

            if (!caja1.Intersecta(caja2))
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X != luchador2.Posicion.X
                ? luchador1.Posicion.X < luchador2.Posicion.X
                : luchador1.MiraDerecha;

            var medioAnchoIzquierdo = jugador1ALaIzquierda ? caja1.MedioAncho : caja2.MedioAncho;
            var medioAnchoDerecho = jugador1ALaIzquierda ? caja2.MedioAncho : caja1.MedioAncho;
            var xIzquierdo = jugador1ALaIzquierda ? luchador1.Posicion.X : luchador2.Posicion.X;
            var xDerecho = jugador1ALaIzquierda ? luchador2.Posicion.X : luchador1.Posicion.X;

            var separacionMinima = medioAnchoIzquierdo + medioAnchoDerecho;
            var solapamiento = separacionMinima - (xDerecho - xIzquierdo);
            var mitad = solapamiento / 2;

            xIzquierdo -= mitad;
            xDerecho += solapamiento - mitad;

            var minimoIzquierdo = configuracion.LimiteIzquierdo + medioAnchoIzquierdo;
            var maximoDerecho = configuracion.LimiteDerecho - medioAnchoDerecho;

            if (xIzquierdo < minimoIzquierdo)
            {
                xIzquierdo = minimoIzquierdo;
                xDerecho = xIzquierdo + separacionMinima;
            }

            if (xDerecho > maximoDerecho)
            {
                xDerecho = maximoDerecho;
                xIzquierdo = xDerecho - separacionMinima;
            }

            return jugador1ALaIzquierda
                ? (luchador1.ConX(xIzquierdo), luchador2.ConX(xDerecho))
                : (luchador1.ConX(xDerecho), luchador2.ConX(xIzquierdo));
        }

        /// <summary>
        /// Solo giran los luchadores en tierra: quien salta por encima del rival conserva su
        /// orientación hasta aterrizar. Con ambos en la misma X nadie gira.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) Orientar(in EstadoLuchador luchador1, in EstadoLuchador luchador2)
        {
            if (luchador1.Posicion.X == luchador2.Posicion.X)
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X < luchador2.Posicion.X;

            var orientado1 = EstadosAccion.PuedeGirar(luchador1.Accion) ? luchador1.ConOrientacion(jugador1ALaIzquierda) : luchador1;
            var orientado2 = EstadosAccion.PuedeGirar(luchador2.Accion) ? luchador2.ConOrientacion(!jugador1ALaIzquierda) : luchador2;

            return (orientado1, orientado2);
        }
    }
}
