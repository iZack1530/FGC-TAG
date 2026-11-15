using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Paso puro del combate por equipos. El duelo en pantalla lo resuelve
    /// <see cref="SimuladorCombate"/>; esta capa decide quién está en pantalla y mueve a los
    /// asistentes.
    /// </summary>
    /// <remarks>
    /// Orden de cada frame: 1) botones de equipo: pulsar llama a un asistente y mantener cambia de
    /// luchador; 2) los asistentes avanzan y golpean; 3) un paso del duelo entre los dos activos;
    /// 4) esperas y regeneración de las reservas; 5) si el duelo se detuvo, se decide el combate
    /// (equipo eliminado o tiempo agotado) o, tras la pausa de K.O., entra el siguiente luchador.
    /// </remarks>
    public static class SimuladorEquipos
    {
        public static EstadoCombateEquipos Avanzar(in EstadoCombateEquipos estado, in EntradasFrame entradas, in ConfiguracionEquipos configuracion)
        {
            var enJuego = !estado.Terminado && !estado.Duelo.RondaTerminada;

            var (luchador1, equipo1) = enJuego
                ? AtenderBotonesDeEquipo(estado.Duelo.Luchador1, estado.Equipo1, entradas.Jugador1, estado.Duelo.Luchador2.Posicion.X, configuracion)
                : (estado.Duelo.Luchador1, SinBotonesNiAsistente(estado.Equipo1, entradas.Jugador1));

            var (luchador2, equipo2) = enJuego
                ? AtenderBotonesDeEquipo(estado.Duelo.Luchador2, estado.Equipo2, entradas.Jugador2, estado.Duelo.Luchador1.Posicion.X, configuracion)
                : (estado.Duelo.Luchador2, SinBotonesNiAsistente(estado.Equipo2, entradas.Jugador2));

            var reglasDuelo = configuracion.DueloEntre(equipo1.IndiceActivo, equipo2.IndiceActivo);

            var (equipo1ConAsistente, golpeado2) = AvanzarAsistente(equipo1, configuracion.Equipo1, luchador2, reglasDuelo.Jugador2, entradas.Jugador2, reglasDuelo, configuracion);
            var (equipo2ConAsistente, golpeado1) = AvanzarAsistente(equipo2, configuracion.Equipo2, luchador1, reglasDuelo.Jugador1, entradas.Jugador1, reglasDuelo, configuracion);

            var dueloPreparado = new EstadoPartida(estado.Duelo.Frame, golpeado1, golpeado2, estado.Duelo.FramesRestantes, estado.Duelo.Resultado);
            var duelo = SimuladorCombate.Avanzar(dueloPreparado, entradas, reglasDuelo);

            var equipo1Avanzado = AvanzarReservas(equipo1ConAsistente, enJuego, reglasDuelo.VidaRecuperable);
            var equipo2Avanzado = AvanzarReservas(equipo2ConAsistente, enJuego, reglasDuelo.VidaRecuperable);

            if (estado.Terminado)
            {
                return new EstadoCombateEquipos(duelo, equipo1Avanzado, equipo2Avanzado, estado.Resultado, estado.FramesDesdeNocaut);
            }

            if (!duelo.RondaTerminada)
            {
                return new EstadoCombateEquipos(duelo, equipo1Avanzado, equipo2Avanzado, ResultadoRonda.EnCurso, 0);
            }

            var resultado = DecidirCombate(duelo, equipo1Avanzado, equipo2Avanzado, configuracion);

            if (resultado != ResultadoRonda.EnCurso)
            {
                return new EstadoCombateEquipos(duelo, equipo1Avanzado, equipo2Avanzado, resultado, 0);
            }

            var framesDesdeNocaut = estado.FramesDesdeNocaut + 1;

            return framesDesdeNocaut < configuracion.FramesPausaNocaut
                ? new EstadoCombateEquipos(duelo, equipo1Avanzado, equipo2Avanzado, ResultadoRonda.EnCurso, framesDesdeNocaut)
                : RelevarNoqueados(duelo, equipo1Avanzado, equipo2Avanzado, entradas, configuracion);
        }

        /// <summary>
        /// Con el duelo detenido no hay cambios ni asistencias: el asistente se retira y un botón
        /// que siga mantenido no cuenta hasta que se suelte.
        /// </summary>
        private static EstadoEquipo SinBotonesNiAsistente(in EstadoEquipo equipo, BotonesEntrada botones) =>
            equipo
                .ConAsistente(EstadoAsistente.Ninguno)
                .ConBotonesMantenidos(
                    InterpretacionEntradas.Tiene(botones, BotonesEntrada.Asistencia1) ? EstadoEquipo.PulsacionConsumida : 0,
                    InterpretacionEntradas.Tiene(botones, BotonesEntrada.Asistencia2) ? EstadoEquipo.PulsacionConsumida : 0);

        private static (EstadoLuchador, EstadoEquipo) AtenderBotonesDeEquipo(
            in EstadoLuchador activo,
            in EstadoEquipo equipo,
            BotonesEntrada botones,
            Fijo xRival,
            in ConfiguracionEquipos configuracion)
        {
            var (activoTrasBoton1, equipoTrasBoton1, mantenido1) = AtenderBoton(
                activo,
                equipo,
                botones,
                InterpretacionEntradas.Tiene(botones, BotonesEntrada.Asistencia1),
                equipo.FramesMantenido1,
                equipo.IndiceDelCompanero(1),
                xRival,
                configuracion);

            var (activoTrasBoton2, equipoTrasBoton2, mantenido2) = AtenderBoton(
                activoTrasBoton1,
                equipoTrasBoton1,
                botones,
                InterpretacionEntradas.Tiene(botones, BotonesEntrada.Asistencia2),
                equipo.FramesMantenido2,
                equipo.IndiceDelCompanero(2),
                xRival,
                configuracion);

            return (activoTrasBoton2, equipoTrasBoton2.ConBotonesMantenidos(mantenido1, mantenido2));
        }

        /// <summary>
        /// Un botón de asistencia: soltarlo pronto llama a ese compañero como asistente; mantenerlo
        /// los frames configurados cambia a él en cuanto el cambio sea posible.
        /// </summary>
        /// <returns>El activo, el equipo y los frames que lleva mantenido el botón tras este frame.</returns>
        private static (EstadoLuchador, EstadoEquipo, int) AtenderBoton(
            in EstadoLuchador activo,
            in EstadoEquipo equipo,
            BotonesEntrada botones,
            bool mantenido,
            int framesMantenidoAntes,
            int companero,
            Fijo xRival,
            in ConfiguracionEquipos configuracion)
        {
            if (!mantenido)
            {
                var fuePulsacionCorta = framesMantenidoAntes > 0 && framesMantenidoAntes < configuracion.FramesMantenerParaCambiar;

                return fuePulsacionCorta && PuedeLlamarAsistente(activo, equipo, companero)
                    ? (activo, LlamarAsistente(activo, equipo, companero, xRival, configuracion.FramesEsperaAsistencia), 0)
                    : (activo, equipo, 0);
            }

            if (framesMantenidoAntes == EstadoEquipo.PulsacionConsumida)
            {
                return (activo, equipo, EstadoEquipo.PulsacionConsumida);
            }

            var framesMantenido = framesMantenidoAntes + 1;

            if (framesMantenido < configuracion.FramesMantenerParaCambiar || !PuedeCambiar(activo, equipo, companero))
            {
                return (activo, equipo, framesMantenido);
            }

            var (entrante, equipoCambiado) = Cambiar(activo, equipo, companero, botones, configuracion.FramesEsperaCambio);

            return (entrante, equipoCambiado, EstadoEquipo.PulsacionConsumida);
        }

        private static bool PuedeLlamarAsistente(in EstadoLuchador activo, in EstadoEquipo equipo, int companero) =>
            !equipo.Asistente.EnPantalla &&
            equipo.FramesEsperaAsistencia <= 0 &&
            equipo.VidaEnReserva(companero) > 0 &&
            EstadosAccion.PuedePedirAsistencia(activo.Accion);

        /// <summary>El asistente aparece en el suelo, donde está el activo, mirando hacia el rival.</summary>
        private static EstadoEquipo LlamarAsistente(in EstadoLuchador activo, in EstadoEquipo equipo, int companero, Fijo xRival, int framesEspera)
        {
            var miraDerecha = xRival != activo.Posicion.X ? xRival > activo.Posicion.X : activo.MiraDerecha;
            var posicion = new VectorFijo(activo.Posicion.X, Fijo.Cero);

            return equipo
                .ConAsistente(EstadoAsistente.Llamado(companero, posicion, miraDerecha))
                .ConEsperaAsistencia(framesEspera);
        }

        /// <summary>
        /// Se cambia con control en tierra o, en mitad de un combo, interrumpiendo un ataque en
        /// tierra que conectó sin ser bloqueado. No se puede cambiar a un compañero sin vida ni
        /// al que está en pantalla como asistente.
        /// </summary>
        private static bool PuedeCambiar(in EstadoLuchador activo, in EstadoEquipo equipo, int companero) =>
            equipo.FramesEsperaCambio <= 0 &&
            !activo.EnHitstop &&
            equipo.VidaEnReserva(companero) > 0 &&
            !(equipo.Asistente.EnPantalla && equipo.Asistente.Miembro == companero) &&
            (EstadosAccion.TieneControlEnTierra(activo.Accion) || EsCambioEnCombo(activo));

        private static bool EsCambioEnCombo(in EstadoLuchador activo) =>
            EstadosAccion.EstaAtacandoEnTierra(activo.Accion) && activo.GolpeConectado && !activo.GolpeBloqueado;

        /// <summary>
        /// El que entra ocupa el sitio del que sale. En un cambio normal tarda unos frames en poder
        /// actuar; en un cambio en mitad de combo entra con control para poder seguirlo.
        /// </summary>
        private static (EstadoLuchador, EstadoEquipo) Cambiar(
            in EstadoLuchador activo,
            in EstadoEquipo equipo,
            int companero,
            BotonesEntrada botones,
            int framesEsperaCambio)
        {
            var entrante = Entrar(activo.Posicion.ConY(Fijo.Cero), activo.MiraDerecha, equipo.Reserva(companero), botones, EsCambioEnCombo(activo));

            var equipoCambiado = equipo
                .ConReserva(equipo.IndiceActivo, new VidaEnReserva(activo.Vida, activo.VidaRecuperable))
                .ConActivo(companero)
                .ConEsperaCambio(framesEsperaCambio);

            return (entrante, equipoCambiado);
        }

        /// <summary>
        /// Luchador que llega a la pantalla. Recuerda los botones ya pulsados para que mantenerlos
        /// durante el cambio no lance un ataque.
        /// </summary>
        private static EstadoLuchador Entrar(VectorFijo posicion, bool miraDerecha, VidaEnReserva reserva, BotonesEntrada botones, bool conControl)
        {
            var entrante = EstadoLuchador.EnReposo(posicion, miraDerecha, reserva.Vida)
                .ConVidaRecuperable(reserva.Recuperable)
                .ConBotonesAnteriores(botones);

            return conControl ? entrante : entrante.ConAccionNueva(AccionLuchador.Entrando);
        }

        /// <summary>
        /// El asistente llega, ejecuta el ataque de asistencia de su luchador y se marcha. Su golpe
        /// se aplica al rival en pantalla igual que uno del luchador activo: se puede bloquear,
        /// cuenta para el combo y congela al asistente durante el hitstop.
        /// </summary>
        private static (EstadoEquipo, EstadoLuchador) AvanzarAsistente(
            in EstadoEquipo equipo,
            in ParametrosEquipo parametrosEquipo,
            in EstadoLuchador rival,
            in ParametrosLuchador parametrosRival,
            BotonesEntrada botonesRival,
            in ConfiguracionCombate reglasDuelo,
            in ConfiguracionEquipos configuracion)
        {
            var asistente = equipo.Asistente;

            if (!asistente.EnPantalla)
            {
                return (equipo, rival);
            }

            if (asistente.FramesHitstop > 0)
            {
                return (equipo.ConAsistente(asistente.ConHitstop(asistente.FramesHitstop - 1)), rival);
            }

            var movimiento = parametrosEquipo.Obtener(asistente.Miembro).Movimientos.Asistencia;
            var frameDelAtaque = asistente.Frames - configuracion.FramesLlegadaAsistencia;

            if (frameDelAtaque >= movimiento.DuracionTotal)
            {
                return (equipo.ConAsistente(EstadoAsistente.Ninguno), rival);
            }

            var golpea = frameDelAtaque >= 0 &&
                !asistente.GolpeConectado &&
                movimiento.EstaActivo(frameDelAtaque) &&
                ResolucionGolpes.PuedeSerGolpeado(rival, reglasDuelo.Juggle) &&
                CajaGolpeDelAsistente(asistente, movimiento).Intersecta(parametrosRival.CajaDanio(rival));

            if (!golpea)
            {
                return (equipo.ConAsistente(asistente.UnFrameDespues()), rival);
            }

            var rivalGolpeado = ResolucionGolpes.RecibirGolpeDesde(rival, asistente.Posicion.X, asistente.MiraDerecha, botonesRival, movimiento, reglasDuelo);

            return (equipo.ConAsistente(asistente.TrasConectar(movimiento.Hitstop)), rivalGolpeado);
        }

        /// <summary>Caja de golpe del asistente en el escenario, para comprobar el golpe y para dibujarla.</summary>
        public static CajaColision CajaGolpeDelAsistente(in EstadoAsistente asistente, in DatosMovimiento movimiento) =>
            movimiento.CajaGolpe.Orientada(asistente.MiraDerecha).Desplazada(asistente.Posicion);

        /// <summary>
        /// Descuenta las esperas y, con el duelo en marcha, regenera la vida recuperable de los
        /// compañeros en reserva: más deprisa que en pantalla y sin espera.
        /// </summary>
        private static EstadoEquipo AvanzarReservas(in EstadoEquipo equipo, bool enJuego, in ReglasVidaRecuperable reglas)
        {
            var conEsperas = equipo
                .ConEsperaCambio(equipo.FramesEsperaCambio > 0 ? equipo.FramesEsperaCambio - 1 : 0)
                .ConEsperaAsistencia(equipo.FramesEsperaAsistencia > 0 ? equipo.FramesEsperaAsistencia - 1 : 0);

            if (!enJuego)
            {
                return conEsperas;
            }

            var regenerado = conEsperas;

            for (var pasos = 1; pasos < EstadoEquipo.Miembros; pasos++)
            {
                var indice = conEsperas.IndiceDelCompanero(pasos);
                var reserva = conEsperas.Reserva(indice);

                if (reserva.Vida > 0 && reserva.Recuperable > 0)
                {
                    var curacion = ReglasVidaRecuperable.Curacion(reserva.Recuperable, reglas.RegeneracionEnReservaPorFrame);
                    regenerado = regenerado.ConReserva(indice, new VidaEnReserva(reserva.Vida + curacion, reserva.Recuperable - curacion));
                }
            }

            return regenerado;
        }

        /// <summary>
        /// Con el duelo detenido: pierde el equipo cuyo luchador cayó sin compañeros con vida; con
        /// el tiempo agotado gana el equipo que conserva mayor proporción de su vida total.
        /// </summary>
        private static ResultadoRonda DecidirCombate(
            in EstadoPartida duelo,
            in EstadoEquipo equipo1,
            in EstadoEquipo equipo2,
            in ConfiguracionEquipos configuracion)
        {
            var noqueado1 = duelo.Luchador1.SinVida;
            var noqueado2 = duelo.Luchador2.SinVida;

            if (!noqueado1 && !noqueado2)
            {
                var proporcion1 = (long)equipo1.VidaTotal(duelo.Luchador1) * configuracion.Equipo2.VidaMaximaTotal;
                var proporcion2 = (long)equipo2.VidaTotal(duelo.Luchador2) * configuracion.Equipo1.VidaMaximaTotal;

                if (proporcion1 == proporcion2)
                {
                    return ResultadoRonda.Empate;
                }

                return proporcion1 > proporcion2 ? ResultadoRonda.GanaJugador1 : ResultadoRonda.GanaJugador2;
            }

            var eliminado1 = noqueado1 && equipo1.SiguienteConVida() < 0;
            var eliminado2 = noqueado2 && equipo2.SiguienteConVida() < 0;

            if (eliminado1 && eliminado2)
            {
                return ResultadoRonda.Empate;
            }

            if (eliminado1)
            {
                return ResultadoRonda.GanaJugador2;
            }

            return eliminado2 ? ResultadoRonda.GanaJugador1 : ResultadoRonda.EnCurso;
        }

        /// <summary>
        /// Entra el siguiente luchador de cada equipo noqueado y el duelo se reanuda. Quien releva
        /// aparece a la distancia de entrada del rival, hacia el lado de su equipo; si relevan los
        /// dos, se colocan a esa distancia entre sí alrededor del centro del escenario.
        /// </summary>
        private static EstadoCombateEquipos RelevarNoqueados(
            in EstadoPartida duelo,
            in EstadoEquipo equipo1,
            in EstadoEquipo equipo2,
            in EntradasFrame entradas,
            in ConfiguracionEquipos configuracion)
        {
            var releva1 = duelo.Luchador1.SinVida;
            var releva2 = duelo.Luchador2.SinVida;
            var centro = (configuracion.Reglas.LimiteIzquierdo + configuracion.Reglas.LimiteDerecho) / 2;
            var mitad = configuracion.DistanciaEntrada / 2;

            var x1 = releva2 ? centro - mitad : duelo.Luchador2.Posicion.X - configuracion.DistanciaEntrada;
            var x2 = releva1 ? centro + mitad : duelo.Luchador1.Posicion.X + configuracion.DistanciaEntrada;

            var equipo1Relevado = releva1 ? Relevar(equipo1) : equipo1;
            var equipo2Relevado = releva2 ? Relevar(equipo2) : equipo2;

            var luchador1 = releva1
                ? Entrar(EnElEscenario(x1, configuracion), true, equipo1Relevado.Reserva(equipo1Relevado.IndiceActivo), entradas.Jugador1, false)
                : duelo.Luchador1;

            var luchador2 = releva2
                ? Entrar(EnElEscenario(x2, configuracion), false, equipo2Relevado.Reserva(equipo2Relevado.IndiceActivo), entradas.Jugador2, false)
                : duelo.Luchador2;

            var dueloReanudado = new EstadoPartida(duelo.Frame, luchador1, luchador2, duelo.FramesRestantes, ResultadoRonda.EnCurso);

            return new EstadoCombateEquipos(dueloReanudado, equipo1Relevado, equipo2Relevado, ResultadoRonda.EnCurso, 0);
        }

        /// <summary>El noqueado queda sin vida en su casilla y pasa a estar activo el siguiente compañero con vida.</summary>
        private static EstadoEquipo Relevar(in EstadoEquipo equipo)
        {
            var conBaja = equipo.ConReserva(equipo.IndiceActivo, new VidaEnReserva(0, 0));

            return conBaja.ConActivo(conBaja.SiguienteConVida());
        }

        private static VectorFijo EnElEscenario(Fijo x, in ConfiguracionEquipos configuracion) =>
            new VectorFijo(Fijo.Limitar(x, configuracion.Reglas.LimiteIzquierdo, configuracion.Reglas.LimiteDerecho), Fijo.Cero);
    }
}
