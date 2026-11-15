namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Estado de la ronda. Los valores forman parte de la suma de verificación: se agregan al
    /// final y nunca se reordenan.
    /// </summary>
    public enum ResultadoRonda : byte
    {
        EnCurso = 0,
        GanaJugador1 = 1,
        GanaJugador2 = 2,

        /// <summary>Doble K.O., o tiempo agotado con la misma proporción de vida.</summary>
        Empate = 3,
    }
}
