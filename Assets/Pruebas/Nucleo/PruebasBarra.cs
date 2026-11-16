using FgcTag.Nucleo.Combate;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: barras de 100 unidades y 7 como máximo. Del daño base del
    /// movimiento: 5 % al ejecutarlo, 20 % más si conecta o 10 % más si lo bloquean; quien lo
    /// recibe gana 15 % y quien lo bloquea, 5 %. El golpe débil hace 100 de daño; el medio, 200.
    /// </summary>
    public class PruebasBarra
    {
        private const int FrameDeImpacto = 5;
        private const BotonesEntrada AtrasJugador2 = BotonesEntrada.Derecha;

        private static EstadoPartida GolpeDebilHasta(EstadoPartida inicial, BotonesEntrada jugador2, int frame)
        {
            var estado = Simular(inicial, BotonesEntrada.Debil, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, frame - 1);
        }

        [Test]
        public void LasReglasCalculanCadaGananciaYRespetanElTope()
        {
            Assert.AreEqual(700, Barra.Maximo);
            Assert.AreEqual(5, Barra.AlEjecutar(0, 100));
            Assert.AreEqual(20, Barra.AlConectar(0, 100));
            Assert.AreEqual(10, Barra.AlSerBloqueado(0, 100));
            Assert.AreEqual(15, Barra.AlRecibir(0, 100));
            Assert.AreEqual(5, Barra.AlBloquear(0, 100));
            Assert.AreEqual(700, Barra.AlConectar(690, 300));
            Assert.AreEqual(2, Barra.BarrasCompletas(250));
        }

        [Test]
        public void ElCombateEmpiezaSinBarra()
        {
            var estado = Inicial(0, 1);

            Assert.AreEqual(0, estado.Luchador1.Barra);
            Assert.AreEqual(0, estado.Luchador2.Barra);
        }

        [Test]
        public void UnAtaqueAlAireDaUnPocoDeBarraUnaSolaVez()
        {
            var estado = GolpeDebilHasta(Inicial(0, 5), BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(5, estado.Luchador1.Barra);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 30);

            Assert.AreEqual(5, estado.Luchador1.Barra);
            Assert.AreEqual(0, estado.Luchador2.Barra);
        }

        [Test]
        public void UnGolpeQueConectaDaBarraAlAtacanteYAlGolpeado()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), BotonesEntrada.Ninguno, FrameDeImpacto);

            Assert.AreEqual(25, estado.Luchador1.Barra);
            Assert.AreEqual(15, estado.Luchador2.Barra);
        }

        [Test]
        public void UnGolpeBloqueadoDaMenosBarraALosDos()
        {
            var estado = GolpeDebilHasta(Inicial(0, 1), AtrasJugador2, FrameDeImpacto);

            Assert.AreEqual(15, estado.Luchador1.Barra);
            Assert.AreEqual(5, estado.Luchador2.Barra);
        }

        [Test]
        public void UnAgarreDaBarraAlIntentarloYAlLanzar()
        {
            var agarre = BotonesEntrada.Derecha | BotonesEntrada.Fuerte;
            var estado = Simular(Inicial(0, 1), agarre, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(12, estado.Luchador1.Barra);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 13);

            Assert.AreEqual(62, estado.Luchador1.Barra);
            Assert.AreEqual(37, estado.Luchador2.Barra);
        }

        [Test]
        public void LaBarraPasaAlCompaneroQueEntraEnUnCambio()
        {
            var estado = SimularEquipos(InicialPorEquipos(-5, 5), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 20);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia1);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(5, estado.Duelo.Luchador1.Barra);
        }

        [Test]
        public void ElRelevoHeredaLaBarraDelLuchadorNoqueado()
        {
            var estado = SimularEquipos(ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1 + FramesPausaNocaut);

            Assert.AreEqual(1, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(15, estado.Duelo.Luchador2.Barra);
            Assert.AreEqual(25, estado.Duelo.Luchador1.Barra);
        }

        [Test]
        public void ElGolpeDelAsistenteDaBarraAlEquipo()
        {
            var estado = LlamarAsistenteJugador1(InicialPorEquipos(0, 1), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesLlegadaAsistencia + 6);

            Assert.AreEqual(40, estado.Duelo.Luchador1.Barra);
            Assert.AreEqual(30, estado.Duelo.Luchador2.Barra);
        }
    }
}
