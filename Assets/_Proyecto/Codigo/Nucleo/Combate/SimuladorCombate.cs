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
    /// (caja de golpe contra caja de daño) y después los agarres; 5) los luchadores que pueden girar se orientan hacia
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
                AvanzarLuchador(estado.Luchador1, entradas.Jugador1, estado.Luchador2, configuracion.Jugador1, configuracion),
                configuracion.Jugador1,
                configuracion);

            var luchador2 = LimitarAlEscenario(
                AvanzarLuchador(estado.Luchador2, entradas.Jugador2, estado.Luchador1, configuracion.Jugador2, configuracion),
                configuracion.Jugador2,
                configuracion);

            var (empujado1, empujado2) = ResolverEmpuje(luchador1, luchador2, configuracion);
            var (golpeado1, golpeado2) = estado.RondaTerminada
                ? (empujado1, empujado2)
                : ResolucionGolpes.Resolver(empujado1, empujado2, entradas, configuracion);
            var (agarrado1, agarrado2) = estado.RondaTerminada
                ? (golpeado1, golpeado2)
                : ResolucionAgarres.Resolver(golpeado1, golpeado2, entradas, configuracion);
            var (orientado1, orientado2) = Orientar(agarrado1, agarrado2);

            if (estado.RondaTerminada)
            {
                return new EstadoPartida(estado.Frame + 1, orientado1, orientado2, estado.FramesRestantes, estado.Resultado);
            }

            var regenerado1 = Regenerar(orientado1, configuracion.VidaRecuperable);
            var regenerado2 = Regenerar(orientado2, configuracion.VidaRecuperable);

            var framesRestantes = estado.FramesRestantes > 0 ? estado.FramesRestantes - 1 : 0;
            var resultado = DecidirRonda(regenerado1, regenerado2, framesRestantes, configuracion);

            return new EstadoPartida(estado.Frame + 1, regenerado1, regenerado2, framesRestantes, resultado);
        }

        /// <summary>
        /// Vida recuperable del luchador en pantalla: tras unos frames sin recibir golpes, cada
        /// frame convierte un poco de vida azul en vida real.
        /// </summary>
        private static EstadoLuchador Regenerar(in EstadoLuchador luchador, in ReglasVidaRecuperable reglas)
        {
            if (luchador.FramesSinRecibirGolpe < reglas.FramesEsperaRegeneracion)
            {
                return luchador.ConFramesSinRecibirGolpe(luchador.FramesSinRecibirGolpe + 1);
            }

            if (luchador.SinVida || luchador.VidaRecuperable <= 0)
            {
                return luchador;
            }

            var curacion = ReglasVidaRecuperable.Curacion(luchador.VidaRecuperable, reglas.RegeneracionPorFrame);

            return luchador.ConVida(luchador.Vida + curacion).ConVidaRecuperable(luchador.VidaRecuperable - curacion);
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
            in EstadoLuchador rival,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            var conBufer = ActualizarBufer(luchador, botones, configuracion.FramesBuferEntradas);

            var avanzado = conBufer.EnHitstop
                ? conBufer.ConHitstop(conBufer.FramesHitstop - 1)
                : ContarFramesDesdeAgachado(ConBarraAlEjecutar(
                    AlEmpezarUnAtaque(Actuar(conBufer, botones, SentidoHacia(luchador, rival), parametros, configuracion), luchador, botones, rival, parametros, configuracion),
                    parametros,
                    configuracion.Barra));

            return ContarProteccionDeAgarre(avanzado, configuracion.Agarres).ConBotonesAnteriores(botones);
        }

        /// <summary>Sentido horizontal (+1 o -1) en el que está el rival; si están en la misma X, hacia donde mira el luchador.</summary>
        private static int SentidoHacia(in EstadoLuchador luchador, in EstadoLuchador rival) =>
            rival.Posicion.X != luchador.Posicion.X
                ? (rival.Posicion.X > luchador.Posicion.X ? 1 : -1)
                : luchador.SentidoAdelante;

        /// <summary>
        /// Ejecutar un ataque da un poco de barra cuando llega a su primer frame activo, conecte o
        /// no. Un ataque cancelado antes (por ejemplo, con un plink dash) no da nada.
        /// </summary>
        private static EstadoLuchador ConBarraAlEjecutar(in EstadoLuchador luchador, in ParametrosLuchador parametros, in ReglasBarra barra)
        {
            if (EstadosAccion.EstaAtacando(luchador.Accion))
            {
                var movimiento = parametros.MovimientoEnCurso(luchador);

                return luchador.FramesEnAccion == movimiento.Arranque ? luchador.ConBarra(barra.AlEjecutar(luchador.Barra, movimiento.Danio)) : luchador;
            }

            if (luchador.Accion == AccionLuchador.IntentandoAgarre)
            {
                var agarre = parametros.AgarreEnCurso(luchador);

                return luchador.FramesEnAccion == agarre.Arranque ? luchador.ConBarra(barra.AlEjecutar(luchador.Barra, agarre.Danio)) : luchador;
            }

            return luchador;
        }

        /// <summary>
        /// En el frame en que empieza un ataque: una dirección más el golpe fuerte, cerca de un
        /// rival en tierra, se convierte en agarre.
        /// </summary>
        private static EstadoLuchador AlEmpezarUnAtaque(
            in EstadoLuchador luchador,
            in EstadoLuchador antes,
            BotonesEntrada botones,
            in EstadoLuchador rival,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            if (!EstadosAccion.EstaAtacando(luchador.Accion) || luchador.FramesEnAccion != 0)
            {
                return luchador;
            }

            var esCancelacion = EstadosAccion.EstaAtacando(antes.Accion) &&
                antes.FramesEnAccion + 1 < parametros.MovimientoEnCurso(antes).DuracionTotal;

            if (esCancelacion || !EsAgarre(luchador, antes, botones, rival, configuracion.Agarres))
            {
                return luchador;
            }

            var haciaAtras = InterpretacionEntradas.DireccionHorizontal(botones) != luchador.SentidoAdelante;

            return SinAtaque(luchador)
                .ConVelocidad(VectorFijo.Cero)
                .ConAccionNueva(AccionLuchador.IntentandoAgarre)
                .ConAgarreHaciaAtras(haciaAtras);
        }

        /// <summary>
        /// El golpe fuerte de pie es un agarre si se pulsa manteniendo adelante o atrás, con el
        /// rival en tierra y a la distancia de activación. Cancelar otro ataque nunca da un agarre.
        /// </summary>
        private static bool EsAgarre(in EstadoLuchador luchador, in EstadoLuchador antes, BotonesEntrada botones, in EstadoLuchador rival, in ReglasAgarre reglas) =>
            luchador.Accion == AccionLuchador.Atacando &&
            luchador.Ataque == TipoAtaque.Fuerte &&
            InterpretacionEntradas.DireccionHorizontal(botones) != 0 &&
            !EstadosAccion.EstaEnElAire(rival.Accion) &&
            Fijo.Abs(rival.Posicion.X - antes.Posicion.X) <= reglas.DistanciaActivacion;

        /// <summary>Mientras dura un aturdimiento la protección contra agarres está completa; después se descuenta.</summary>
        private static EstadoLuchador ContarProteccionDeAgarre(in EstadoLuchador luchador, in ReglasAgarre reglas)
        {
            if (EstadosAccion.ProtegeDeAgarres(luchador.Accion))
            {
                return luchador.ConProteccionAgarre(reglas.FramesProteccion);
            }

            return luchador.FramesProteccionAgarre > 0 ? luchador.ConProteccionAgarre(luchador.FramesProteccionAgarre - 1) : luchador;
        }

        private static EstadoLuchador ContarFramesDesdeAgachado(in EstadoLuchador luchador)
        {
            if (luchador.Accion == AccionLuchador.Agachado)
            {
                return luchador.ConFramesDesdeAgachado(0);
            }

            return luchador.FramesDesdeAgachado >= EstadoLuchador.MaximoFramesDesdeAgachado
                ? luchador
                : luchador.ConFramesDesdeAgachado(luchador.FramesDesdeAgachado + 1);
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

        /// <summary>
        /// En tierra, un ataque sale agachado si el jugador mantiene abajo en el frame en que
        /// empieza. El lanzador es el mismo de pie y agachado.
        /// </summary>
        private static EstadoLuchador IniciarAtaqueEnTierra(in EstadoLuchador luchador, BotonesEntrada botones)
        {
            var agachado = luchador.AtaqueEnBufer != TipoAtaque.Especial && InterpretacionEntradas.DireccionVertical(botones) < 0;

            return IniciarAtaque(luchador, agachado ? AccionLuchador.AtacandoAgachado : AccionLuchador.Atacando);
        }

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
            if (EsPlinkDash(luchador, botones, parametros.Dash))
            {
                return IniciarDash(luchador, botones, parametros.Dash);
            }

            if (PuedeCancelar(luchador))
            {
                return IniciarAtaqueEnTierra(luchador, botones);
            }

            var movimiento = parametros.MovimientoEnCurso(luchador);

            var golpeoSinBloqueo = luchador.GolpeConectado && !luchador.GolpeBloqueado;

            if (golpeoSinBloqueo && movimiento.CancelableConSalto && InterpretacionEntradas.DireccionVertical(botones) > 0)
            {
                return SinAtaque(luchador).ConAccionNueva(AccionLuchador.PreSalto).ConSupersalto(false);
            }

            return luchador.FramesEnAccion + 1 >= movimiento.DuracionTotal
                ? ControlarEnTierra(SinAtaque(luchador).ConVelocidad(VectorFijo.Cero), botones, parametros)
                : Impulsado(luchador.ConAccion(luchador.Accion), parametros.Dash.Friccion);
        }

        private static EstadoLuchador Actuar(
            in EstadoLuchador luchador,
            BotonesEntrada botones,
            int sentidoRival,
            in ParametrosLuchador parametros,
            in ConfiguracionCombate configuracion)
        {
            switch (luchador.Accion)
            {
                case AccionLuchador.Atacando:
                case AccionLuchador.AtacandoAgachado:
                    return ContinuarAtaqueEnTierra(luchador, botones, parametros);

                case AccionLuchador.Hitstun:
                case AccionLuchador.Blockstun:
                case AccionLuchador.BlockstunAgachado:
                    return luchador.FramesAturdimiento <= 0
                        ? ControlarEnTierra(luchador.ConVelocidad(VectorFijo.Cero).ConGolpesEnCombo(0), botones, parametros)
                        : Deslizar(luchador, configuracion.FriccionRetroceso).ConAturdimiento(luchador.FramesAturdimiento - 1);

                case AccionLuchador.Noqueado:
                    return Deslizar(luchador, configuracion.FriccionRetroceso);

                case AccionLuchador.EnAire when QuiereDashAereo(luchador, botones, parametros.Dash):
                    return IniciarDashAereo(luchador, botones, sentidoRival, parametros.Dash);

                case AccionLuchador.DashAereo:
                    return ContinuarDashAereo(luchador, parametros);

                case AccionLuchador.Dash:
                    return ContinuarDash(luchador, botones, parametros);

                case AccionLuchador.AgarreRoto:
                    return luchador.FramesEnAccion + 1 >= configuracion.Agarres.FramesAgarreRoto
                        ? ControlarEnTierra(luchador.ConVelocidad(VectorFijo.Cero), botones, parametros)
                        : Deslizar(luchador, configuracion.FriccionRetroceso);

                case AccionLuchador.EnAire:
                case AccionLuchador.AtacandoEnAire:
                    return MoverEnAire(DecidirAccionAerea(luchador, parametros), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);

                case AccionLuchador.GolpeadoEnAire:
                    if (luchador.FramesAturdimiento <= 0 && !luchador.SinVida)
                    {
                        return MoverEnAire(RecuperarEnAire(luchador, botones, configuracion.Juggle), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);
                    }

                    return MoverEnAire(
                        luchador.ConAccion(AccionLuchador.GolpeadoEnAire).ConAturdimiento(luchador.FramesAturdimiento > 0 ? luchador.FramesAturdimiento - 1 : 0),
                        configuracion.Juggle.GravedadCon(luchador.GolpesEnCombo),
                        parametros.Salto.FramesAterrizaje);

                case AccionLuchador.RecuperandoEnAire:
                    return MoverEnAire(
                        luchador.FramesEnAccion + 1 >= configuracion.Juggle.FramesRecuperacion
                            ? luchador.ConAccionNueva(AccionLuchador.EnAire)
                            : luchador.ConAccion(AccionLuchador.RecuperandoEnAire),
                        parametros.Salto.Gravedad,
                        parametros.Salto.FramesAterrizaje);

                case AccionLuchador.BloqueandoEnAire:
                    return MoverEnAire(
                        luchador.FramesAturdimiento <= 0
                            ? luchador.ConAccionNueva(AccionLuchador.EnAire)
                            : luchador.ConAccion(AccionLuchador.BloqueandoEnAire).ConAturdimiento(luchador.FramesAturdimiento - 1),
                        parametros.Salto.Gravedad,
                        parametros.Salto.FramesAterrizaje);

                case AccionLuchador.Derribado:
                    return luchador.FramesEnAccion + 1 >= configuracion.Juggle.FramesDerribado
                        ? ControlarEnTierra(luchador.ConGolpesEnCombo(0), botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Derribado);

                case AccionLuchador.PreSalto:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesPreSalto
                        ? Despegar(luchador, botones, parametros.Salto)
                        : luchador.ConAccion(AccionLuchador.PreSalto);

                case AccionLuchador.Entrando:
                    return MoverEnAire(luchador.ConAccion(AccionLuchador.Entrando), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);

                case AccionLuchador.IntentandoAgarre when EsPlinkDash(luchador, botones, parametros.Dash):
                    return IniciarDash(luchador, botones, parametros.Dash);

                case AccionLuchador.IntentandoAgarre:
                    return luchador.FramesEnAccion + 1 >= parametros.AgarreEnCurso(luchador).DuracionAlFallar
                        ? ControlarEnTierra(luchador, botones, parametros)
                        : luchador.ConAccion(AccionLuchador.IntentandoAgarre);

                case AccionLuchador.Agarrando:
                    return luchador.FramesEnAccion + 1 >= parametros.AgarreEnCurso(luchador).DuracionAlAgarrar
                        ? ControlarEnTierra(luchador, botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Agarrando);

                case AccionLuchador.Agarrado:
                    return luchador.ConAccion(AccionLuchador.Agarrado);

                case AccionLuchador.Aterrizaje:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesAterrizaje
                        ? ControlarEnTierra(luchador, botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Aterrizaje);

                default:
                    return ControlarEnTierra(luchador, botones, parametros);
            }
        }

        /// <summary>
        /// Comando de dash con control en tierra: dos botones de golpe mantenidos y uno de ellos
        /// pulsado hace poco (está en el búfer), sin mantener abajo.
        /// </summary>
        private static bool QuiereDash(in EstadoLuchador luchador, BotonesEntrada botones) =>
            luchador.TieneAtaqueEnBufer &&
            LecturaAtaques.EsGolpe(luchador.AtaqueEnBufer) &&
            LecturaAtaques.DosGolpesMantenidos(botones) &&
            InterpretacionEntradas.DireccionVertical(botones) >= 0;

        /// <summary>
        /// Plink dash: en los primeros frames de un ataque en tierra que aún no conectó, pulsar
        /// otro botón de golpe sin soltar el primero cambia el ataque por un dash.
        /// </summary>
        private static bool EsPlinkDash(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosDash dash) =>
            luchador.FramesEnAccion < dash.VentanaPlink &&
            !luchador.GolpeConectado &&
            LecturaAtaques.EsGolpe(LecturaAtaques.RecienPulsado(botones, luchador.BotonesAnteriores)) &&
            LecturaAtaques.DosGolpesMantenidos(botones) &&
            InterpretacionEntradas.DireccionVertical(botones) >= 0;

        /// <summary>El dash va hacia donde mira el luchador (hacia el rival) o, si mantiene atrás, en sentido contrario.</summary>
        private static EstadoLuchador IniciarDash(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosDash dash)
        {
            var haciaAtras = InterpretacionEntradas.DireccionHorizontal(botones) == -luchador.SentidoAdelante;
            var velocidad = haciaAtras ? dash.VelocidadAtras * -luchador.SentidoAdelante : dash.VelocidadAdelante * luchador.SentidoAdelante;

            var enDash = SinAtaque(luchador)
                .ConBufer(TipoAtaque.Ninguno, 0)
                .ConVelocidad(new VectorFijo(velocidad, Fijo.Cero))
                .ConAccionNueva(AccionLuchador.Dash);

            return Impulsado(enDash, dash.Friccion);
        }

        /// <summary>
        /// Un frame de impulso en el suelo: el luchador avanza con su velocidad horizontal y la
        /// fricción la reduce. No cambia su acción, así que sirve para el dash y para el ataque
        /// que hereda su impulso.
        /// </summary>
        private static EstadoLuchador Impulsado(in EstadoLuchador luchador, Fijo friccion)
        {
            var velocidadX = luchador.Velocidad.X;
            var frenada = Fijo.Max(Fijo.Abs(velocidadX) - friccion, Fijo.Cero) * Fijo.Signo(velocidadX);

            return luchador
                .ConX(luchador.Posicion.X + velocidadX)
                .ConVelocidad(new VectorFijo(frenada, Fijo.Cero));
        }

        /// <summary>
        /// El dash se desliza perdiendo velocidad. Se cancela pronto con un ataque, que hereda el
        /// impulso que le quede y sigue deslizándose, y un poco más
        /// tarde, manteniendo abajo (queda agachado: la base del wavedash) o saltando. Como el
        /// dash no empieza con abajo mantenido, para repetirlo hay que soltar abajo.
        /// </summary>
        private static EstadoLuchador ContinuarDash(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            var dash = parametros.Dash;
            var frames = luchador.FramesEnAccion + 1;
            var parado = luchador.ConVelocidad(VectorFijo.Cero);

            if (frames >= dash.FramesDash)
            {
                return ControlarEnTierra(parado, botones, parametros);
            }

            if (frames >= dash.FramesMinimosParaAtacar && luchador.TieneAtaqueEnBufer)
            {
                return Impulsado(IniciarAtaqueEnTierra(luchador, botones), dash.Friccion);
            }

            if (frames >= dash.FramesMinimosParaCancelar)
            {
                if (InterpretacionEntradas.DireccionVertical(botones) < 0)
                {
                    return parado.ConAccionNueva(AccionLuchador.Agachado);
                }

                if (InterpretacionEntradas.DireccionVertical(botones) > 0)
                {
                    return parado.ConAccionNueva(AccionLuchador.PreSalto).ConSupersalto(false);
                }
            }

            return Impulsado(luchador.ConAccion(AccionLuchador.Dash), dash.Friccion);
        }

        /// <summary>Dash aéreo: durante un salto libre, una vez por salto y por encima de la altura mínima.</summary>
        private static bool QuiereDashAereo(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosDash dash) =>
            !luchador.DashAereoUsado &&
            luchador.TieneAtaqueEnBufer &&
            LecturaAtaques.EsGolpe(luchador.AtaqueEnBufer) &&
            LecturaAtaques.DosGolpesMantenidos(botones) &&
            luchador.Posicion.Y >= dash.AlturaMinimaDashAereo;

        /// <summary>El dash aéreo va hacia el rival o, si se mantiene la dirección contraria, se aleja de él.</summary>
        private static EstadoLuchador IniciarDashAereo(in EstadoLuchador luchador, BotonesEntrada botones, int sentidoRival, in ParametrosDash dash)
        {
            var sentido = InterpretacionEntradas.DireccionHorizontal(botones) == -sentidoRival ? -sentidoRival : sentidoRival;
            var velocidad = new VectorFijo(dash.VelocidadDashAereo * sentido, Fijo.Cero);

            return luchador
                .ConBufer(TipoAtaque.Ninguno, 0)
                .ConDashAereoUsado(true)
                .ConVelocidad(velocidad)
                .ConPosicion(luchador.Posicion + velocidad)
                .ConAccionNueva(AccionLuchador.DashAereo);
        }

        /// <summary>
        /// Avanza en horizontal sin caer. Pasados sus frames mínimos se cancela con un ataque
        /// aéreo, que conserva el impulso; al terminar, el luchador sigue cayendo con ese impulso.
        /// </summary>
        private static EstadoLuchador ContinuarDashAereo(in EstadoLuchador luchador, in ParametrosLuchador parametros)
        {
            var dash = parametros.Dash;
            var frames = luchador.FramesEnAccion + 1;

            if (frames >= dash.FramesDashAereo)
            {
                return MoverEnAire(luchador.ConAccionNueva(AccionLuchador.EnAire), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);
            }

            if (frames >= dash.FramesMinimosDashAereo && TieneAtaqueAereoEnBufer(luchador))
            {
                return MoverEnAire(IniciarAtaque(luchador, AccionLuchador.AtacandoEnAire), parametros.Salto.Gravedad, parametros.Salto.FramesAterrizaje);
            }

            return luchador.ConAccion(AccionLuchador.DashAereo).ConPosicion(luchador.Posicion + luchador.Velocidad);
        }

        private static EstadoLuchador ControlarEnTierra(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            if (QuiereDash(luchador, botones))
            {
                return IniciarDash(luchador, botones, parametros.Dash);
            }

            if (luchador.TieneAtaqueEnBufer)
            {
                return IniciarAtaqueEnTierra(luchador, botones);
            }

            var vertical = InterpretacionEntradas.DireccionVertical(botones);

            if (vertical > 0)
            {
                var vieneDeAgacharse = luchador.Accion == AccionLuchador.Agachado ||
                    luchador.FramesDesdeAgachado <= parametros.Salto.VentanaSupersalto;

                return luchador.ConAccion(AccionLuchador.PreSalto).ConSupersalto(vieneDeAgacharse);
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

        /// <summary>
        /// Recuperación en el aire: al agotarse el aturdimiento, el combo termina y el luchador da
        /// un pequeño salto hacia la dirección que mantenga. Mientras dura la recuperación no puede
        /// atacar; sus primeros frames son invencibles y en el resto ya puede bloquear.
        /// </summary>
        private static EstadoLuchador RecuperarEnAire(in EstadoLuchador luchador, BotonesEntrada botones, in ReglasJuggle juggle)
        {
            var direccion = InterpretacionEntradas.DireccionHorizontal(botones);

            return luchador
                .ConAccionNueva(AccionLuchador.RecuperandoEnAire)
                .ConGolpesEnCombo(0)
                .ConVelocidad(new VectorFijo(juggle.VelocidadRecuperacion * direccion, juggle.ImpulsoRecuperacion));
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
                .ConVelocidad(new VectorFijo(velocidadHorizontal, luchador.Supersalto ? salto.VelocidadVerticalSupersalto : salto.VelocidadVertical));
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

            var enReposo = SinAtaque(luchador).ConVelocidad(VectorFijo.Cero).ConAturdimiento(0).ConSupersalto(false).ConDashAereoUsado(false);

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
