using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Agarres de un frame, después de los golpes: un agarre activo atrapa al rival si su caja de
    /// agarre lo alcanza y el rival es agarrable; unos frames después, el rival sale lanzado.
    /// </summary>
    /// <remarks>
    /// Un golpe gana a un agarre: quien recibe un golpe en el mismo frame ya no está agarrando.
    /// El rival sujeto escapa si, en los primeros frames, pulsa el comando del agarre (una
    /// dirección más el golpe fuerte); también se rompen dos agarres simultáneos y el agarre que
    /// atrapa a quien ya estaba intentando el suyo. Un agarre roto separa a los dos sin daño.
    /// Agarrar manteniendo la dirección contraria al rival intercambia las posiciones al lanzar,
    /// y el rival sale hacia el otro lado.
    /// </remarks>
    public static class ResolucionAgarres
    {
        public static (EstadoLuchador, EstadoLuchador) Resolver(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in EntradasFrame entradas,
            in ConfiguracionCombate configuracion)
        {
            if (Escapa(luchador2, entradas.Jugador2, luchador1, configuracion.Agarres) ||
                Escapa(luchador1, entradas.Jugador1, luchador2, configuracion.Agarres))
            {
                return Romper(luchador1, luchador2, configuracion.Agarres);
            }

            if (DebeLanzar(luchador1, configuracion.Jugador1, luchador2))
            {
                return Lanzar(luchador1, luchador2, configuracion.Jugador1.AgarreEnCurso(luchador1), configuracion);
            }

            if (DebeLanzar(luchador2, configuracion.Jugador2, luchador1))
            {
                return Invertir(Lanzar(luchador2, luchador1, configuracion.Jugador2.AgarreEnCurso(luchador2), configuracion));
            }

            var agarra1 = Atrapa(luchador1, configuracion.Jugador1, luchador2, configuracion.Jugador2);
            var agarra2 = Atrapa(luchador2, configuracion.Jugador2, luchador1, configuracion.Jugador1);

            if (!agarra1 && !agarra2)
            {
                return (luchador1, luchador2);
            }

            var seRompe = (agarra1 && agarra2) ||
                (agarra1 && IntentaAgarrar(luchador2, configuracion.Jugador2)) ||
                (agarra2 && IntentaAgarrar(luchador1, configuracion.Jugador1));

            if (seRompe)
            {
                return Romper(luchador1, luchador2, configuracion.Agarres);
            }

            return agarra1
                ? Sujetar(luchador1, luchador2)
                : Invertir(Sujetar(luchador2, luchador1));
        }

        /// <summary>El rival sujeto escapa si pulsa a tiempo una dirección más el golpe fuerte.</summary>
        private static bool Escapa(in EstadoLuchador sujeto, BotonesEntrada botones, in EstadoLuchador atacante, in ReglasAgarre reglas) =>
            sujeto.Accion == AccionLuchador.Agarrado &&
            atacante.Accion == AccionLuchador.Agarrando &&
            sujeto.FramesEnAccion <= reglas.FramesVentanaEscape &&
            sujeto.TieneAtaqueEnBufer &&
            sujeto.AtaqueEnBufer == TipoAtaque.Fuerte &&
            InterpretacionEntradas.DireccionHorizontal(botones) != 0;

        /// <summary>Está en el arranque o en los frames activos de su propio agarre, no en la recuperación de uno fallado.</summary>
        private static bool IntentaAgarrar(in EstadoLuchador luchador, in ParametrosLuchador parametros)
        {
            var agarre = parametros.AgarreEnCurso(luchador);

            return luchador.Accion == AccionLuchador.IntentandoAgarre && luchador.FramesEnAccion < agarre.Arranque + agarre.Activos;
        }

        /// <summary>Agarre roto: los dos se separan deslizándose, sin daño, y recuperan el control a la vez.</summary>
        private static (EstadoLuchador, EstadoLuchador) Romper(in EstadoLuchador luchador1, in EstadoLuchador luchador2, in ReglasAgarre reglas)
        {
            var sentido1 = luchador1.Posicion.X != luchador2.Posicion.X
                ? (luchador1.Posicion.X < luchador2.Posicion.X ? -1 : 1)
                : -luchador1.SentidoAdelante;

            return (Separado(luchador1, sentido1, reglas), Separado(luchador2, -sentido1, reglas));
        }

        private static EstadoLuchador Separado(in EstadoLuchador luchador, int sentido, in ReglasAgarre reglas) =>
            luchador
                .ConVelocidad(new VectorFijo(reglas.RetrocesoAlEscapar * sentido, Fijo.Cero))
                .ConBufer(TipoAtaque.Ninguno, 0)
                .ConAccionNueva(AccionLuchador.AgarreRoto);

        /// <summary>
        /// Se puede agarrar a un rival en tierra que no esté aturdido, derribado, noqueado,
        /// despegando ni protegido por haber salido hace poco de un aturdimiento.
        /// </summary>
        public static bool EsAgarrable(in EstadoLuchador defensor) =>
            !defensor.SinVida &&
            defensor.FramesProteccionAgarre <= 0 &&
            !EstadosAccion.EstaEnElAire(defensor.Accion) &&
            !EstadosAccion.EsInvulnerable(defensor.Accion) &&
            !EstadosAccion.ProtegeDeAgarres(defensor.Accion) &&
            defensor.Accion != AccionLuchador.PreSalto;

        private static bool Atrapa(
            in EstadoLuchador atacante,
            in ParametrosLuchador parametrosAtacante,
            in EstadoLuchador defensor,
            in ParametrosLuchador parametrosDefensor)
        {
            var caja = parametrosAtacante.CajaAgarreActiva(atacante);

            return caja.HasValue && EsAgarrable(defensor) && caja.Value.Intersecta(parametrosDefensor.CajaDanio(defensor));
        }

        /// <summary>Quien agarra pasa a sujetar y el rival queda sujeto, con su búfer vacío para que solo cuente lo que pulse después.</summary>
        private static (EstadoLuchador, EstadoLuchador) Sujetar(in EstadoLuchador atacante, in EstadoLuchador defensor)
        {
            var sujeto = defensor
                .ConVelocidad(VectorFijo.Cero)
                .ConGolpeConectado(false)
                .ConAtaque(TipoAtaque.Ninguno)
                .ConBufer(TipoAtaque.Ninguno, 0)
                .ConAturdimiento(0)
                .ConGolpesEnCombo(0)
                .ConAccionNueva(AccionLuchador.Agarrado);

            return (atacante.ConAccionNueva(AccionLuchador.Agarrando), sujeto);
        }

        private static (EstadoLuchador, EstadoLuchador) Invertir((EstadoLuchador, EstadoLuchador) par) => (par.Item2, par.Item1);

        private static bool DebeLanzar(in EstadoLuchador atacante, in ParametrosLuchador parametros, in EstadoLuchador defensor) =>
            atacante.Accion == AccionLuchador.Agarrando &&
            defensor.Accion == AccionLuchador.Agarrado &&
            atacante.FramesEnAccion == parametros.AgarreEnCurso(atacante).FramesSujecion;

        /// <summary>
        /// El rival sujeto recibe el daño del agarre, que cuenta como primer golpe de un combo, y
        /// sale por el aire hacia donde mira quien lo agarró. En un agarre hacia atrás, antes los
        /// dos intercambian su posición y quien agarra se da la vuelta.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) Lanzar(in EstadoLuchador atacante, in EstadoLuchador defensor, in DatosAgarre agarre, in ConfiguracionCombate configuracion)
        {
            var invierte = atacante.AgarraHaciaAtras;
            var miraDerecha = invierte ? !atacante.MiraDerecha : atacante.MiraDerecha;
            var danio = configuracion.Danio.Escalar(agarre.Danio, 1);
            var vida = defensor.Vida > danio ? defensor.Vida - danio : 0;
            var recuperable = vida > 0 ? configuracion.VidaRecuperable.TrasGolpe(defensor.VidaRecuperable, defensor.Vida - vida) : 0;

            var lanzador = atacante
                .ConX(invierte ? defensor.Posicion.X : atacante.Posicion.X)
                .ConOrientacion(miraDerecha)
                .ConBarra(configuracion.Barra.AlConectar(atacante.Barra, agarre.Danio));

            var lanzado = defensor
                .ConX(invierte ? atacante.Posicion.X : defensor.Posicion.X)
                .ConVida(vida)
                .ConVidaRecuperable(recuperable)
                .ConFramesSinRecibirGolpe(0)
                .ConBarra(configuracion.Barra.AlRecibir(defensor.Barra, agarre.Danio))
                .ConGolpesEnCombo(1)
                .ConVelocidad(new VectorFijo(agarre.VelocidadLanzamiento.X * (miraDerecha ? 1 : -1), agarre.VelocidadLanzamiento.Y))
                .ConAccionNueva(AccionLuchador.GolpeadoEnAire)
                .ConAturdimiento(agarre.AturdimientoLanzado);

            return (lanzador, lanzado);
        }
    }
}
