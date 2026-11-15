using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Paso puro del combate por equipos. El duelo en pantalla lo resuelve
    /// <see cref="SimuladorCombate"/>; esta capa decide quién está en pantalla.
    /// </summary>
    /// <remarks>
    /// Orden de cada frame: 1) cambios de luchador pedidos por los jugadores; 2) un paso del duelo
    /// entre los dos activos; 3) si el duelo se detuvo, se decide el combate (equipo eliminado o
    /// tiempo agotado) o, tras la pausa de K.O., entra el siguiente luchador y el duelo continúa.
    /// </remarks>
    public static class SimuladorEquipos
    {
        public static EstadoCombateEquipos Avanzar(in EstadoCombateEquipos estado, in EntradasFrame entradas, in ConfiguracionEquipos configuracion)
        {
            var puedenCambiar = !estado.Terminado && !estado.Duelo.RondaTerminada;

            var (luchador1, equipo1) = puedenCambiar
                ? IntentarCambio(estado.Duelo.Luchador1, estado.Equipo1, entradas.Jugador1, configuracion.FramesEsperaCambio)
                : (estado.Duelo.Luchador1, estado.Equipo1);

            var (luchador2, equipo2) = puedenCambiar
                ? IntentarCambio(estado.Duelo.Luchador2, estado.Equipo2, entradas.Jugador2, configuracion.FramesEsperaCambio)
                : (estado.Duelo.Luchador2, estado.Equipo2);

            var dueloConCambios = new EstadoPartida(estado.Duelo.Frame, luchador1, luchador2, estado.Duelo.FramesRestantes, estado.Duelo.Resultado);
            var reglasDuelo = configuracion.DueloEntre(equipo1.IndiceActivo, equipo2.IndiceActivo);
            var duelo = SimuladorCombate.Avanzar(dueloConCambios, entradas, reglasDuelo);

            var equipo1Avanzado = DescontarEspera(equipo1);
            var equipo2Avanzado = DescontarEspera(equipo2);

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

        private static EstadoEquipo DescontarEspera(in EstadoEquipo equipo) =>
            equipo.FramesEsperaCambio > 0 ? equipo.ConEsperaCambio(equipo.FramesEsperaCambio - 1) : equipo;

        /// <summary>
        /// Cambio de luchador: Asistencia 1 llama al siguiente compañero y Asistencia 2 al otro.
        /// Solo se puede con control en tierra, sin espera pendiente y si el compañero tiene vida.
        /// El que entra ocupa el sitio del que sale y tarda unos frames en poder actuar.
        /// </summary>
        private static (EstadoLuchador, EstadoEquipo) IntentarCambio(
            in EstadoLuchador activo,
            in EstadoEquipo equipo,
            BotonesEntrada botones,
            int framesEsperaCambio)
        {
            if (equipo.FramesEsperaCambio > 0 || activo.EnHitstop || !EstadosAccion.TieneControlEnTierra(activo.Accion))
            {
                return (activo, equipo);
            }

            var recienPulsados = botones & ~activo.BotonesAnteriores;
            var pasos = InterpretacionEntradas.Tiene(recienPulsados, BotonesEntrada.Asistencia1)
                ? 1
                : InterpretacionEntradas.Tiene(recienPulsados, BotonesEntrada.Asistencia2) ? 2 : 0;

            if (pasos == 0)
            {
                return (activo, equipo);
            }

            var destino = equipo.IndiceDelCompanero(pasos);

            if (equipo.VidaEnReserva(destino) <= 0)
            {
                return (activo, equipo);
            }

            var entrante = Entrar(activo.Posicion, activo.MiraDerecha, equipo.VidaEnReserva(destino), botones);

            var equipoCambiado = equipo
                .ConVidaEnReserva(equipo.IndiceActivo, activo.Vida)
                .ConActivo(destino)
                .ConEsperaCambio(framesEsperaCambio);

            return (entrante, equipoCambiado);
        }

        /// <summary>
        /// Luchador que llega a la pantalla. Recuerda los botones ya pulsados para que mantenerlos
        /// durante el cambio no lance un ataque.
        /// </summary>
        private static EstadoLuchador Entrar(VectorFijo posicion, bool miraDerecha, int vida, BotonesEntrada botones) =>
            EstadoLuchador.EnReposo(posicion, miraDerecha, vida)
                .ConAccionNueva(AccionLuchador.Entrando)
                .ConBotonesAnteriores(botones);

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
                ? Entrar(EnElEscenario(x1, configuracion), true, equipo1Relevado.VidaEnReserva(equipo1Relevado.IndiceActivo), entradas.Jugador1)
                : duelo.Luchador1;

            var luchador2 = releva2
                ? Entrar(EnElEscenario(x2, configuracion), false, equipo2Relevado.VidaEnReserva(equipo2Relevado.IndiceActivo), entradas.Jugador2)
                : duelo.Luchador2;

            var dueloReanudado = new EstadoPartida(duelo.Frame, luchador1, luchador2, duelo.FramesRestantes, ResultadoRonda.EnCurso);

            return new EstadoCombateEquipos(dueloReanudado, equipo1Relevado, equipo2Relevado, ResultadoRonda.EnCurso, 0);
        }

        /// <summary>El noqueado queda con 0 de vida en su casilla y pasa a estar activo el siguiente compañero con vida.</summary>
        private static EstadoEquipo Relevar(in EstadoEquipo equipo) =>
            equipo.ConVidaEnReserva(equipo.IndiceActivo, 0).ConActivo(equipo.ConVidaEnReserva(equipo.IndiceActivo, 0).SiguienteConVida());

        private static VectorFijo EnElEscenario(Fijo x, in ConfiguracionEquipos configuracion) =>
            new VectorFijo(Fijo.Limitar(x, configuracion.Reglas.LimiteIzquierdo, configuracion.Reglas.LimiteDerecho), Fijo.Cero);
    }
}
