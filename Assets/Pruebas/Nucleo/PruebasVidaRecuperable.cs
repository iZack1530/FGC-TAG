using FgcTag.Nucleo.Combate;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: la mitad del daño de un golpe queda como vida recuperable, cada
    /// golpe nuevo destruye un cuarto de la acumulada y, tras 30 frames sin recibir golpes, se
    /// regeneran 2 por frame. Un golpe débil (100 de daño) pulsado en el frame 1 conecta en el 5.
    /// </summary>
    public class PruebasVidaRecuperable
    {
        private const int FrameDeImpacto = 5;

        private static EstadoPartida GolpeDebilHasta(EstadoPartida inicial, BotonesEntrada jugador2, int frame)
        {
            var estado = Simular(inicial, BotonesEntrada.Debil, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, frame - 1);
        }

        [Test]
        public void LasReglasConservanParteDeLaVidaAzulYSumanParteDelDanio()
        {
            Assert.AreEqual(50, Recuperable.TrasGolpe(0, 100));
            Assert.AreEqual(127, Recuperable.TrasGolpe(50, 180));
            Assert.AreEqual(3, ReglasVidaRecuperable.Curacion(3, 5));
            Assert.AreEqual(5, ReglasVidaRecuperable.Curacion(40, 5));
        }

        [Test]
        public void UnGolpeDejaLaMitadDeSuDanioComoVidaRecuperable()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto);

            Assert.AreEqual(VidaMaxima - 100, estado.Luchador2.Vida);
            Assert.AreEqual(50, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void ElDanioChipNoDejaVidaRecuperable()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Derecha, FrameDeImpacto);

            Assert.AreEqual(VidaMaxima - 10, estado.Luchador2.Vida);
            Assert.AreEqual(0, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void OtroGolpeDestruyeParteDeLaVidaRecuperableAcumulada()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto);
            estado = Simular(estado, BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 7 + 7);

            Assert.AreEqual(2, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(VidaMaxima - 100 - 180, estado.Luchador2.Vida);
            Assert.AreEqual(127, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void LaRegeneracionEsperaUnosFramesSinRecibirGolpes()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto + 29);

            Assert.AreEqual(VidaMaxima - 100, estado.Luchador2.Vida);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(VidaMaxima - 98, estado.Luchador2.Vida);
            Assert.AreEqual(48, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void LaRegeneracionSoloDevuelveLaVidaRecuperable()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto + 200);

            Assert.AreEqual(VidaMaxima - 50, estado.Luchador2.Vida);
            Assert.AreEqual(0, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void RecibirOtroGolpeReiniciaLaEspera()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto + 25);
            estado = GolpeDebilHasta(estado, BotonesEntrada.Ninguno, FrameDeImpacto);
            var vidaTrasElSegundoGolpe = estado.Luchador2.Vida;

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 29);

            Assert.AreEqual(vidaTrasElSegundoGolpe, estado.Luchador2.Vida);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(vidaTrasElSegundoGolpe + 2, estado.Luchador2.Vida);
        }

        [Test]
        public void UnNoqueadoNoConservaVidaRecuperable()
        {
            var estado = GolpeDebilHasta(ConVidas(Inicial(0, 1), VidaMaxima, 100), BotonesEntrada.Ninguno, FrameDeImpacto + 100);

            Assert.AreEqual(0, estado.Luchador2.Vida);
            Assert.AreEqual(0, estado.Luchador2.VidaRecuperable);
        }

        [Test]
        public void ConLaRondaTerminadaNadieRegenera()
        {
            var inicial = ConVidas(Inicial(0, 1), 600, 100);
            var conAzul = new EstadoPartida(inicial.Frame, inicial.Luchador1.ConVidaRecuperable(200), inicial.Luchador2, inicial.FramesRestantes, inicial.Resultado);
            var estado = GolpeDebilHasta(conAzul, BotonesEntrada.Ninguno, FrameDeImpacto);
            var vidaAlTerminar = estado.Luchador1.Vida;

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 120);

            Assert.IsTrue(estado.RondaTerminada);
            Assert.AreEqual(vidaAlTerminar, estado.Luchador1.Vida);
        }
    }
}
