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

        public EstadoPartida(int frame, EstadoLuchador luchador1, EstadoLuchador luchador2)
        {
            Frame = frame;
            Luchador1 = luchador1;
            Luchador2 = luchador2;
        }

        /// <summary>Estado del frame 0 con ambos luchadores quietos y mirándose.</summary>
        public static EstadoPartida Crear(VectorFijo posicionJugador1, VectorFijo posicionJugador2)
        {
            var jugador1MiraDerecha = posicionJugador1.X <= posicionJugador2.X;

            return new EstadoPartida(
                0,
                new EstadoLuchador(posicionJugador1, jugador1MiraDerecha, AccionLuchador.Quieto, 0),
                new EstadoLuchador(posicionJugador2, !jugador1MiraDerecha, AccionLuchador.Quieto, 0));
        }

        public SumaVerificacion CalcularSumaVerificacion() =>
            Luchador2.AgregarA(Luchador1.AgregarA(SumaVerificacion.Inicial.Agregar(Frame)));
    }
}
