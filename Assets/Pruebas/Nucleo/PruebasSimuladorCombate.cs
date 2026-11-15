using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    public class PruebasSimuladorCombate
    {
        /// <summary>Entradas pseudoaleatorias reproducibles (generador congruencial lineal).</summary>
        private static EntradasFrame EntradasPseudoaleatorias(ref uint semilla)
        {
            semilla = unchecked(semilla * 1103515245u + 12345u);
            var jugador1 = (BotonesEntrada)((semilla >> 16) & 0x3FF);
            semilla = unchecked(semilla * 1103515245u + 12345u);
            var jugador2 = (BotonesEntrada)((semilla >> 16) & 0x3FF);

            return new EntradasFrame(jugador1, jugador2);
        }

        private static EstadoPartida SimularPseudoaleatorio(EstadoPartida estado, ref uint semilla, int frames)
        {
            for (var i = 0; i < frames; i++)
            {
                estado = SimuladorCombate.Avanzar(estado, EntradasPseudoaleatorias(ref semilla), Configuracion);
            }

            return estado;
        }

        [Test]
        public void CadaPasoAvanzaUnFrame()
        {
            Assert.AreEqual(3, Simular(Inicial(-2, 2), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 3).Frame);
        }

        [Test]
        public void EstadoInicialHaceQueLosLuchadoresSeMiren()
        {
            var estado = Inicial(4, -4);

            Assert.IsFalse(estado.Luchador1.MiraDerecha);
            Assert.IsTrue(estado.Luchador2.MiraDerecha);
        }

        [Test]
        public void CaminarHaciaElRivalUsaLaVelocidadAdelante()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.DesdeEntero(-5) + Fijo.DesdeFraccion(1, 4), estado.Luchador1.Posicion.X);
            Assert.AreEqual(AccionLuchador.CaminandoAdelante, estado.Luchador1.Accion);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void MantenerLaAccionAvanzaSuContador()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 4);

            Assert.AreEqual(3, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void CaminarAlejandoseUsaLaVelocidadAtras()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Izquierda, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.DesdeEntero(-5) - Fijo.DesdeFraccion(1, 8), estado.Luchador1.Posicion.X);
            Assert.AreEqual(AccionLuchador.CaminandoAtras, estado.Luchador1.Accion);
        }

        [Test]
        public void ParaElJugadorDeLaDerechaAdelanteEsLaIzquierda()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Ninguno, BotonesEntrada.Izquierda, 1);

            Assert.AreEqual(Fijo.DesdeEntero(5) - Fijo.DesdeFraccion(1, 4), estado.Luchador2.Posicion.X);
            Assert.AreEqual(AccionLuchador.CaminandoAdelante, estado.Luchador2.Accion);
        }

        [Test]
        public void IzquierdaYDerechaALaVezSonNeutral()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Izquierda | BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.DesdeEntero(-5), estado.Luchador1.Posicion.X);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void AbajoAgachaSinDesplazar()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Abajo | BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.DesdeEntero(-5), estado.Luchador1.Posicion.X);
            Assert.AreEqual(AccionLuchador.Agachado, estado.Luchador1.Accion);
        }

        [Test]
        public void LaParedDetieneAlLuchador()
        {
            var estado = Simular(Inicial(-9, 5), BotonesEntrada.Izquierda, BotonesEntrada.Ninguno, 30);

            Assert.AreEqual(Fijo.DesdeEntero(-10) + Fijo.Medio, estado.Luchador1.Posicion.X);
        }

        [Test]
        public void LasCajasDeEmpujeNuncaQuedanSolapadas()
        {
            var estado = Inicial(-1, 1);

            for (var i = 0; i < 120; i++)
            {
                estado = Simular(estado, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 1);

                var caja1 = Parametros.CajaEmpuje(estado.Luchador1);
                var caja2 = Parametros.CajaEmpuje(estado.Luchador2);
                Assert.IsFalse(caja1.Intersecta(caja2), $"Solapamiento en el frame {estado.Frame}");
            }

            Assert.IsTrue(estado.Luchador2.Posicion.X > Fijo.DesdeEntero(1), "El rival debe ser empujado");
        }

        [Test]
        public void ContraLaParedElEmpujeDetieneAlAtacante()
        {
            var estado = Simular(Inicial(7, 9), BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 100);

            Assert.AreEqual(Fijo.DesdeEntero(10) - Fijo.Medio, estado.Luchador2.Posicion.X);
            Assert.AreEqual(Fijo.DesdeEntero(10) - Fijo.Medio - Fijo.Uno, estado.Luchador1.Posicion.X);
        }

        [Test]
        public void LosLuchadoresSeGiranParaMirarAlRival()
        {
            var malOrientado = new EstadoPartida(
                0,
                EstadoLuchador.EnReposo(VectorFijo.DesdeEnteros(-2, 0), false),
                EstadoLuchador.EnReposo(VectorFijo.DesdeEnteros(2, 0), true));

            var estado = SimuladorCombate.Avanzar(malOrientado, EntradasFrame.Vacias, Configuracion);

            Assert.IsTrue(estado.Luchador1.MiraDerecha);
            Assert.IsFalse(estado.Luchador2.MiraDerecha);
        }

        [Test]
        public void MismasEntradasProducenElMismoEstado()
        {
            var semillaA = 2026u;
            var semillaB = 2026u;

            var a = SimularPseudoaleatorio(Inicial(-2, 2), ref semillaA, 600);
            var b = SimularPseudoaleatorio(Inicial(-2, 2), ref semillaB, 600);

            Assert.AreEqual(a.CalcularSumaVerificacion().Valor, b.CalcularSumaVerificacion().Valor);
        }

        /// <summary>
        /// Simula un rollback: se predice mal el futuro, se restaura el estado guardado y se
        /// resimula con las entradas reales. El resultado debe ser idéntico a no haber fallado.
        /// </summary>
        [Test]
        public void RestaurarYResimularDaElMismoResultadoQueSimularDirecto()
        {
            var semillaDirecta = 77u;
            var directo = SimularPseudoaleatorio(Inicial(-2, 2), ref semillaDirecta, 600);

            var semilla = 77u;
            var guardado = SimularPseudoaleatorio(Inicial(-2, 2), ref semilla, 300);
            var semillaGuardada = semilla;

            var semillaEquivocada = 999u;
            SimularPseudoaleatorio(guardado, ref semillaEquivocada, 300);

            var resimulado = SimularPseudoaleatorio(guardado, ref semillaGuardada, 300);

            Assert.AreEqual(directo.CalcularSumaVerificacion().Valor, resimulado.CalcularSumaVerificacion().Valor);
        }

        /// <summary>
        /// Valor dorado: si cambia, cambió el comportamiento de la simulación y las repeticiones
        /// grabadas dejan de valer. Actualizarlo exige una entrada en el registro de decisiones.
        /// </summary>
        [Test]
        public void RepeticionDeReferenciaConservaSuSumaDeVerificacion()
        {
            var semilla = 1530u;
            var estado = SimularPseudoaleatorio(Inicial(-2, 2), ref semilla, 600);

            Assert.AreEqual("7CA96CC36311B220", estado.CalcularSumaVerificacion().ToString());
        }
    }
}
