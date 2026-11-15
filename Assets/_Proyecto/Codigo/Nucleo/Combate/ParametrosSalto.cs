using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Arco del salto. Con velocidad vertical v y gravedad g por frame, la altura máxima es
    /// aproximadamente v² / (2g) y el tiempo en el aire unos 2v / g frames.
    /// </summary>
    public readonly struct ParametrosSalto
    {
        public readonly int FramesPreSalto;
        public readonly int FramesAterrizaje;

        /// <summary>Velocidad vertical al despegar, en unidades por frame.</summary>
        public readonly Fijo VelocidadVertical;

        /// <summary>Velocidad vertical que se pierde cada frame en el aire.</summary>
        public readonly Fijo Gravedad;

        public readonly Fijo VelocidadAdelante;
        public readonly Fijo VelocidadAtras;

        /// <summary>Velocidad vertical al despegar en un supersalto.</summary>
        public readonly Fijo VelocidadVerticalSupersalto;

        /// <summary>
        /// Frames tras dejar de estar agachado durante los que un salto es un supersalto: da
        /// margen para soltar abajo y pulsar arriba.
        /// </summary>
        public readonly int VentanaSupersalto;

        public ParametrosSalto(
            int framesPreSalto,
            int framesAterrizaje,
            Fijo velocidadVertical,
            Fijo gravedad,
            Fijo velocidadAdelante,
            Fijo velocidadAtras,
            Fijo velocidadVerticalSupersalto,
            int ventanaSupersalto)
        {
            if (framesPreSalto < 1 || framesAterrizaje < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(framesPreSalto), "El presalto dura al menos 1 frame y el aterrizaje no puede ser negativo.");
            }

            if (velocidadVertical <= Fijo.Cero || gravedad <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadVertical), "La velocidad vertical y la gravedad deben ser positivas.");
            }

            if (velocidadAdelante < Fijo.Cero || velocidadAtras < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadAdelante), "Las velocidades horizontales del salto no pueden ser negativas.");
            }

            if (velocidadVerticalSupersalto < velocidadVertical || ventanaSupersalto < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadVerticalSupersalto), "El supersalto no puede ser más bajo que el salto y su ventana no puede ser negativa.");
            }

            FramesPreSalto = framesPreSalto;
            FramesAterrizaje = framesAterrizaje;
            VelocidadVertical = velocidadVertical;
            Gravedad = gravedad;
            VelocidadAdelante = velocidadAdelante;
            VelocidadAtras = velocidadAtras;
            VelocidadVerticalSupersalto = velocidadVerticalSupersalto;
            VentanaSupersalto = ventanaSupersalto;
        }
    }
}
