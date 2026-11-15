using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: presalto de 4 frames, velocidad vertical 0.4, gravedad 0.02,
    /// altura máxima 4.2 y 41 frames en el aire.
    /// </summary>
    public class PruebasSalto
    {
        private const BotonesEntrada ArribaAdelante = BotonesEntrada.Arriba | BotonesEntrada.Derecha;
        private const BotonesEntrada ArribaAtras = BotonesEntrada.Arriba | BotonesEntrada.Izquierda;

        private static EstadoPartida SimularHastaAccion(EstadoPartida estado, BotonesEntrada jugador1, AccionLuchador accion, int maximoFrames)
        {
            for (var i = 0; i < maximoFrames; i++)
            {
                if (estado.Luchador1.Accion == accion)
                {
                    return estado;
                }

                estado = SimularJugador1(estado, jugador1, 1);
            }

            throw new InvalidOperationException($"El jugador 1 no llegó a {accion} en {maximoFrames} frames.");
        }

        [Test]
        public void ElPresaltoDuraLosFramesConfiguradosYNoSeparaDelSuelo()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 4);

            Assert.AreEqual(AccionLuchador.PreSalto, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Posicion.Y);

            estado = SimularJugador1(estado, BotonesEntrada.Arriba, 1);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(2, 5), estado.Luchador1.Velocidad.Y);
        }

        [Test]
        public void ElSaltoNeutralSubeHastaSuAlturaYVuelveAlMismoSitio()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 1);
            var alturaMaxima = Fijo.Cero;

            for (var i = 0; i < 80; i++)
            {
                estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);
                alturaMaxima = Fijo.Max(alturaMaxima, estado.Luchador1.Posicion.Y);
            }

            Assert.AreEqual(Fijo.DesdeEntero(-5), estado.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Posicion.Y);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.IsTrue(alturaMaxima > Fijo.DesdeEntero(4) && alturaMaxima < Fijo.DesdeFraccion(43, 10), $"Altura máxima {alturaMaxima}");
        }

        [Test]
        public void ElSaltoAdelanteYElSaltoAtrasTienenSuDireccion()
        {
            var adelante = SimularJugador1(Inicial(-5, 5), ArribaAdelante, 5);
            var atras = SimularJugador1(Inicial(-5, 5), ArribaAtras, 5);

            Assert.AreEqual(Fijo.DesdeFraccion(1, 10), adelante.Luchador1.Velocidad.X);
            Assert.AreEqual(-Fijo.DesdeFraccion(1, 10), atras.Luchador1.Velocidad.X);
        }

        [Test]
        public void LaDireccionSeEligeEnElFrameDeDespegue()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 1);
            estado = SimularJugador1(estado, ArribaAdelante, 4);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 10), estado.Luchador1.Velocidad.X);
        }

        [Test]
        public void EnElAireLaDireccionNoCambia()
        {
            var estado = SimularJugador1(Inicial(-5, 5), ArribaAdelante, 5);
            estado = SimularJugador1(estado, BotonesEntrada.Izquierda, 10);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 10), estado.Luchador1.Velocidad.X);
        }

        [Test]
        public void ElAterrizajeQuitaElControlDuranteSusFrames()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 1);
            estado = SimularHastaAccion(estado, BotonesEntrada.Ninguno, AccionLuchador.Aterrizaje, 100);
            var xAlAterrizar = estado.Luchador1.Posicion.X;

            estado = SimularJugador1(estado, BotonesEntrada.Derecha, 2);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Luchador1.Accion);
            Assert.AreEqual(xAlAterrizar, estado.Luchador1.Posicion.X);

            estado = SimularJugador1(estado, BotonesEntrada.Derecha, 1);

            Assert.AreEqual(AccionLuchador.CaminandoAdelante, estado.Luchador1.Accion);
        }

        [Test]
        public void UnSaltoAltoPasaPorEncimaDelRivalYGiraAlAterrizar()
        {
            var estado = SimularJugador1(Inicial(-1, 1), ArribaAdelante, 5);
            var mantuvoOrientacionEnElAire = true;

            for (var i = 0; i < 60; i++)
            {
                estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

                if (estado.Luchador1.Accion == AccionLuchador.EnAire && estado.Luchador1.Posicion.X > estado.Luchador2.Posicion.X)
                {
                    mantuvoOrientacionEnElAire &= estado.Luchador1.MiraDerecha;
                }
            }

            Assert.IsTrue(estado.Luchador1.Posicion.X > estado.Luchador2.Posicion.X, "El jugador 1 debe quedar del otro lado");
            Assert.IsTrue(mantuvoOrientacionEnElAire, "En el aire no se gira");
            Assert.IsFalse(estado.Luchador1.MiraDerecha);
            Assert.IsTrue(estado.Luchador2.MiraDerecha);
        }

        [Test]
        public void LaParedTambienLimitaEnElAire()
        {
            var estado = SimularJugador1(Inicial(-9, 5), ArribaAtras, 5);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 80);

            Assert.AreEqual(Fijo.DesdeEntero(-10) + Fijo.Medio, estado.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Posicion.Y);
        }
    }
}
