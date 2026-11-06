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

        /// <summary>Frames en el suelo antes de despegar; el jugador aún puede elegir la dirección del salto.</summary>
        PreSalto = 4,

        EnAire = 5,

        /// <summary>Frames de recuperación al tocar el suelo, sin control.</summary>
        Aterrizaje = 6,
    }

    public static class EstadosAccion
    {
        /// <summary>Acciones en el suelo desde las que el jugador puede caminar, agacharse o saltar.</summary>
        public static bool TieneControlEnTierra(AccionLuchador accion) =>
            accion == AccionLuchador.Quieto ||
            accion == AccionLuchador.CaminandoAdelante ||
            accion == AccionLuchador.CaminandoAtras ||
            accion == AccionLuchador.Agachado;

        /// <summary>Acciones en las que el luchador se gira para mirar al rival. En el aire conserva su orientación.</summary>
        public static bool PuedeGirar(AccionLuchador accion) =>
            TieneControlEnTierra(accion) || accion == AccionLuchador.Aterrizaje;
    }
}
