using System;
using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Datos fijos de un luchador que la simulación consulta pero nunca modifica.</summary>
    public readonly struct ParametrosLuchador
    {
        /// <summary>Unidades por frame al caminar hacia el rival.</summary>
        public readonly Fijo VelocidadAdelante;

        /// <summary>Unidades por frame al caminar alejándose del rival.</summary>
        public readonly Fijo VelocidadAtras;

        public readonly Fijo MedioAnchoEmpuje;
        public readonly Fijo MedioAltoEmpuje;

        public ParametrosLuchador(Fijo velocidadAdelante, Fijo velocidadAtras, Fijo medioAnchoEmpuje, Fijo medioAltoEmpuje)
        {
            if (velocidadAdelante < Fijo.Cero || velocidadAtras < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(velocidadAdelante), "Las velocidades de caminata no pueden ser negativas.");
            }

            if (medioAnchoEmpuje <= Fijo.Cero || medioAltoEmpuje <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(medioAnchoEmpuje), "La caja de empuje debe tener tamaño positivo.");
            }

            VelocidadAdelante = velocidadAdelante;
            VelocidadAtras = velocidadAtras;
            MedioAnchoEmpuje = medioAnchoEmpuje;
            MedioAltoEmpuje = medioAltoEmpuje;
        }

        /// <summary>Caja de empuje apoyada en el suelo: <paramref name="posicion"/> es el punto entre los pies.</summary>
        public CajaColision CajaEmpuje(VectorFijo posicion) =>
            new CajaColision(new VectorFijo(posicion.X, posicion.Y + MedioAltoEmpuje), MedioAnchoEmpuje, MedioAltoEmpuje);
    }
}
