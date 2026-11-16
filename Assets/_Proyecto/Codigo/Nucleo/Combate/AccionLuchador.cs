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

        /// <summary>Ejecutando un ataque agachado; el lanzador no tiene versión agachada.</summary>
        AtacandoAgachado = 14,

        /// <summary>Aturdido en el aire por un golpe bloqueado; sigue la trayectoria del salto.</summary>
        BloqueandoEnAire = 15,

        /// <summary>
        /// Recuperándose en el aire tras un combo: invencible al principio; después puede bloquear,
        /// pero no atacar hasta que termina.
        /// </summary>
        RecuperandoEnAire = 16,

        /// <summary>Aturdido en el suelo por un golpe bloqueado con la guardia baja.</summary>
        BlockstunAgachado = 17,

        /// <summary>Llega a la pantalla saltando desde detrás: está en el aire, sin control y sin poder bloquear.</summary>
        Entrando = 18,

        /// <summary>Intento de agarre: arranque, frames activos y recuperación si falla.</summary>
        IntentandoAgarre = 19,

        /// <summary>Sujeta al rival y lo lanza; no recibe golpes mientras dura.</summary>
        Agarrando = 20,

        /// <summary>Sujeto por el rival hasta que lo lanza; no recibe golpes mientras dura.</summary>
        Agarrado = 21,
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
            TieneControlEnTierra(accion) ||
            accion == AccionLuchador.Blockstun ||
            accion == AccionLuchador.BlockstunAgachado;

        public static bool EstaEnElAire(AccionLuchador accion) =>
            accion == AccionLuchador.EnAire ||
            accion == AccionLuchador.AtacandoEnAire ||
            accion == AccionLuchador.GolpeadoEnAire ||
            accion == AccionLuchador.BloqueandoEnAire ||
            accion == AccionLuchador.RecuperandoEnAire ||
            accion == AccionLuchador.Entrando;

        /// <summary>
        /// En el aire se bloquea durante un salto libre, encadenando bloqueos o al recuperarse de
        /// un combo; no mientras se ataca.
        /// </summary>
        public static bool PuedeBloquearEnAire(AccionLuchador accion) =>
            accion == AccionLuchador.EnAire ||
            accion == AccionLuchador.BloqueandoEnAire ||
            accion == AccionLuchador.RecuperandoEnAire;

        public static bool EstaBloqueando(AccionLuchador accion) =>
            accion == AccionLuchador.Blockstun ||
            accion == AccionLuchador.BlockstunAgachado ||
            accion == AccionLuchador.BloqueandoEnAire;

        /// <summary>Posturas bajas: la caja de daño es la de agachado.</summary>
        public static bool EstaAgachado(AccionLuchador accion) =>
            accion == AccionLuchador.Agachado ||
            accion == AccionLuchador.AtacandoAgachado ||
            accion == AccionLuchador.BlockstunAgachado;

        public static bool EstaAtacando(AccionLuchador accion) =>
            accion == AccionLuchador.Atacando ||
            accion == AccionLuchador.AtacandoAgachado ||
            accion == AccionLuchador.AtacandoEnAire;

        public static PosturaAtaque PosturaDeAtaque(AccionLuchador accion)
        {
            switch (accion)
            {
                case AccionLuchador.AtacandoEnAire:
                    return PosturaAtaque.Aire;
                case AccionLuchador.AtacandoAgachado:
                    return PosturaAtaque.Agachado;
                default:
                    return PosturaAtaque.DePie;
            }
        }

        /// <summary>Acciones en las que ningún golpe puede conectar.</summary>
        public static bool EsInvulnerable(AccionLuchador accion) =>
            accion == AccionLuchador.Derribado ||
            accion == AccionLuchador.Noqueado ||
            accion == AccionLuchador.Agarrando ||
            accion == AccionLuchador.Agarrado;

        /// <summary>Estados que renuevan la protección contra agarres: mientras duran, y unos frames después, no se puede agarrar al luchador.</summary>
        public static bool ProtegeDeAgarres(AccionLuchador accion) =>
            EstaAturdido(accion) ||
            accion == AccionLuchador.GolpeadoEnAire ||
            accion == AccionLuchador.Derribado ||
            accion == AccionLuchador.RecuperandoEnAire ||
            accion == AccionLuchador.BloqueandoEnAire ||
            accion == AccionLuchador.Agarrado;

        /// <summary>Recibiendo un combo: el siguiente golpe cuenta como parte de él.</summary>
        public static bool EstaEnCombo(AccionLuchador accion) =>
            accion == AccionLuchador.Hitstun || accion == AccionLuchador.GolpeadoEnAire;

        /// <summary>
        /// Se puede llamar a un asistente mientras el luchador controla lo que hace: no si está
        /// aturdido, en un combo, derribado, entrando o recuperándose en el aire.
        /// </summary>
        public static bool PuedePedirAsistencia(AccionLuchador accion) =>
            !EstaAturdido(accion) &&
            !EstaEnCombo(accion) &&
            !EsInvulnerable(accion) &&
            accion != AccionLuchador.Entrando &&
            accion != AccionLuchador.BloqueandoEnAire &&
            accion != AccionLuchador.RecuperandoEnAire;

        /// <summary>Ataques en tierra: los únicos que, al conectar, se pueden interrumpir con un cambio de luchador.</summary>
        public static bool EstaAtacandoEnTierra(AccionLuchador accion) =>
            accion == AccionLuchador.Atacando || accion == AccionLuchador.AtacandoAgachado;

        public static bool EstaAturdido(AccionLuchador accion) =>
            accion == AccionLuchador.Hitstun ||
            accion == AccionLuchador.Blockstun ||
            accion == AccionLuchador.BlockstunAgachado;
    }
}
