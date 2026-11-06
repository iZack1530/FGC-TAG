using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Paso puro de la simulación: mismo estado + mismas entradas = mismo resultado, siempre.
    /// No lee el reloj, no genera números aleatorios y no asigna memoria.
    /// </summary>
    /// <remarks>
    /// Orden de cada frame: 1) cada luchador actúa según su acción y sus entradas;
    /// 2) se aplican las paredes; 3) se separan las cajas de empuje; 4) los luchadores
    /// que pueden girar se orientan hacia el rival.
    /// </remarks>
    public static class SimuladorCombate
    {
        public static EstadoPartida Avanzar(in EstadoPartida estado, in EntradasFrame entradas, in ConfiguracionCombate configuracion)
        {
            var luchador1 = LimitarAlEscenario(
                Actuar(estado.Luchador1, entradas.Jugador1, configuracion.Jugador1),
                configuracion.Jugador1,
                configuracion);

            var luchador2 = LimitarAlEscenario(
                Actuar(estado.Luchador2, entradas.Jugador2, configuracion.Jugador2),
                configuracion.Jugador2,
                configuracion);

            var (empujado1, empujado2) = ResolverEmpuje(luchador1, luchador2, configuracion);
            var (orientado1, orientado2) = Orientar(empujado1, empujado2);

            return new EstadoPartida(estado.Frame + 1, orientado1, orientado2);
        }

        private static EstadoLuchador Actuar(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            switch (luchador.Accion)
            {
                case AccionLuchador.EnAire:
                    return MoverEnAire(luchador, parametros.Salto);

                case AccionLuchador.PreSalto:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesPreSalto
                        ? Despegar(luchador, botones, parametros.Salto)
                        : luchador.ConAccion(AccionLuchador.PreSalto);

                case AccionLuchador.Aterrizaje:
                    return luchador.FramesEnAccion + 1 >= parametros.Salto.FramesAterrizaje
                        ? ControlarEnTierra(luchador, botones, parametros)
                        : luchador.ConAccion(AccionLuchador.Aterrizaje);

                default:
                    return ControlarEnTierra(luchador, botones, parametros);
            }
        }

        private static EstadoLuchador ControlarEnTierra(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            var vertical = InterpretacionEntradas.DireccionVertical(botones);

            if (vertical > 0)
            {
                return luchador.ConAccion(AccionLuchador.PreSalto);
            }

            if (vertical < 0)
            {
                return luchador.ConAccion(AccionLuchador.Agachado);
            }

            var direccion = InterpretacionEntradas.DireccionHorizontal(botones);

            if (direccion == 0)
            {
                return luchador.ConAccion(AccionLuchador.Quieto);
            }

            var haciaAdelante = direccion == luchador.SentidoAdelante;
            var velocidad = haciaAdelante ? parametros.VelocidadAdelante : parametros.VelocidadAtras;
            var accion = haciaAdelante ? AccionLuchador.CaminandoAdelante : AccionLuchador.CaminandoAtras;

            return luchador
                .ConAccion(accion)
                .ConX(luchador.Posicion.X + velocidad * direccion);
        }

        /// <summary>
        /// La dirección del salto se lee en el frame de despegue, no al pulsar arriba: así el
        /// jugador puede corregirla durante el presalto. Una vez en el aire ya no cambia.
        /// </summary>
        private static EstadoLuchador Despegar(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosSalto salto)
        {
            var direccion = InterpretacionEntradas.DireccionHorizontal(botones);
            var velocidadHorizontal = direccion == 0
                ? Fijo.Cero
                : (direccion == luchador.SentidoAdelante ? salto.VelocidadAdelante : salto.VelocidadAtras) * direccion;

            return luchador
                .ConAccion(AccionLuchador.EnAire)
                .ConVelocidad(new VectorFijo(velocidadHorizontal, salto.VelocidadVertical));
        }

        /// <summary>Integración explícita: primero se mueve con la velocidad actual y después la gravedad la reduce.</summary>
        private static EstadoLuchador MoverEnAire(in EstadoLuchador luchador, in ParametrosSalto salto)
        {
            var posicion = luchador.Posicion + luchador.Velocidad;

            if (posicion.Y <= Fijo.Cero)
            {
                var accionAlCaer = salto.FramesAterrizaje > 0 ? AccionLuchador.Aterrizaje : AccionLuchador.Quieto;

                return new EstadoLuchador(posicion.ConY(Fijo.Cero), VectorFijo.Cero, luchador.MiraDerecha, accionAlCaer, 0);
            }

            var velocidad = luchador.Velocidad.ConY(luchador.Velocidad.Y - salto.Gravedad);

            return luchador
                .ConAccion(AccionLuchador.EnAire)
                .ConPosicion(posicion)
                .ConVelocidad(velocidad);
        }

        private static EstadoLuchador LimitarAlEscenario(in EstadoLuchador luchador, in ParametrosLuchador parametros, in ConfiguracionCombate configuracion)
        {
            var medioAncho = parametros.CajaEmpuje(luchador).MedioAncho;
            var minimo = configuracion.LimiteIzquierdo + medioAncho;
            var maximo = configuracion.LimiteDerecho - medioAncho;

            return luchador.ConX(Fijo.Limitar(luchador.Posicion.X, minimo, maximo));
        }

        /// <summary>
        /// Separa las cajas de empuje en horizontal repartiendo el solapamiento a partes iguales.
        /// Si uno queda contra la pared, el otro absorbe todo el desplazamiento: nadie atraviesa a nadie.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) ResolverEmpuje(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in ConfiguracionCombate configuracion)
        {
            var caja1 = configuracion.Jugador1.CajaEmpuje(luchador1);
            var caja2 = configuracion.Jugador2.CajaEmpuje(luchador2);

            if (!caja1.Intersecta(caja2))
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X != luchador2.Posicion.X
                ? luchador1.Posicion.X < luchador2.Posicion.X
                : luchador1.MiraDerecha;

            var medioAnchoIzquierdo = jugador1ALaIzquierda ? caja1.MedioAncho : caja2.MedioAncho;
            var medioAnchoDerecho = jugador1ALaIzquierda ? caja2.MedioAncho : caja1.MedioAncho;
            var xIzquierdo = jugador1ALaIzquierda ? luchador1.Posicion.X : luchador2.Posicion.X;
            var xDerecho = jugador1ALaIzquierda ? luchador2.Posicion.X : luchador1.Posicion.X;

            var separacionMinima = medioAnchoIzquierdo + medioAnchoDerecho;
            var solapamiento = separacionMinima - (xDerecho - xIzquierdo);
            var mitad = solapamiento / 2;

            xIzquierdo -= mitad;
            xDerecho += solapamiento - mitad;

            var minimoIzquierdo = configuracion.LimiteIzquierdo + medioAnchoIzquierdo;
            var maximoDerecho = configuracion.LimiteDerecho - medioAnchoDerecho;

            if (xIzquierdo < minimoIzquierdo)
            {
                xIzquierdo = minimoIzquierdo;
                xDerecho = xIzquierdo + separacionMinima;
            }

            if (xDerecho > maximoDerecho)
            {
                xDerecho = maximoDerecho;
                xIzquierdo = xDerecho - separacionMinima;
            }

            return jugador1ALaIzquierda
                ? (luchador1.ConX(xIzquierdo), luchador2.ConX(xDerecho))
                : (luchador1.ConX(xDerecho), luchador2.ConX(xIzquierdo));
        }

        /// <summary>
        /// Solo giran los luchadores en tierra: quien salta por encima del rival conserva su
        /// orientación hasta aterrizar. Con ambos en la misma X nadie gira.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) Orientar(in EstadoLuchador luchador1, in EstadoLuchador luchador2)
        {
            if (luchador1.Posicion.X == luchador2.Posicion.X)
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X < luchador2.Posicion.X;

            var orientado1 = EstadosAccion.PuedeGirar(luchador1.Accion) ? luchador1.ConOrientacion(jugador1ALaIzquierda) : luchador1;
            var orientado2 = EstadosAccion.PuedeGirar(luchador2.Accion) ? luchador2.ConOrientacion(!jugador1ALaIzquierda) : luchador2;

            return (orientado1, orientado2);
        }
    }
}
