using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Reglas comunes de los agarres: cuándo sale el agarre normal y cuánto dura la protección tras un aturdimiento.</summary>
    public readonly struct ReglasAgarre
    {
        /// <summary>
        /// Frames durante los que no se puede agarrar a quien acaba de salir de un hitstun, un
        /// blockstun, un derribo o una recuperación aérea. Evita encadenar bloqueo y agarre sin escape.
        /// </summary>
        public readonly int FramesProteccion;

        /// <summary>
        /// Distancia horizontal máxima al rival, que debe estar en tierra, para que una dirección
        /// más el golpe fuerte sea un agarre; más lejos sale el golpe fuerte de siempre.
        /// </summary>
        public readonly Fijo DistanciaActivacion;

        /// <summary>
        /// Frames, desde que el rival lo sujeta, durante los que el luchador puede escapar del
        /// agarre pulsando el mismo comando: una dirección más el golpe fuerte.
        /// </summary>
        public readonly int FramesVentanaEscape;

        /// <summary>Frames que los dos luchadores pasan separándose, sin control, tras un agarre roto.</summary>
        public readonly int FramesAgarreRoto;

        /// <summary>Velocidad inicial por frame con la que cada luchador se aleja del otro tras un agarre roto.</summary>
        public readonly Fijo RetrocesoAlEscapar;

        public ReglasAgarre(int framesProteccion, Fijo distanciaActivacion, int framesVentanaEscape, int framesAgarreRoto, Fijo retrocesoAlEscapar)
        {
            if (framesProteccion < 0 || distanciaActivacion < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(framesProteccion), "La protección y la distancia de activación no pueden ser negativas.");
            }

            if (framesVentanaEscape < 0 || framesAgarreRoto < 1 || retrocesoAlEscapar < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(framesVentanaEscape), "La ventana de escape y el retroceso no pueden ser negativos y el agarre roto dura al menos 1 frame.");
            }

            FramesVentanaEscape = framesVentanaEscape;
            FramesAgarreRoto = framesAgarreRoto;
            RetrocesoAlEscapar = retrocesoAlEscapar;

            FramesProteccion = framesProteccion;
            DistanciaActivacion = distanciaActivacion;
        }
    }
}
