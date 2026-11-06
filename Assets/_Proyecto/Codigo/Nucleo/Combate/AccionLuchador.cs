namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Acción que ejecuta el luchador. Los valores numéricos forman parte de la suma de
    /// verificación: se agregan al final y nunca se reordenan.
    /// </summary>
    public enum AccionLuchador : byte
    {
        Quieto = 0,
        CaminandoAdelante = 1,
        CaminandoAtras = 2,
        Agachado = 3,
    }
}
