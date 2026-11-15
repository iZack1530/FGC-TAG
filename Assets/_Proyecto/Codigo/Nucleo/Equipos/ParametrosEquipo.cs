using System;
using FgcTag.Nucleo.Combate;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>Los tres luchadores de un equipo, en su orden de selección.</summary>
    public readonly struct ParametrosEquipo
    {
        public readonly ParametrosLuchador Primero;
        public readonly ParametrosLuchador Segundo;
        public readonly ParametrosLuchador Tercero;

        public ParametrosEquipo(ParametrosLuchador primero, ParametrosLuchador segundo, ParametrosLuchador tercero)
        {
            Primero = primero;
            Segundo = segundo;
            Tercero = tercero;
        }

        /// <param name="indice">Posición en el equipo, de 0 a 2.</param>
        public ParametrosLuchador Obtener(int indice)
        {
            switch (indice)
            {
                case 0:
                    return Primero;
                case 1:
                    return Segundo;
                case 2:
                    return Tercero;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indice), "Un equipo tiene tres luchadores, con índices de 0 a 2.");
            }
        }

        public int VidaMaximaTotal => Primero.VidaMaxima + Segundo.VidaMaxima + Tercero.VidaMaxima;
    }
}
