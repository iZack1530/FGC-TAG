namespace FgcTag.Nucleo.Combate
{
    /// <summary>Altura de un golpe: decide con qué guardia se puede bloquear en tierra.</summary>
    public enum AlturaGolpe : byte
    {
        /// <summary>Se bloquea de pie o agachado.</summary>
        Medio = 0,

        /// <summary>Golpe bajo: solo se bloquea agachado.</summary>
        Bajo = 1,

        /// <summary>Golpe alto (los ataques aéreos): solo se bloquea de pie.</summary>
        Alto = 2,
    }

    /// <summary>Postura desde la que sale un ataque; cada botón tiene un movimiento distinto en cada una.</summary>
    public enum PosturaAtaque : byte
    {
        DePie = 0,
        Agachado = 1,
        Aire = 2,
    }

    public static class Guardia
    {
        /// <summary>En tierra la guardia debe corresponder a la altura del golpe; si no, el golpe entra.</summary>
        public static bool Detiene(AlturaGolpe altura, bool guardiaAgachada)
        {
            switch (altura)
            {
                case AlturaGolpe.Bajo:
                    return guardiaAgachada;
                case AlturaGolpe.Alto:
                    return !guardiaAgachada;
                default:
                    return true;
            }
        }
    }
}
