using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Agarres de un frame, después de los golpes: un agarre activo atrapa al rival si su caja de
    /// agarre lo alcanza y el rival es agarrable; unos frames después, el rival sale lanzado.
    /// </summary>
    /// <remarks>
    /// Un golpe gana a un agarre: quien recibe un golpe en el mismo frame ya no está agarrando.
    /// Si los dos se agarran en el mismo frame, los agarres se anulan y ambos fallan.
    /// Agarrar manteniendo la dirección contraria al rival intercambia las posiciones, y el rival
    /// sale lanzado hacia el otro lado.
    /// </remarks>
    public static class ResolucionAgarres
    {
        public static (EstadoLuchador, EstadoLuchador) Resolver(in EstadoLuchador luchador1, in EstadoLuchador luchador2, in ConfiguracionCombate configuracion)
        {
            if (DebeLanzar(luchador1, configuracion.Jugador1, luchador2))
            {
                return (ConBarraAlConectar(luchador1, configuracion.Jugador1, configuracion.Barra), Lanzado(luchador2, luchador1, configuracion.Jugador1.AgarreEnCurso(luchador1), configuracion));
            }

            if (DebeLanzar(luchador2, configuracion.Jugador2, luchador1))
            {
                return (Lanzado(luchador1, luchador2, configuracion.Jugador2.AgarreEnCurso(luchador2), configuracion), ConBarraAlConectar(luchador2, configuracion.Jugador2, configuracion.Barra));
            }

            var agarra1 = Atrapa(luchador1, configuracion.Jugador1, luchador2, configuracion.Jugador2);
            var agarra2 = Atrapa(luchador2, configuracion.Jugador2, luchador1, configuracion.Jugador1);

            if (agarra1 == agarra2)
            {
                return (luchador1, luchador2);
            }

            return agarra1
                ? Sujetar(luchador1, luchador2)
                : Invertir(Sujetar(luchador2, luchador1));
        }

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

        /// <summary>
        /// Quien agarra pasa a sujetar y el rival queda sujeto. En un agarre hacia atrás los dos
        /// intercambian su posición y quien agarra se da la vuelta.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) Sujetar(in EstadoLuchador atacante, in EstadoLuchador defensor)
        {
            var xAtacante = atacante.AgarraHaciaAtras ? defensor.Posicion.X : atacante.Posicion.X;
            var xDefensor = atacante.AgarraHaciaAtras ? atacante.Posicion.X : defensor.Posicion.X;

            var sujetando = atacante
                .ConX(xAtacante)
                .ConOrientacion(atacante.AgarraHaciaAtras ? !atacante.MiraDerecha : atacante.MiraDerecha)
                .ConAccionNueva(AccionLuchador.Agarrando);

            var sujeto = defensor
                .ConX(xDefensor)
                .ConVelocidad(VectorFijo.Cero)
                .ConGolpeConectado(false)
                .ConAtaque(TipoAtaque.Ninguno)
                .ConBufer(TipoAtaque.Ninguno, 0)
                .ConAturdimiento(0)
                .ConGolpesEnCombo(0)
                .ConAccionNueva(AccionLuchador.Agarrado);

            return (sujetando, sujeto);
        }

        private static (EstadoLuchador, EstadoLuchador) Invertir((EstadoLuchador, EstadoLuchador) par) => (par.Item2, par.Item1);

        private static bool DebeLanzar(in EstadoLuchador atacante, in ParametrosLuchador parametros, in EstadoLuchador defensor) =>
            atacante.Accion == AccionLuchador.Agarrando &&
            defensor.Accion == AccionLuchador.Agarrado &&
            atacante.FramesEnAccion == parametros.AgarreEnCurso(atacante).FramesSujecion;

        private static EstadoLuchador ConBarraAlConectar(in EstadoLuchador atacante, in ParametrosLuchador parametros, in ReglasBarra barra) =>
            atacante.ConBarra(barra.AlConectar(atacante.Barra, parametros.AgarreEnCurso(atacante).Danio));

        /// <summary>
        /// El rival sujeto recibe el daño del agarre, que cuenta como primer golpe de un combo, y
        /// sale por el aire hacia donde mira quien lo agarró.
        /// </summary>
        private static EstadoLuchador Lanzado(in EstadoLuchador defensor, in EstadoLuchador atacante, in DatosAgarre agarre, in ConfiguracionCombate configuracion)
        {
            var danio = configuracion.Danio.Escalar(agarre.Danio, 1);
            var vida = defensor.Vida > danio ? defensor.Vida - danio : 0;
            var recuperable = vida > 0 ? configuracion.VidaRecuperable.TrasGolpe(defensor.VidaRecuperable, defensor.Vida - vida) : 0;

            return defensor
                .ConVida(vida)
                .ConVidaRecuperable(recuperable)
                .ConFramesSinRecibirGolpe(0)
                .ConBarra(configuracion.Barra.AlRecibir(defensor.Barra, agarre.Danio))
                .ConGolpesEnCombo(1)
                .ConVelocidad(new VectorFijo(agarre.VelocidadLanzamiento.X * atacante.SentidoAdelante, agarre.VelocidadLanzamiento.Y))
                .ConAccionNueva(AccionLuchador.GolpeadoEnAire)
                .ConAturdimiento(agarre.AturdimientoLanzado);
        }
    }
}
