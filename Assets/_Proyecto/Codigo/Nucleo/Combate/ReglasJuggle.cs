using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Reglas del rival golpeado en el aire (juggle): cada golpe lo vuelve a levantar un poco, pero
    /// cae más rápido y se recupera antes cuanto más largo es el combo. Si toca el suelo sin
    /// haberse recuperado, queda derribado.
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

        /// <summary>Frames de aturdimiento aéreo que se pierden por cada golpe del combo después del primero.</summary>
        public readonly int ReduccionAturdimientoPorGolpe;

        /// <summary>Aturdimiento aéreo por debajo del cual el decaimiento ya no baja.</summary>
        public readonly int AturdimientoMinimo;

        /// <summary>Velocidad horizontal al recuperarse en el aire, hacia la dirección que se mantenga.</summary>
        public readonly Fijo VelocidadRecuperacion;

        /// <summary>Velocidad vertical al recuperarse en el aire: un pequeño salto para reorientarse.</summary>
        public readonly Fijo ImpulsoRecuperacion;

        public ReglasJuggle(
            Fijo impulsoAlGolpear,
            Fijo gravedad,
            Fijo gravedadExtraPorGolpe,
            int framesDerribado,
            int reduccionAturdimientoPorGolpe,
            int aturdimientoMinimo,
            Fijo velocidadRecuperacion,
            Fijo impulsoRecuperacion)
        {
            if (reduccionAturdimientoPorGolpe < 0 || aturdimientoMinimo < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(aturdimientoMinimo), "El aturdimiento mínimo dura al menos 1 frame y su reducción no puede ser negativa.");
            }

            if (velocidadRecuperacion < Fijo.Cero || impulsoRecuperacion < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadRecuperacion), "Las velocidades de recuperación no pueden ser negativas.");
            }

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
            ReduccionAturdimientoPorGolpe = reduccionAturdimientoPorGolpe;
            AturdimientoMinimo = aturdimientoMinimo;
            VelocidadRecuperacion = velocidadRecuperacion;
            ImpulsoRecuperacion = impulsoRecuperacion;
        }

        /// <param name="golpesEnCombo">Posición del golpe dentro del combo; 1 es el primero.</param>
        public int AturdimientoCon(int hitstunAereo, int golpesEnCombo)
        {
            var golpesPrevios = golpesEnCombo > 1 ? golpesEnCombo - 1 : 0;
            var aturdimiento = hitstunAereo - ReduccionAturdimientoPorGolpe * golpesPrevios;

            return aturdimiento > AturdimientoMinimo ? aturdimiento : AturdimientoMinimo;
        }

        public Fijo GravedadCon(int golpesEnCombo) => Gravedad + GravedadExtraPorGolpe * golpesEnCombo;
    }
}
