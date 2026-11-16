using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;

namespace FgcTag.Nucleo.Encuentros
{
    /// <summary>
    /// Paso puro del encuentro por rondas. Cada ronda es un combate por equipos completo que
    /// resuelve <see cref="SimuladorEquipos"/>; esta capa lleva el marcador y empieza la siguiente.
    /// </summary>
    /// <remarks>
    /// Al terminar una ronda su ganador suma una victoria (en un empate la suman los dos). Si
    /// alguien reúne las victorias necesarias, el encuentro termina; si las reúnen los dos a la
    /// vez, termina en empate. En otro caso, tras la pausa entre rondas empieza una ronda nueva
    /// con los dos equipos completos.
    /// </remarks>
    public static class SimuladorEncuentro
    {
        public static EstadoEncuentro Avanzar(in EstadoEncuentro estado, in EntradasFrame entradas, in ConfiguracionEncuentro configuracion)
        {
            var ronda = SimuladorEquipos.Avanzar(estado.Ronda, entradas, configuracion.Equipos);

            if (!ronda.Terminado || estado.Terminado)
            {
                return estado.ConRonda(ronda);
            }

            if (estado.FramesTrasLaRonda == 0)
            {
                return AnotarRonda(estado, ronda, configuracion.VictoriasNecesarias);
            }

            var framesTrasLaRonda = estado.FramesTrasLaRonda + 1;

            return framesTrasLaRonda <= configuracion.FramesEntreRondas
                ? new EstadoEncuentro(ronda, estado.Victorias1, estado.Victorias2, estado.NumeroDeRonda, framesTrasLaRonda, ResultadoRonda.EnCurso)
                : new EstadoEncuentro(configuracion.RondaNueva(), estado.Victorias1, estado.Victorias2, estado.NumeroDeRonda + 1, 0, ResultadoRonda.EnCurso);
        }

        private static EstadoEncuentro AnotarRonda(in EstadoEncuentro estado, in EstadoCombateEquipos ronda, int victoriasNecesarias)
        {
            var gana1 = ronda.Resultado == ResultadoRonda.GanaJugador1 || ronda.Resultado == ResultadoRonda.Empate;
            var gana2 = ronda.Resultado == ResultadoRonda.GanaJugador2 || ronda.Resultado == ResultadoRonda.Empate;
            var victorias1 = estado.Victorias1 + (gana1 ? 1 : 0);
            var victorias2 = estado.Victorias2 + (gana2 ? 1 : 0);

            return new EstadoEncuentro(ronda, victorias1, victorias2, estado.NumeroDeRonda, 1, DecidirEncuentro(victorias1, victorias2, victoriasNecesarias));
        }

        private static ResultadoRonda DecidirEncuentro(int victorias1, int victorias2, int victoriasNecesarias)
        {
            var completo1 = victorias1 >= victoriasNecesarias;
            var completo2 = victorias2 >= victoriasNecesarias;

            if (completo1 && completo2)
            {
                return ResultadoRonda.Empate;
            }

            if (completo1)
            {
                return ResultadoRonda.GanaJugador1;
            }

            return completo2 ? ResultadoRonda.GanaJugador2 : ResultadoRonda.EnCurso;
        }
    }
}
