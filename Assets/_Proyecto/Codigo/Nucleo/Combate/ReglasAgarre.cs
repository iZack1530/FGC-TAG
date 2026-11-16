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

        public ReglasAgarre(int framesProteccion, Fijo distanciaActivacion)
        {
            if (framesProteccion < 0 || distanciaActivacion < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(framesProteccion), "La protección y la distancia de activación no pueden ser negativas.");
            }

            FramesProteccion = framesProteccion;
            DistanciaActivacion = distanciaActivacion;
        }
    }
}
