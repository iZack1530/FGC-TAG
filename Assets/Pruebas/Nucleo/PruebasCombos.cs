using FgcTag.Nucleo.Combate;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba y los luchadores pegados, el golpe débil pulsado en el frame 1
    /// conecta en el frame 5, el hitstop ocupa los frames 6 a 13 y el atacante vuelve a actuar
    /// en el frame 14.
    /// </summary>
    public class PruebasCombos
    {
        private const int PrimerFrameTrasElHitstop = 14;

        /// <summary>
        /// El jugador 1 pulsa Débil en el frame 1 y <paramref name="siguiente"/> en el frame 6
        /// (durante el hitstop si el golpe conectó); se simula hasta el frame indicado.
        /// </summary>
        private static EstadoPartida DebilYLuego(EstadoPartida inicial, BotonesEntrada siguiente, BotonesEntrada jugador2, int hastaFrame)
        {
            var estado = Simular(inicial, BotonesEntrada.Debil, jugador2, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, jugador2, 4);
            estado = Simular(estado, siguiente, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, hastaFrame - 6);
        }

        /// <summary>El jugador 1 falla un golpe débil (controla de nuevo en el frame 17) y pulsa otro en el frame indicado.</summary>
        private static EstadoPartida FallarYPulsarEnFrame(int framePulsacion)
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, framePulsacion - 2);
            estado = SimularJugador1(estado, BotonesEntrada.Debil, 1);

            return SimularJugador1(estado, BotonesEntrada.Ninguno, 17 - framePulsacion);
        }

        [Test]
        public void LaFrameDataDeMedioYFuerteCalculaSuVentaja()
        {
            Assert.AreEqual(23, GolpeMedio.DuracionTotal);
            Assert.AreEqual(1, GolpeMedio.VentajaAlGolpear);
            Assert.AreEqual(-3, GolpeMedio.VentajaAlBloquear);
            Assert.AreEqual(33, GolpeFuerte.DuracionTotal);
            Assert.AreEqual(-2, GolpeFuerte.VentajaAlGolpear);
            Assert.AreEqual(-7, GolpeFuerte.VentajaAlBloquear);
        }

        [Test]
        public void CadaBotonLanzaSuAtaque()
        {
            Assert.AreEqual(TipoAtaque.Debil, SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil, 1).Luchador1.Ataque);
            Assert.AreEqual(TipoAtaque.Medio, SimularJugador1(Inicial(-5, 5), BotonesEntrada.Medio, 1).Luchador1.Ataque);
            Assert.AreEqual(TipoAtaque.Fuerte, SimularJugador1(Inicial(-5, 5), BotonesEntrada.Fuerte, 1).Luchador1.Ataque);
        }

        [Test]
        public void ConVariosBotonesALaVezSaleElMasFuerte()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil | BotonesEntrada.Medio | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(TipoAtaque.Fuerte, estado.Luchador1.Ataque);
        }

        [Test]
        public void CadaAtaqueDuraLoQueDiceSuFrameData()
        {
            var estado = SimularJugador1(SimularJugador1(Inicial(-5, 5), BotonesEntrada.Fuerte, 1), BotonesEntrada.Ninguno, 32);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
        }

        [Test]
        public void UnGolpeQueConectaSeCancelaEnUnoMasFuerte()
        {
            var estado = DebilYLuego(Inicial(0, 1), BotonesEntrada.Medio, BotonesEntrada.Ninguno, PrimerFrameTrasElHitstop - 1);

            Assert.AreEqual(TipoAtaque.Debil, estado.Luchador1.Ataque);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
            Assert.IsFalse(estado.Luchador1.GolpeConectado);
            Assert.IsFalse(estado.Luchador1.TieneAtaqueEnBufer);
        }

        [Test]
        public void UnGolpeBloqueadoTambienSeCancela()
        {
            var estado = DebilYLuego(Inicial(0, 1), BotonesEntrada.Fuerte, BotonesEntrada.Derecha, PrimerFrameTrasElHitstop);

            Assert.AreEqual(TipoAtaque.Fuerte, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void UnGolpeQueFallaNoSeCancela()
        {
            var estado = DebilYLuego(Inicial(-5, 5), BotonesEntrada.Medio, BotonesEntrada.Ninguno, 16);

            Assert.AreEqual(TipoAtaque.Debil, estado.Luchador1.Ataque);
            Assert.AreEqual(15, estado.Luchador1.FramesEnAccion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void NoSeCancelaEnElMismoAtaqueNiEnUnoMasDebil()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 6);

            Assert.IsTrue(estado.Luchador1.GolpeConectado);

            estado = Simular(estado, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 9);

            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
            Assert.AreEqual(7, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void DebilMedioFuerteEsUnComboDeTresGolpes()
        {
            var siguientes = new[] { BotonesEntrada.Medio, BotonesEntrada.Fuerte };
            var indice = 0;
            var enHitstop = false;
            var comboMaximo = 0;
            var rivalLibreDuranteElCombo = false;
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            for (var i = 0; i < 150; i++)
            {
                var acabaDeConectar = estado.Luchador1.EnHitstop && !enHitstop;
                var botones = acabaDeConectar && indice < siguientes.Length ? siguientes[indice++] : BotonesEntrada.Ninguno;
                enHitstop = estado.Luchador1.EnHitstop;

                estado = Simular(estado, botones, BotonesEntrada.Ninguno, 1);

                var golpes = estado.Luchador2.GolpesEnCombo;
                rivalLibreDuranteElCombo |= golpes > 0 && golpes < 3 && estado.Luchador2.Accion != AccionLuchador.Hitstun;
                comboMaximo = golpes > comboMaximo ? golpes : comboMaximo;
            }

            Assert.AreEqual(3, comboMaximo);
            Assert.IsFalse(rivalLibreDuranteElCombo, "El rival no debe recuperarse entre golpes");
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
        }

        [Test]
        public void UnGolpeBloqueadoNoCuentaComoCombo()
        {
            var estado = DebilYLuego(Inicial(0, 1), BotonesEntrada.Ninguno, BotonesEntrada.Derecha, 6);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
        }

        [Test]
        public void ElBuferLanzaElAtaquePulsadoPocoAntesDeRecuperarElControl()
        {
            var estado = FallarYPulsarEnFrame(17 - FramesBufer + 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void ElBuferDescartaUnaPulsacionDemasiadoTemprana()
        {
            var estado = FallarYPulsarEnFrame(17 - FramesBufer);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.IsFalse(estado.Luchador1.TieneAtaqueEnBufer);
        }

        [Test]
        public void ElHitstopNoConsumeElBufer()
        {
            var estado = DebilYLuego(Inicial(0, 1), BotonesEntrada.Medio, BotonesEntrada.Ninguno, PrimerFrameTrasElHitstop - 1);

            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.AtaqueEnBufer);
            Assert.AreEqual(FramesBufer, estado.Luchador1.FramesBufer);
        }

        [Test]
        public void ElBuferPermiteAtacarAlSalirDelAturdimiento()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4 + 8 + 12);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Debil, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 2);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.FramesEnAccion);
        }
    }
}
