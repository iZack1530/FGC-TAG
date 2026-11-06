using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Colisiones
{
    /// <summary>
    /// Caja alineada a los ejes para hitboxes, hurtboxes y cajas de empuje.
    /// Las cajas de un movimiento se definen relativas al personaje mirando a la derecha;
    /// <see cref="Orientada"/> las adapta al lado al que mira y <see cref="Desplazada"/> las lleva al mundo.
    /// </summary>
    public readonly struct CajaColision : IEquatable<CajaColision>
    {
        public readonly VectorFijo Centro;
        public readonly Fijo MedioAncho;
        public readonly Fijo MedioAlto;

        public CajaColision(VectorFijo centro, Fijo medioAncho, Fijo medioAlto)
        {
            if (medioAncho < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(medioAncho), "El medio ancho no puede ser negativo.");
            }

            if (medioAlto < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(medioAlto), "El medio alto no puede ser negativo.");
            }

            Centro = centro;
            MedioAncho = medioAncho;
            MedioAlto = medioAlto;
        }

        public Fijo Izquierda => Centro.X - MedioAncho;

        public Fijo Derecha => Centro.X + MedioAncho;

        public Fijo Abajo => Centro.Y - MedioAlto;

        public Fijo Arriba => Centro.Y + MedioAlto;

        /// <summary>
        /// Solapamiento estricto: dos cajas que solo se tocan en el borde no colisionan,
        /// para que un golpe que apenas roza no conecte por redondeo.
        /// </summary>
        public bool Intersecta(CajaColision otra) =>
            Izquierda < otra.Derecha &&
            otra.Izquierda < Derecha &&
            Abajo < otra.Arriba &&
            otra.Abajo < Arriba;

        public CajaColision Desplazada(VectorFijo desplazamiento) =>
            new CajaColision(Centro + desplazamiento, MedioAncho, MedioAlto);

        /// <param name="miraDerecha">Lado al que mira el personaje dueño de la caja.</param>
        public CajaColision Orientada(bool miraDerecha) =>
            miraDerecha ? this : new CajaColision(Centro.ReflejadoHorizontal(), MedioAncho, MedioAlto);

        public bool Equals(CajaColision otra) =>
            Centro == otra.Centro && MedioAncho == otra.MedioAncho && MedioAlto == otra.MedioAlto;

        public override bool Equals(object obj) => obj is CajaColision otra && Equals(otra);

        public override int GetHashCode() =>
            unchecked((Centro.GetHashCode() * 397) ^ (MedioAncho.GetHashCode() * 31) ^ MedioAlto.GetHashCode());

        public override string ToString() => $"Caja(centro {Centro}, medio ancho {MedioAncho}, medio alto {MedioAlto})";
    }
}
