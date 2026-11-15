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

        /// <summary>Ejecutando un ataque: arranque, frames activos y recuperación.</summary>
        Atacando = 7,

        /// <summary>Aturdido en el suelo por un golpe recibido.</summary>
        Hitstun = 8,

        /// <summary>Aturdido en el suelo por un golpe bloqueado.</summary>
        Blockstun = 9,

        /// <summary>Sin vida: en el suelo y sin control hasta que empiece otra ronda.</summary>
        Noqueado = 10,

        /// <summary>Ejecutando un ataque en el aire; la trayectoria del salto no cambia.</summary>
        AtacandoEnAire = 11,

        /// <summary>Golpeado en el aire o lanzado: cae sin control y puede recibir más golpes (juggle).</summary>
        GolpeadoEnAire = 12,

        /// <summary>En el suelo tras caer golpeado: invulnerable hasta levantarse.</summary>
        Derribado = 13,
    }

    public static class EstadosAccion
    {
        /// <summary>Acciones en el suelo desde las que el jugador puede caminar, agacharse, saltar o atacar.</summary>
        public static bool TieneControlEnTierra(AccionLuchador accion) =>
            accion == AccionLuchador.Quieto ||
            accion == AccionLuchador.CaminandoAdelante ||
            accion == AccionLuchador.CaminandoAtras ||
            accion == AccionLuchador.Agachado;

        /// <summary>Acciones en las que el luchador se gira para mirar al rival. En el aire, atacando o aturdido conserva su orientación.</summary>
        public static bool PuedeGirar(AccionLuchador accion) =>
            TieneControlEnTierra(accion) || accion == AccionLuchador.Aterrizaje;

        /// <summary>Se puede bloquear con control en tierra, o encadenando bloqueos mientras dura el blockstun.</summary>
        public static bool PuedeBloquear(AccionLuchador accion) =>
            TieneControlEnTierra(accion) || accion == AccionLuchador.Blockstun;

        public static bool EstaEnElAire(AccionLuchador accion) =>
            accion == AccionLuchador.EnAire ||
            accion == AccionLuchador.AtacandoEnAire ||
            accion == AccionLuchador.GolpeadoEnAire;

        public static bool EstaAtacando(AccionLuchador accion) =>
            accion == AccionLuchador.Atacando || accion == AccionLuchador.AtacandoEnAire;

        /// <summary>Acciones en las que ningún golpe puede conectar.</summary>
        public static bool EsInvulnerable(AccionLuchador accion) =>
            accion == AccionLuchador.Derribado || accion == AccionLuchador.Noqueado;

        /// <summary>Recibiendo un combo: el siguiente golpe cuenta como parte de él.</summary>
        public static bool EstaEnCombo(AccionLuchador accion) =>
            accion == AccionLuchador.Hitstun || accion == AccionLuchador.GolpeadoEnAire;

        public static bool EstaAturdido(AccionLuchador accion) =>
            accion == AccionLuchador.Hitstun || accion == AccionLuchador.Blockstun;
    }
}
