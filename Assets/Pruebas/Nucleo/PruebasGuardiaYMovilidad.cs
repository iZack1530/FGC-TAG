using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba y los luchadores pegados, un golpe débil (de pie, agachado o
    /// aéreo) pulsado en el frame 1 conecta en el frame 5; el hitstop ocupa los frames 6 a 13.
    /// </summary>
    public class PruebasGuardiaYMovilidad
    {
        private const BotonesEntrada DebilAgachado = BotonesEntrada.Abajo | BotonesEntrada.Debil;
        private const BotonesEntrada AtrasJugador2 = BotonesEntrada.Derecha;
        private const BotonesEntrada AbajoAtrasJugador2 = BotonesEntrada.Abajo | BotonesEntrada.Derecha;

        /// <summary>El jugador 1 pulsa sus botones en el frame 1 y los mantiene (sin el ataque) hasta el frame 5.</summary>
        private static EstadoPartida GolpearEnElFrame5(EstadoPartida inicial, BotonesEntrada jugador1, BotonesEntrada jugador2)
        {
            var estado = Simular(inicial, jugador1, jugador2, 1);

            return Simular(estado, jugador1 & BotonesEntrada.Abajo, jugador2, 4);
        }

        /// <summary>
        /// El jugador 2 salta en el frame 1 y recibe en el frame 6, recién despegado, un débil
        /// agachado: el único golpe del escenario con aturdimiento aéreo corto (4 frames).
        /// Tras el hitstop (frames 7 a 14) y el aturdimiento (15 a 18), se recupera en el frame 19.
        /// </summary>
        private static EstadoPartida GolpeCortoAlRivalQueSalta()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Abajo, BotonesEntrada.Arriba, 1);
            estado = Simular(estado, DebilAgachado, BotonesEntrada.Arriba, 1);

            return Simular(estado, BotonesEntrada.Abajo, BotonesEntrada.Arriba, 4);
        }

        private static EstadoPartida SaltarTrasAgacharse(int framesNeutrales)
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Abajo, 2);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, framesNeutrales);

            return SimularJugador1(estado, BotonesEntrada.Arriba, 5);
        }

        [Test]
        public void AbajoMasUnGolpeLanzaLaVersionAgachada()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DebilAgachado, 1);

            Assert.AreEqual(AccionLuchador.AtacandoAgachado, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Debil, estado.Luchador1.Ataque);
            Assert.AreEqual(AlturaGolpe.Bajo, Parametros.MovimientoEnCurso(estado.Luchador1).Altura);
            Assert.AreEqual(Fijo.Medio, Parametros.CajaDanio(estado.Luchador1).MedioAlto);
        }

        [Test]
        public void ElLanzadorEsElMismoDePieYAgachado()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Abajo | BotonesEntrada.Especial, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Especial, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElAtaqueAgachadoDuraSusFramesYDevuelveElControl()
        {
            var estado = SimularJugador1(SimularJugador1(Inicial(-5, 5), DebilAgachado, 1), BotonesEntrada.Abajo, 15);

            Assert.AreEqual(AccionLuchador.AtacandoAgachado, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Abajo, 1);

            Assert.AreEqual(AccionLuchador.Agachado, estado.Luchador1.Accion);
        }

        [Test]
        public void UnDebilAgachadoSeCancelaEnUnMedioDePie()
        {
            var estado = GolpearEnElFrame5(Inicial(0, 1), DebilAgachado, BotonesEntrada.Ninguno);
            estado = Simular(estado, BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 8);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void UnDebilDePieSeCancelaEnUnMedioAgachado()
        {
            var estado = GolpearEnElFrame5(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno);
            estado = Simular(estado, BotonesEntrada.Abajo | BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Abajo, BotonesEntrada.Ninguno, 8);

            Assert.AreEqual(AccionLuchador.AtacandoAgachado, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
        }

        [Test]
        public void LaGuardiaDebeCorresponderALaAlturaDelGolpe()
        {
            Assert.IsTrue(Guardia.Detiene(AlturaGolpe.Medio, false));
            Assert.IsTrue(Guardia.Detiene(AlturaGolpe.Medio, true));
            Assert.IsFalse(Guardia.Detiene(AlturaGolpe.Bajo, false));
            Assert.IsTrue(Guardia.Detiene(AlturaGolpe.Bajo, true));
            Assert.IsTrue(Guardia.Detiene(AlturaGolpe.Alto, false));
            Assert.IsFalse(Guardia.Detiene(AlturaGolpe.Alto, true));
        }

        [Test]
        public void UnGolpeBajoEntraContraLaGuardiaDePie()
        {
            var estado = GolpearEnElFrame5(Inicial(0, 1), DebilAgachado, AtrasJugador2);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 100, estado.Luchador2.Vida);
        }

        [Test]
        public void UnGolpeBajoSeBloqueaAgachado()
        {
            var estado = GolpearEnElFrame5(Inicial(0, 1), DebilAgachado, AbajoAtrasJugador2);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
        }

        [Test]
        public void UnAtaqueAereoEntraContraLaGuardiaAgachada()
        {
            var estado = GolpearEnElFrame5(ConJugador1EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, AbajoAtrasJugador2);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
        }

        [Test]
        public void UnAtaqueAereoSeBloqueaDePie()
        {
            var estado = GolpearEnElFrame5(ConJugador1EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, AtrasJugador2);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
        }

        [Test]
        public void EnElAireSeBloqueaManteniendoAtras()
        {
            var enElAire = ConJugador2EnElAire(Inicial(0, 1), 1);
            var antes = Simular(Simular(enElAire, BotonesEntrada.Debil, AtrasJugador2, 1), BotonesEntrada.Ninguno, AtrasJugador2, 3);
            var estado = Simular(antes, BotonesEntrada.Ninguno, AtrasJugador2, 1);

            Assert.AreEqual(AccionLuchador.BloqueandoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(10, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(8, estado.Luchador2.FramesHitstop);
            Assert.AreEqual(VidaMaxima - 10, estado.Luchador2.Vida);
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
            Assert.IsTrue(estado.Luchador1.GolpeBloqueado);
        }

        [Test]
        public void ElBloqueoAereoTerminaEnUnAterrizajeNormal()
        {
            var estado = GolpearEnElFrame5(ConJugador2EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, AtrasJugador2);
            var huboDerribo = false;

            for (var i = 0; i < 60; i++)
            {
                estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
                huboDerribo |= estado.Luchador2.Accion == AccionLuchador.Derribado;
            }

            Assert.IsFalse(huboDerribo);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.FramesAturdimiento);
        }

        [Test]
        public void SinMantenerAtrasElGolpeEntraEnElAire()
        {
            var estado = GolpearEnElFrame5(ConJugador2EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
        }

        [Test]
        public void QuienAtacaEnElAireNoPuedeBloquear()
        {
            var enElAire = ConJugador2EnElAire(Inicial(0, 1), 1);
            var estado = Simular(enElAire, BotonesEntrada.Debil, BotonesEntrada.Fuerte | AtrasJugador2, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, AtrasJugador2, 4);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
        }

        [Test]
        public void ElAturdimientoAereoDecaeConElComboHastaUnMinimo()
        {
            Assert.AreEqual(60, Juggle.AturdimientoCon(60, 1));
            Assert.AreEqual(56, Juggle.AturdimientoCon(60, 3));
            Assert.AreEqual(4, Juggle.AturdimientoCon(5, 10));
        }

        [Test]
        public void AlAgotarseElAturdimientoElRivalSeRecuperaEnElAire()
        {
            var golpeado = GolpeCortoAlRivalQueSalta();

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, golpeado.Luchador2.Accion);
            Assert.AreEqual(4, golpeado.Luchador2.FramesAturdimiento);

            var estado = Simular(golpeado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 8 + 4);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Velocidad.X);
            Assert.IsTrue(estado.Luchador2.Velocidad.Y > Fijo.Cero, "La recuperación da un pequeño salto");
        }

        [Test]
        public void LaRecuperacionSeDirigeConLaDireccionMantenida()
        {
            var estado = Simular(GolpeCortoAlRivalQueSalta(), BotonesEntrada.Ninguno, BotonesEntrada.Derecha, 8 + 4 + 1);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador2.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 8), estado.Luchador2.Velocidad.X);
        }

        [Test]
        public void QuienSeRecuperaEnElAireAterrizaSinQuedarDerribado()
        {
            var estado = GolpeCortoAlRivalQueSalta();
            var huboDerribo = false;

            for (var i = 0; i < 90; i++)
            {
                estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
                huboDerribo |= estado.Luchador2.Accion == AccionLuchador.Derribado;
            }

            Assert.IsFalse(huboDerribo);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
        }

        [Test]
        public void SaltarDesdeAgachadoEsUnSupersalto()
        {
            var estado = SaltarTrasAgacharse(0);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.IsTrue(estado.Luchador1.Supersalto);
            Assert.AreEqual(Fijo.DesdeFraccion(3, 5), estado.Luchador1.Velocidad.Y);
        }

        [Test]
        public void UnSaltoSinAgacharseEsNormal()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 5);

            Assert.IsFalse(estado.Luchador1.Supersalto);
            Assert.AreEqual(Fijo.DesdeFraccion(2, 5), estado.Luchador1.Velocidad.Y);
        }

        [Test]
        public void ElSupersaltoAdmiteUnosFramesNeutralesTrasAgacharse()
        {
            Assert.IsTrue(SaltarTrasAgacharse(6).Luchador1.Supersalto);
            Assert.IsFalse(SaltarTrasAgacharse(7).Luchador1.Supersalto);
        }

        [Test]
        public void ElSupersaltoSubeMasQueElSaltoYSeOlvidaAlAterrizar()
        {
            var estado = SaltarTrasAgacharse(0);
            var alturaMaxima = Fijo.Cero;

            for (var i = 0; i < 120; i++)
            {
                estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);
                alturaMaxima = Fijo.Max(alturaMaxima, estado.Luchador1.Posicion.Y);
            }

            Assert.IsTrue(alturaMaxima > Fijo.DesdeEntero(8), $"Altura máxima {alturaMaxima}");
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.IsFalse(estado.Luchador1.Supersalto);
        }

        [Test]
        public void ElSaltoQueCancelaElLanzadorEsNormal()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Especial, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Arriba, BotonesEntrada.Ninguno, 8 + 12 + 5);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.IsFalse(estado.Luchador1.Supersalto);
        }
    }
}
