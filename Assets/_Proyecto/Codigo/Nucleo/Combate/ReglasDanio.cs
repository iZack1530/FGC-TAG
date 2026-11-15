using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Escalado del daño en combos: cada golpe adicional hace un porcentaje menos de su daño
    /// base, hasta un mínimo. Todo en enteros para que el resultado sea idéntico en cualquier máquina.
    /// </summary>
    public readonly struct ReglasDanio
    {
        /// <summary>Puntos porcentuales que pierde el daño por cada golpe del combo después del primero.</summary>
        public readonly int ReduccionPorGolpe;

        /// <summary>Porcentaje del daño base por debajo del cual el escalado ya no baja.</summary>
        public readonly int PorcentajeMinimo;

        public ReglasDanio(int reduccionPorGolpe, int porcentajeMinimo)
        {
            if (reduccionPorGolpe < 0 || reduccionPorGolpe > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(reduccionPorGolpe), "La reducción por golpe va de 0 a 100.");
            }

            if (porcentajeMinimo < 0 || porcentajeMinimo > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(porcentajeMinimo), "El porcentaje mínimo va de 0 a 100.");
            }

            ReduccionPorGolpe = reduccionPorGolpe;
            PorcentajeMinimo = porcentajeMinimo;
        }

        /// <param name="numeroDeGolpe">Posición del golpe dentro del combo; 1 es el primero.</param>
        public int Porcentaje(int numeroDeGolpe)
        {
            var golpesPrevios = numeroDeGolpe > 1 ? numeroDeGolpe - 1 : 0;
            var porcentaje = 100 - ReduccionPorGolpe * golpesPrevios;

            return porcentaje > PorcentajeMinimo ? porcentaje : PorcentajeMinimo;
        }

        /// <summary>Daño que hace un golpe en esa posición del combo, redondeado hacia abajo.</summary>
        public int Escalar(int danioBase, int numeroDeGolpe) => danioBase * Porcentaje(numeroDeGolpe) / 100;
    }
}
