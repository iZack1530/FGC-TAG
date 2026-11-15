using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Golpes de un frame: comprueba la caja de golpe de cada atacante contra la caja de daño del
    /// rival y aplica el resultado (bloqueo, hitstun, lanzamiento, juggle o K.O.).
    /// </summary>
    public static class ResolucionGolpes
    {
        /// <summary>Los dos golpes se evalúan sobre el mismo estado, así que dos ataques simultáneos se intercambian.</summary>
        public static (EstadoLuchador, EstadoLuchador) Resolver(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in EntradasFrame entradas,
            in ConfiguracionCombate configuracion)
        {
            var golpea1 = Conecta(luchador1, configuracion.Jugador1, luchador2, configuracion.Jugador2, configuracion.Juggle);
            var golpea2 = Conecta(luchador2, configuracion.Jugador2, luchador1, configuracion.Jugador1, configuracion.Juggle);

            var recibido1 = golpea2
                ? RecibirGolpe(luchador1, luchador2, entradas.Jugador1, configuracion.Jugador2.MovimientoEnCurso(luchador2), configuracion)
                : luchador1;

            var recibido2 = golpea1
                ? RecibirGolpe(luchador2, luchador1, entradas.Jugador2, configuracion.Jugador1.MovimientoEnCurso(luchador1), configuracion)
                : luchador2;

            var resultado1 = golpea1 && !golpea2
                ? MarcarContacto(luchador1, configuracion.Jugador1.MovimientoEnCurso(luchador1), recibido2)
                : recibido1;

            var resultado2 = golpea2 && !golpea1
                ? MarcarContacto(luchador2, configuracion.Jugador2.MovimientoEnCurso(luchador2), recibido1)
                : recibido2;

            return (resultado1, resultado2);
        }

        private static bool Conecta(
            in EstadoLuchador atacante,
            in ParametrosLuchador parametrosAtacante,
            in EstadoLuchador defensor,
            in ParametrosLuchador parametrosDefensor,
            in ReglasJuggle juggle)
        {
            if (atacante.GolpeConectado || !PuedeSerGolpeado(defensor, juggle))
            {
                return false;
            }

            var cajaGolpe = parametrosAtacante.CajaGolpeActiva(atacante);

            return cajaGolpe.HasValue && cajaGolpe.Value.Intersecta(parametrosDefensor.CajaDanio(defensor));
        }

        /// <summary>Un luchador derribado, noqueado o en los primeros frames de su recuperación aérea no recibe golpes.</summary>
        public static bool PuedeSerGolpeado(in EstadoLuchador defensor, in ReglasJuggle juggle) =>
            !EstadosAccion.EsInvulnerable(defensor.Accion) && !juggle.EsInvencibleAlRecuperarse(defensor);

        /// <summary>El atacante anota que su golpe conectó, y si el rival lo bloqueó, y entra en hitstop.</summary>
        private static EstadoLuchador MarcarContacto(in EstadoLuchador atacante, in DatosMovimiento movimiento, in EstadoLuchador defensorTrasElGolpe)
        {
            var marcado = EstadosAccion.EstaBloqueando(defensorTrasElGolpe.Accion)
                ? atacante.ConGolpeBloqueado()
                : atacante.ConGolpeConectado(true);

            return marcado.ConHitstop(movimiento.Hitstop);
        }

        /// <summary>
        /// Se bloquea manteniendo la dirección que aleja del atacante. En tierra la guardia debe
        /// corresponder a la altura del golpe: los bajos se bloquean agachado y los altos de pie.
        /// En el aire se bloquea cualquier golpe durante un salto libre. Un golpe que no se bloquea
        /// deja al rival en hitstun, o en el aire si ya estaba allí o si el golpe es un lanzador.
        /// </summary>
        private static EstadoLuchador RecibirGolpe(
            in EstadoLuchador defensor,
            in EstadoLuchador atacante,
            BotonesEntrada botonesDefensor,
            in DatosMovimiento movimiento,
            in ConfiguracionCombate configuracion) =>
            RecibirGolpeDesde(defensor, atacante.Posicion.X, atacante.MiraDerecha, botonesDefensor, movimiento, configuracion);

        /// <summary>
        /// Aplica un golpe que viene de la posición indicada, sea del rival en pantalla o de un
        /// asistente. Devuelve al defensor tras bloquearlo o recibirlo.
        /// </summary>
        public static EstadoLuchador RecibirGolpeDesde(
            in EstadoLuchador defensor,
            Fijo xAtacante,
            bool atacanteMiraDerecha,
            BotonesEntrada botonesDefensor,
            in DatosMovimiento movimiento,
            in ConfiguracionCombate configuracion)
        {
            var sentidoRetroceso = defensor.Posicion.X != xAtacante
                ? (defensor.Posicion.X > xAtacante ? 1 : -1)
                : (atacanteMiraDerecha ? 1 : -1);

            var enAire = EstadosAccion.EstaEnElAire(defensor.Accion);
            var congelado = defensor.ConHitstop(movimiento.Hitstop).ConGolpeConectado(false).ConAtaque(TipoAtaque.Ninguno);

            var mantieneAtras = InterpretacionEntradas.DireccionHorizontal(botonesDefensor) == sentidoRetroceso;
            var guardiaAgachada = InterpretacionEntradas.DireccionVertical(botonesDefensor) < 0;

            if (mantieneAtras && EstadosAccion.PuedeBloquearEnAire(defensor.Accion))
            {
                return congelado
                    .ConVida(VidaTrasBloqueo(defensor, configuracion.Danio.AlBloquear(movimiento.Danio)))
                    .ConGolpesEnCombo(0)
                    .ConVelocidad(new VectorFijo(movimiento.RetrocesoAlBloquear * sentidoRetroceso, defensor.Velocidad.Y))
                    .ConAccionNueva(AccionLuchador.BloqueandoEnAire)
                    .ConAturdimiento(movimiento.Blockstun);
            }

            var bloquea = !enAire &&
                mantieneAtras &&
                EstadosAccion.PuedeBloquear(defensor.Accion) &&
                Guardia.Detiene(movimiento.Altura, guardiaAgachada);

            if (bloquea)
            {
                return congelado
                    .ConVida(VidaTrasBloqueo(defensor, configuracion.Danio.AlBloquear(movimiento.Danio)))
                    .ConGolpesEnCombo(0)
                    .ConVelocidad(new VectorFijo(movimiento.RetrocesoAlBloquear * sentidoRetroceso, Fijo.Cero))
                    .ConAccionNueva(guardiaAgachada ? AccionLuchador.BlockstunAgachado : AccionLuchador.Blockstun)
                    .ConAturdimiento(movimiento.Blockstun);
            }

            var golpesEnCombo = EstadosAccion.EstaEnCombo(defensor.Accion) ? defensor.GolpesEnCombo + 1 : 1;
            var vida = VidaTrasGolpe(defensor, configuracion.Danio.Escalar(movimiento.Danio, golpesEnCombo));
            var retroceso = movimiento.RetrocesoAlGolpear * sentidoRetroceso;
            var recuperable = vida > 0 ? configuracion.VidaRecuperable.TrasGolpe(defensor.VidaRecuperable, defensor.Vida - vida) : 0;

            var golpeado = congelado
                .ConGolpesEnCombo(golpesEnCombo)
                .ConVida(vida)
                .ConVidaRecuperable(recuperable)
                .ConFramesSinRecibirGolpe(0)
                .ConAturdimiento(0);

            if (enAire || movimiento.Lanza)
            {
                var impulso = enAire ? configuracion.Juggle.ImpulsoAlGolpear : movimiento.VelocidadLanzamiento;

                return golpeado
                    .ConVelocidad(new VectorFijo(retroceso, impulso))
                    .ConAturdimiento(configuracion.Juggle.AturdimientoCon(movimiento.HitstunAereo, golpesEnCombo))
                    .ConAccionNueva(AccionLuchador.GolpeadoEnAire);
            }

            var enTierra = golpeado.ConVelocidad(new VectorFijo(retroceso, Fijo.Cero));

            return vida > 0
                ? enTierra.ConAccionNueva(AccionLuchador.Hitstun).ConAturdimiento(movimiento.Hitstun)
                : enTierra.ConAccionNueva(AccionLuchador.Noqueado);
        }

        private static int VidaTrasGolpe(in EstadoLuchador defensor, int danio) =>
            defensor.Vida > danio ? defensor.Vida - danio : 0;

        /// <summary>El daño chip nunca noquea: deja al menos 1 punto de vida.</summary>
        private static int VidaTrasBloqueo(in EstadoLuchador defensor, int danioChip) =>
            defensor.Vida > danioChip ? defensor.Vida - danioChip : 1;
    }
}
