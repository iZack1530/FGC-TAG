using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Dash de un luchador: se hace con dos botones de ataque a la vez, hacia el rival o, si se
    /// mantiene atrás, alejándose de él. En tierra es un impulso que la fricción va frenando; en
    /// el aire, un tramo horizontal sin gravedad, una vez por salto.
    /// </summary>
    /// <remarks>
    /// El dash en tierra arranca rápido y pierde velocidad, así que reiniciarlo recorre más que
    /// dejarlo terminar. Tras unos frames mínimos se cancela agachándose (y desde ahí se repite:
    /// wavedash), con un ataque o con un salto. Además, los primeros frames de un ataque en tierra
    /// se cancelan en dash pulsando un segundo botón (plink): encadenar dash, ataque y plink es
    /// más rápido que el wavedash, pero exige un ritmo de uno o dos frames.
    /// </remarks>
    public readonly struct ParametrosDash
    {
        /// <summary>Frames que dura el dash en tierra si no se cancela.</summary>
        public readonly int FramesDash;

        /// <summary>Frames de dash en tierra antes de poder cancelarlo agachándose o saltando.</summary>
        public readonly int FramesMinimosParaCancelar;

        /// <summary>Frames de dash en tierra antes de poder cancelarlo con un ataque; menos que para agacharse, y por eso el plink dash es más rápido que el wavedash.</summary>
        public readonly int FramesMinimosParaAtacar;

        /// <summary>Velocidad inicial por frame del dash hacia el rival.</summary>
        public readonly Fijo VelocidadAdelante;

        /// <summary>Velocidad inicial por frame del dash que se aleja del rival.</summary>
        public readonly Fijo VelocidadAtras;

        /// <summary>Velocidad que pierde el dash en tierra cada frame.</summary>
        public readonly Fijo Friccion;

        /// <summary>Frames iniciales de un ataque en tierra durante los que un segundo botón lo convierte en dash.</summary>
        public readonly int VentanaPlink;

        /// <summary>Frames que dura el dash aéreo si no se cancela.</summary>
        public readonly int FramesDashAereo;

        /// <summary>Frames de dash aéreo antes de poder cancelarlo con un ataque aéreo.</summary>
        public readonly int FramesMinimosDashAereo;

        /// <summary>Velocidad horizontal por frame del dash aéreo, constante.</summary>
        public readonly Fijo VelocidadDashAereo;

        /// <summary>Altura mínima sobre el suelo para hacer un dash aéreo.</summary>
        public readonly Fijo AlturaMinimaDashAereo;

        public ParametrosDash(
            int framesDash,
            int framesMinimosParaCancelar,
            int framesMinimosParaAtacar,
            Fijo velocidadAdelante,
            Fijo velocidadAtras,
            Fijo friccion,
            int ventanaPlink,
            int framesDashAereo,
            int framesMinimosDashAereo,
            Fijo velocidadDashAereo,
            Fijo alturaMinimaDashAereo)
        {
            if (framesDash < 1 || framesDashAereo < 1 || framesMinimosParaCancelar < 1 || framesMinimosParaAtacar < 1 || framesMinimosDashAereo < 1 || ventanaPlink < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(framesDash), "Los dashes duran al menos 1 frame y no se cancelan antes de 1 frame.");
            }

            if (velocidadAdelante <= Fijo.Cero || velocidadAtras <= Fijo.Cero || velocidadDashAereo <= Fijo.Cero || friccion < Fijo.Cero || alturaMinimaDashAereo < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadAdelante), "Las velocidades de dash deben ser positivas; la fricción y la altura mínima, no negativas.");
            }

            FramesDash = framesDash;
            FramesMinimosParaCancelar = framesMinimosParaCancelar;
            FramesMinimosParaAtacar = framesMinimosParaAtacar;
            VelocidadAdelante = velocidadAdelante;
            VelocidadAtras = velocidadAtras;
            Friccion = friccion;
            VentanaPlink = ventanaPlink;
            FramesDashAereo = framesDashAereo;
            FramesMinimosDashAereo = framesMinimosDashAereo;
            VelocidadDashAereo = velocidadDashAereo;
            AlturaMinimaDashAereo = alturaMinimaDashAereo;
        }
    }
}
