using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Estado completo de la partida en un frame. Guardar y restaurar un frame para el rollback
    /// es copiar este struct.
    /// </summary>
    public readonly struct EstadoPartida
    {
        public readonly int Frame;
        public readonly EstadoLuchador Luchador1;
        public readonly EstadoLuchador Luchador2;

        /// <summary>Frames que le quedan al reloj de la ronda; se detiene cuando la ronda termina.</summary>
        public readonly int FramesRestantes;

        public readonly ResultadoRonda Resultado;

        public EstadoPartida(int frame, EstadoLuchador luchador1, EstadoLuchador luchador2, int framesRestantes, ResultadoRonda resultado)
        {
            Frame = frame;
            Luchador1 = luchador1;
            Luchador2 = luchador2;
            FramesRestantes = framesRestantes;
            Resultado = resultado;
        }

        public bool RondaTerminada => Resultado != ResultadoRonda.EnCurso;

        /// <summary>Estado del frame 0: ambos luchadores quietos, mirándose, con la vida llena y el reloj completo.</summary>
        public static EstadoPartida Crear(VectorFijo posicionJugador1, VectorFijo posicionJugador2, in ConfiguracionCombate configuracion)
        {
            var jugador1MiraDerecha = posicionJugador1.X <= posicionJugador2.X;

            return new EstadoPartida(
                0,
                EstadoLuchador.EnReposo(posicionJugador1, jugador1MiraDerecha, configuracion.Jugador1.VidaMaxima),
                EstadoLuchador.EnReposo(posicionJugador2, !jugador1MiraDerecha, configuracion.Jugador2.VidaMaxima),
                configuracion.FramesPorRonda,
                ResultadoRonda.EnCurso);
        }

        public SumaVerificacion CalcularSumaVerificacion() =>
            Luchador2.AgregarA(Luchador1.AgregarA(SumaVerificacion.Inicial.Agregar(Frame)))
                .Agregar(FramesRestantes)
                .Agregar((int)Resultado);
    }
}
