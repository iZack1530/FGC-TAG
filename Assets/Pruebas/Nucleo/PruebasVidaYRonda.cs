using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: vida 1000; Débil 100, Medio 200 y Fuerte 300 de daño; cada
    /// golpe del combo escala 10 puntos menos. El golpe débil pulsado en el frame 1 conecta en el 5.
    /// </summary>
    public class PruebasVidaYRonda
    {
        private const int FrameDeImpacto = 5;

        private static EstadoPartida GolpeDebilHastaElImpacto(EstadoPartida inicial, BotonesEntrada jugador2)
        {
            var estado = Simular(inicial, BotonesEntrada.Debil, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, FrameDeImpacto - 1);
        }

        /// <summary>El jugador 1 encadena Débil, Medio y Fuerte pulsando el siguiente botón en cuanto conecta el anterior.</summary>
        /// <returns>La vida más baja del jugador 2 durante el combo: después empieza a regenerar su vida recuperable.</returns>
        private static int VidaMinimaTrasElCombo(EstadoPartida inicial, int frames)
        {
            var vidaMinima = inicial.Luchador2.Vida;
            var siguientes = new[] { BotonesEntrada.Medio, BotonesEntrada.Fuerte };
            var indice = 0;
            var enHitstop = false;
            var estado = Simular(inicial, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            for (var i = 0; i < frames; i++)
            {
                var acabaDeConectar = estado.Luchador1.EnHitstop && !enHitstop;
                var botones = acabaDeConectar && indice < siguientes.Length ? siguientes[indice++] : BotonesEntrada.Ninguno;
                enHitstop = estado.Luchador1.EnHitstop;
                estado = Simular(estado, botones, BotonesEntrada.Ninguno, 1);
                vidaMinima = Math.Min(vidaMinima, estado.Luchador2.Vida);
            }

            return vidaMinima;
        }

        [Test]
        public void LaRondaEmpiezaConVidaLlenaYRelojCompleto()
        {
            var estado = Inicial(-2, 2);

            Assert.AreEqual(VidaMaxima, estado.Luchador1.Vida);
            Assert.AreEqual(VidaMaxima, estado.Luchador2.Vida);
            Assert.AreEqual(FramesPorRonda, estado.FramesRestantes);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
        }

        [Test]
        public void ElEscaladoBajaPorGolpeHastaElMinimo()
        {
            Assert.AreEqual(100, Danio.Porcentaje(1));
            Assert.AreEqual(90, Danio.Porcentaje(2));
            Assert.AreEqual(30, Danio.Porcentaje(8));
            Assert.AreEqual(30, Danio.Porcentaje(20));
            Assert.AreEqual(94, Danio.Escalar(105, 2));
        }

        [Test]
        public void UnGolpeQuitaSuDanioBase()
        {
            var estado = GolpeDebilHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno);

            Assert.AreEqual(VidaMaxima - 100, estado.Luchador2.Vida);
            Assert.AreEqual(VidaMaxima, estado.Luchador1.Vida);
        }

        [Test]
        public void UnGolpeBloqueadoSoloHaceDanioChip()
        {
            var estado = GolpeDebilHastaElImpacto(Inicial(0, 1), BotonesEntrada.Derecha);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
            Assert.AreEqual(10, Danio.AlBloquear(100));
            Assert.AreEqual(VidaMaxima - 10, estado.Luchador2.Vida);
        }

        [Test]
        public void ElDanioChipNuncaNoquea()
        {
            var estado = GolpeDebilHastaElImpacto(ConVidas(Inicial(0, 1), VidaMaxima, 5), BotonesEntrada.Derecha);

            Assert.AreEqual(1, estado.Luchador2.Vida);
            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
        }

        [Test]
        public void ElComboEscalaElDanioDeCadaGolpe()
        {
            Assert.AreEqual(VidaMaxima - (100 + 180 + 240), VidaMinimaTrasElCombo(Inicial(0, 1), 150));
        }

        [Test]
        public void QuedarseSinVidaNoqueaYTerminaLaRondaEnElMismoFrame()
        {
            var estado = GolpeDebilHastaElImpacto(ConVidas(Inicial(0, 1), VidaMaxima, 100), BotonesEntrada.Ninguno);

            Assert.AreEqual(0, estado.Luchador2.Vida);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador2.Accion);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.AreEqual(8, estado.Luchador2.FramesHitstop);
        }

        [Test]
        public void LaVidaNuncaEsNegativa()
        {
            var estado = GolpeDebilHastaElImpacto(ConVidas(Inicial(0, 1), VidaMaxima, 40), BotonesEntrada.Ninguno);

            Assert.AreEqual(0, estado.Luchador2.Vida);
        }

        [Test]
        public void ElJugador2TambienPuedeGanar()
        {
            var estado = Simular(ConVidas(Inicial(0, 1), 100, VidaMaxima), BotonesEntrada.Ninguno, BotonesEntrada.Debil, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1);

            Assert.AreEqual(ResultadoRonda.GanaJugador2, estado.Resultado);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador1.Accion);
        }

        [Test]
        public void ElDobleNocautEsEmpate()
        {
            var estado = Simular(ConVidas(Inicial(0, 1), 100, 100), BotonesEntrada.Debil, BotonesEntrada.Debil, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1);

            Assert.AreEqual(ResultadoRonda.Empate, estado.Resultado);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador2.Accion);
        }

        [Test]
        public void TrasLaRondaSeIgnoranLasEntradasYElRelojSeDetiene()
        {
            var final = GolpeDebilHastaElImpacto(ConVidas(Inicial(0, 1), VidaMaxima, 100), BotonesEntrada.Ninguno);
            var estado = Simular(final, BotonesEntrada.Derecha | BotonesEntrada.Fuerte, BotonesEntrada.Arriba, 120);

            Assert.AreEqual(final.Frame + 120, estado.Frame);
            Assert.AreEqual(final.FramesRestantes, estado.FramesRestantes);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.AreEqual(final.Luchador1.Posicion, estado.Luchador1.Posicion);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador2.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Velocidad.X);
            Assert.IsTrue(estado.Luchador2.Posicion.X > final.Luchador2.Posicion.X, "El noqueado retrocede con el golpe final");
        }

        [Test]
        public void UnNocautEnElAireTerminaLaRondaYElRivalCaeNoqueado()
        {
            var estado = Simular(ConVidas(Inicial(0, 1), VidaMaxima, 100), BotonesEntrada.Debil, BotonesEntrada.Arriba, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Arriba, FrameDeImpacto - 1);

            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 60);

            Assert.AreEqual(AccionLuchador.Noqueado, estado.Luchador2.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Posicion.Y);
        }

        [Test]
        public void ElRelojBajaUnFramePorPaso()
        {
            var estado = Simular(Inicial(-2, 2), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 90);

            Assert.AreEqual(FramesPorRonda - 90, estado.FramesRestantes);
        }

        [Test]
        public void AlAgotarseElTiempoGanaQuienTieneMasVida()
        {
            var configuracion = ConfiguracionConReloj(10);
            var estado = ConVidas(EstadoPartida.Crear(VectorFijo.DesdeEnteros(-2, 0), VectorFijo.DesdeEnteros(2, 0), configuracion), 600, 500);

            for (var i = 0; i < 9; i++)
            {
                estado = SimuladorCombate.Avanzar(estado, EntradasFrame.Vacias, configuracion);
            }

            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);

            estado = SimuladorCombate.Avanzar(estado, EntradasFrame.Vacias, configuracion);

            Assert.AreEqual(0, estado.FramesRestantes);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
        }

        [Test]
        public void AlAgotarseElTiempoConLaMismaVidaEsEmpate()
        {
            var configuracion = ConfiguracionConReloj(10);
            var estado = EstadoPartida.Crear(VectorFijo.DesdeEnteros(-2, 0), VectorFijo.DesdeEnteros(2, 0), configuracion);

            for (var i = 0; i < 10; i++)
            {
                estado = SimuladorCombate.Avanzar(estado, EntradasFrame.Vacias, configuracion);
            }

            Assert.AreEqual(ResultadoRonda.Empate, estado.Resultado);
        }
    }
}
