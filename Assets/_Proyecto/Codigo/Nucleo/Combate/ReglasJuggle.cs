using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Reglas del rival golpeado en el aire (juggle): cada golpe lo vuelve a levantar un poco, pero
    /// cae más rápido cuanto más largo es el combo, y al tocar el suelo queda derribado.
    /// </summary>
    public readonly struct ReglasJuggle
    {
        /// <summary>Velocidad vertical que recibe un rival golpeado cuando ya está en el aire.</summary>
        public readonly Fijo ImpulsoAlGolpear;

        /// <summary>Gravedad del rival golpeado en el aire, por frame.</summary>
        public readonly Fijo Gravedad;

        /// <summary>Gravedad que se suma por cada golpe del combo; es lo que impide los combos infinitos.</summary>
        public readonly Fijo GravedadExtraPorGolpe;

        /// <summary>Frames que el rival pasa en el suelo, invulnerable, antes de levantarse.</summary>
        public readonly int FramesDerribado;

        public ReglasJuggle(Fijo impulsoAlGolpear, Fijo gravedad, Fijo gravedadExtraPorGolpe, int framesDerribado)
        {
            if (impulsoAlGolpear < Fijo.Cero || gravedadExtraPorGolpe < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(impulsoAlGolpear), "El impulso y la gravedad extra no pueden ser negativos.");
            }

            if (gravedad <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(gravedad), "La gravedad del juggle debe ser positiva.");
            }

            if (framesDerribado < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesDerribado), "El derribo dura al menos 1 frame.");
            }

            ImpulsoAlGolpear = impulsoAlGolpear;
            Gravedad = gravedad;
            GravedadExtraPorGolpe = gravedadExtraPorGolpe;
            FramesDerribado = framesDerribado;
        }

        public Fijo GravedadCon(int golpesEnCombo) => Gravedad + GravedadExtraPorGolpe * golpesEnCombo;
    }
}
