using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Paso puro de la simulación: mismo estado + mismas entradas = mismo resultado, siempre.
    /// No lee el reloj, no genera números aleatorios y no asigna memoria.
    /// </summary>
    public static class SimuladorCombate
    {
        public static EstadoPartida Avanzar(in EstadoPartida estado, in EntradasFrame entradas, in ConfiguracionCombate configuracion)
        {
            var luchador1 = LimitarAlEscenario(
                Caminar(estado.Luchador1, entradas.Jugador1, configuracion.Jugador1),
                configuracion.Jugador1,
                configuracion);

            var luchador2 = LimitarAlEscenario(
                Caminar(estado.Luchador2, entradas.Jugador2, configuracion.Jugador2),
                configuracion.Jugador2,
                configuracion);

            var (empujado1, empujado2) = ResolverEmpuje(luchador1, luchador2, configuracion);
            var (orientado1, orientado2) = Orientar(empujado1, empujado2);

            return new EstadoPartida(estado.Frame + 1, orientado1, orientado2);
        }

        private static EstadoLuchador Caminar(in EstadoLuchador luchador, BotonesEntrada botones, in ParametrosLuchador parametros)
        {
            if (InterpretacionEntradas.DireccionVertical(botones) < 0)
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

        private static EstadoLuchador LimitarAlEscenario(in EstadoLuchador luchador, in ParametrosLuchador parametros, in ConfiguracionCombate configuracion)
        {
            var minimo = configuracion.LimiteIzquierdo + parametros.MedioAnchoEmpuje;
            var maximo = configuracion.LimiteDerecho - parametros.MedioAnchoEmpuje;

            return luchador.ConX(Fijo.Limitar(luchador.Posicion.X, minimo, maximo));
        }

        /// <summary>
        /// Separa las cajas de empuje repartiendo el solapamiento a partes iguales. Si uno queda
        /// contra la pared, el otro absorbe todo el desplazamiento: así nadie atraviesa a nadie.
        /// </summary>
        private static (EstadoLuchador, EstadoLuchador) ResolverEmpuje(
            in EstadoLuchador luchador1,
            in EstadoLuchador luchador2,
            in ConfiguracionCombate configuracion)
        {
            var caja1 = configuracion.Jugador1.CajaEmpuje(luchador1.Posicion);
            var caja2 = configuracion.Jugador2.CajaEmpuje(luchador2.Posicion);

            if (!caja1.Intersecta(caja2))
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X != luchador2.Posicion.X
                ? luchador1.Posicion.X < luchador2.Posicion.X
                : luchador1.MiraDerecha;

            var parametrosIzquierdo = jugador1ALaIzquierda ? configuracion.Jugador1 : configuracion.Jugador2;
            var parametrosDerecho = jugador1ALaIzquierda ? configuracion.Jugador2 : configuracion.Jugador1;
            var xIzquierdo = jugador1ALaIzquierda ? luchador1.Posicion.X : luchador2.Posicion.X;
            var xDerecho = jugador1ALaIzquierda ? luchador2.Posicion.X : luchador1.Posicion.X;

            var separacionMinima = parametrosIzquierdo.MedioAnchoEmpuje + parametrosDerecho.MedioAnchoEmpuje;
            var solapamiento = separacionMinima - (xDerecho - xIzquierdo);
            var mitad = solapamiento / 2;

            xIzquierdo -= mitad;
            xDerecho += solapamiento - mitad;

            var minimoIzquierdo = configuracion.LimiteIzquierdo + parametrosIzquierdo.MedioAnchoEmpuje;
            var maximoDerecho = configuracion.LimiteDerecho - parametrosDerecho.MedioAnchoEmpuje;

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

        /// <summary>Cada luchador mira al rival; con ambos en la misma X se conserva la orientación previa.</summary>
        private static (EstadoLuchador, EstadoLuchador) Orientar(in EstadoLuchador luchador1, in EstadoLuchador luchador2)
        {
            if (luchador1.Posicion.X == luchador2.Posicion.X)
            {
                return (luchador1, luchador2);
            }

            var jugador1ALaIzquierda = luchador1.Posicion.X < luchador2.Posicion.X;

            return (luchador1.ConOrientacion(jugador1ALaIzquierda), luchador2.ConOrientacion(!jugador1ALaIzquierda));
        }
    }
}
