using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Vida recuperable (la parte azul de la barra): una fracción del daño de cada golpe no
    /// bloqueado se puede recuperar. Se regenera despacio en pantalla tras un rato sin recibir
    /// golpes y más deprisa en reserva. Cada golpe nuevo destruye parte de la que ya había.
    /// </summary>
    public readonly struct ReglasVidaRecuperable
    {
        /// <summary>Porcentaje del daño de un golpe que queda como vida recuperable.</summary>
        public readonly int PorcentajeRecuperable;

        /// <summary>Porcentaje de la vida recuperable acumulada que se pierde al recibir otro golpe.</summary>
        public readonly int PorcentajePerdidoAlRecibirGolpe;

        /// <summary>Frames sin recibir golpes antes de que el luchador en pantalla empiece a regenerar.</summary>
        public readonly int FramesEsperaRegeneracion;

        /// <summary>Vida que recupera por frame el luchador en pantalla.</summary>
        public readonly int RegeneracionPorFrame;

        /// <summary>Vida que recupera por frame un luchador en reserva, sin espera.</summary>
        public readonly int RegeneracionEnReservaPorFrame;

        public ReglasVidaRecuperable(
            int porcentajeRecuperable,
            int porcentajePerdidoAlRecibirGolpe,
            int framesEsperaRegeneracion,
            int regeneracionPorFrame,
            int regeneracionEnReservaPorFrame)
        {
            if (porcentajeRecuperable < 0 || porcentajeRecuperable > 100 || porcentajePerdidoAlRecibirGolpe < 0 || porcentajePerdidoAlRecibirGolpe > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(porcentajeRecuperable), "Los porcentajes de vida recuperable van de 0 a 100.");
            }

            if (framesEsperaRegeneracion < 0 || regeneracionPorFrame < 0 || regeneracionEnReservaPorFrame < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(framesEsperaRegeneracion), "La espera y los ritmos de regeneración no pueden ser negativos.");
            }

            PorcentajeRecuperable = porcentajeRecuperable;
            PorcentajePerdidoAlRecibirGolpe = porcentajePerdidoAlRecibirGolpe;
            FramesEsperaRegeneracion = framesEsperaRegeneracion;
            RegeneracionPorFrame = regeneracionPorFrame;
            RegeneracionEnReservaPorFrame = regeneracionEnReservaPorFrame;
        }

        /// <summary>Vida recuperable tras un golpe no bloqueado: se conserva parte de la anterior y se suma parte del daño nuevo.</summary>
        public int TrasGolpe(int recuperableAnterior, int danioRecibido) =>
            recuperableAnterior * (100 - PorcentajePerdidoAlRecibirGolpe) / 100 + danioRecibido * PorcentajeRecuperable / 100;

        /// <summary>Vida que se recupera en un frame, sin pasar de la recuperable disponible.</summary>
        public static int Curacion(int recuperable, int ritmoPorFrame) =>
            recuperable < ritmoPorFrame ? recuperable : ritmoPorFrame;
    }
}
