using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Tamaño de la caja de daño (hurtbox) del luchador según su postura.</summary>
    public readonly struct MedidasCajaDanio
    {
        public readonly Fijo MedioAncho;
        public readonly Fijo MedioAltoDePie;
        public readonly Fijo MedioAltoAgachado;
        public readonly Fijo MedioAltoAire;

        public MedidasCajaDanio(Fijo medioAncho, Fijo medioAltoDePie, Fijo medioAltoAgachado, Fijo medioAltoAire)
        {
            if (medioAncho <= Fijo.Cero || medioAltoDePie <= Fijo.Cero || medioAltoAgachado <= Fijo.Cero || medioAltoAire <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(medioAncho), "Las cajas de daño deben tener tamaño positivo.");
            }

            MedioAncho = medioAncho;
            MedioAltoDePie = medioAltoDePie;
            MedioAltoAgachado = medioAltoAgachado;
            MedioAltoAire = medioAltoAire;
        }

        public Fijo MedioAlto(AccionLuchador accion)
        {
            switch (accion)
            {
                case AccionLuchador.Agachado:
                    return MedioAltoAgachado;
                default:
                    return EstadosAccion.EstaEnElAire(accion) ? MedioAltoAire : MedioAltoDePie;
            }
        }
    }
}
