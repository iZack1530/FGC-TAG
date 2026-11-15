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
                : ResolverGolpes(empujado1, empujado2, entradas, configuracion);
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

        private static EstadoLuchador IniciarAtaque(in EstadoLuchador luchador, TipoAtaque ataque) =>
            luchador
                .ConAccionNueva(AccionLuchador.Atacando)
                .ConAtaque(ataque)
                .ConGolpeConectado(false)
                .ConBufer(TipoAtaque.Ninguno, 0);

        /// <summary>
        /// Cancelación: un ataque que ya conectó (golpeado o bloqueado) se interrumpe para lanzar
        /// otro más fuerte guardado en el búfer. Un ataque que falla no se puede cancelar.
        /// </summary>
        private static bool PuedeCancelar(in EstadoLuchador luchador) =>
            luchador.GolpeConectado && luchador.TieneAtaqueEnBufer && luchador.AtaqueEnBufer > luchador.Ataque;

        private static EstadoLuchador Actuar(
            in EstadoLuchador luchador,
            BotonesEntrada botones,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            switch (luchador.Accion)
            {
                case AccionLuchador.Atacando:
                    if (PuedeCancelar(luchador))
                    {
                        return IniciarAtaque(luchador, luchador.AtaqueEnBufer);
                    }

                    return luchador.FramesEnAccion + 1 >= parametros.Movimiento(luchador.Ataque).DuracionTotal
                        ? ControlarEnTierra(luchador.ConGolpeConectado(false).ConAtaque(TipoAtaque.Ninguno), botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Atacando);

                case AccionLuchador.Hitstun:
                case AccionLuchador.Blockstun:
                    return luchador.FramesAturdimiento <= 0
                        ? ControlarEnTierra(luchador.ConVelocidad(VectorFijo.Cero).ConGolpesEnCombo(0), botones, parametros)
                        : Deslizar(luchador, configuracion.FriccionRetroceso).ConAturdimiento(luchador.FramesAturdimiento - 1);

                case AccionLuchador.Noqueado:
                    return Deslizar(luchador, configuracion.FriccionRetroceso);

                case AccionLuchador.EnAire:
                    return MoverEnAire(luchador, parametros.Salto);

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
                return IniciarAtaque(luchador, luchador.AtaqueEnBufer);
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
                .ConAccion(AccionLuchador.EnAire)
                .ConVelocidad(new VectorFijo(velocidadHorizontal, salto.VelocidadVertical));
        }

        /// <summary>Integración explícita: primero se mueve con la velocidad actual y después la gravedad la reduce.</summary>
        private static EstadoLuchador MoverEnAire(in EstadoLuchador luchador, in ParametrosSalto salto)
        {
            var posicion = luchador.Posicion + luchador.Velocidad;

            if (posicion.Y <= Fijo.Cero)
            {
                var accionAlCaer = luchador.SinVida
                    ? AccionLuchador.Noqueado
                    : salto.FramesAterrizaje > 0 ? AccionLuchador.Aterrizaje : AccionLuchador.Quieto;

                return luchador
                    .ConPosicion(posicion.ConY(Fijo.Cero))
                    .ConVelocidad(luchador.SinVida ? luchador.Velocidad.ConY(Fijo.Cero) : VectorFijo.Cero)
                    .ConAccionNueva(accionAlCaer);
            }

            var velocidad = luchador.Velocidad.ConY(luchador.Velocidad.Y - salto.Gravedad);

            return luchador
                .ConAccion(AccionLuchador.EnAire)
                .ConPosicion(posicion)
                .ConVelocidad(velocidad);
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
        /// Comprueba la caja de golpe de cada atacante contra la caja de daño del rival. Los dos
        /// golpes se evalúan sobre el mismo estado, así que dos ataques simultáneos se intercambian.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) ResolverGolpes(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in EntradasFrame entradas,
            in ConfiguracionCombate configuracion)
        {
            var golpea1 = Conecta(luchador1, configuracion.Jugador1, luchador2, configuracion.Jugador2);
            var golpea2 = Conecta(luchador2, configuracion.Jugador2, luchador1, configuracion.Jugador1);

            var resultado1 = golpea2
                ? RecibirGolpe(luchador1, luchador2, entradas.Jugador1, configuracion.Jugador2.Movimiento(luchador2.Ataque), configuracion.Danio)
                : golpea1 ? MarcarGolpeConectado(luchador1, configuracion.Jugador1.Movimiento(luchador1.Ataque)) : luchador1;

            var resultado2 = golpea1
                ? RecibirGolpe(luchador2, luchador1, entradas.Jugador2, configuracion.Jugador1.Movimiento(luchador1.Ataque), configuracion.Danio)
                : golpea2 ? MarcarGolpeConectado(luchador2, configuracion.Jugador2.Movimiento(luchador2.Ataque)) : luchador2;

            return (resultado1, resultado2);
        }

        private static bool Conecta(
            in EstadoLuchador atacante,
            in ParametrosLuchador parametrosAtacante,
            in EstadoLuchador defensor,
            in ParametrosLuchador parametrosDefensor)
        {
            if (atacante.GolpeConectado)
            {
                return false;
            }

            var cajaGolpe = parametrosAtacante.CajaGolpeActiva(atacante);

            return cajaGolpe.HasValue && cajaGolpe.Value.Intersecta(parametrosDefensor.CajaDanio(defensor));
        }

        private static EstadoLuchador MarcarGolpeConectado(in EstadoLuchador atacante, in DatosMovimiento movimiento) =>
            atacante.ConGolpeConectado(true).ConHitstop(movimiento.Hitstop);

        /// <summary>
        /// Se bloquea manteniendo la dirección que aleja del atacante, con control en tierra o
        /// durante otro blockstun. En el aire todavía no hay bloqueo: el golpe corta el ascenso y
        /// empuja al rival, que cae y aterriza con normalidad.
        /// </summary>
        private static EstadoLuchador RecibirGolpe(
            in EstadoLuchador defensor,
            in EstadoLuchador atacante,
            BotonesEntrada botonesDefensor,
            in DatosMovimiento movimiento,
            in ReglasDanio reglasDanio)
        {
            var sentidoRetroceso = defensor.Posicion.X != atacante.Posicion.X
                ? (defensor.Posicion.X > atacante.Posicion.X ? 1 : -1)
                : atacante.SentidoAdelante;

            var congelado = defensor.ConHitstop(movimiento.Hitstop).ConGolpeConectado(false).ConAtaque(TipoAtaque.Ninguno);

            if (defensor.Accion == AccionLuchador.EnAire)
            {
                var velocidadCaida = Fijo.Min(defensor.Velocidad.Y, Fijo.Cero);

                return congelado
                    .ConVida(VidaTrasGolpe(defensor, reglasDanio.Escalar(movimiento.Danio, 1)))
                    .ConVelocidad(new VectorFijo(movimiento.RetrocesoAlGolpear * sentidoRetroceso, velocidadCaida));
            }

            var bloquea = EstadosAccion.PuedeBloquear(defensor.Accion) &&
                InterpretacionEntradas.DireccionHorizontal(botonesDefensor) == sentidoRetroceso;

            var retroceso = bloquea ? movimiento.RetrocesoAlBloquear : movimiento.RetrocesoAlGolpear;

            var golpesEnCombo = bloquea ? 0 : (defensor.Accion == AccionLuchador.Hitstun ? defensor.GolpesEnCombo + 1 : 1);

            var empujado = congelado
                .ConGolpesEnCombo(golpesEnCombo)
                .ConVelocidad(new VectorFijo(retroceso * sentidoRetroceso, Fijo.Cero));

            if (bloquea)
            {
                return empujado.ConAccionNueva(AccionLuchador.Blockstun).ConAturdimiento(movimiento.Blockstun);
            }

            var vida = VidaTrasGolpe(defensor, reglasDanio.Escalar(movimiento.Danio, golpesEnCombo));

            return vida > 0
                ? empujado.ConVida(vida).ConAccionNueva(AccionLuchador.Hitstun).ConAturdimiento(movimiento.Hitstun)
                : empujado.ConVida(0).ConAccionNueva(AccionLuchador.Noqueado).ConAturdimiento(0);
        }

        private static int VidaTrasGolpe(in EstadoLuchador defensor, int danio) =>
            defensor.Vida > danio ? defensor.Vida - danio : 0;

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
