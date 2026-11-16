using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba y los luchadores pegados, el lanzador pulsado en el frame 1
    /// conecta en el frame 9, el hitstop ocupa los frames 10 a 21 y ambos vuelven a moverse en el 22.
    /// </summary>
    public class PruebasCombateAereo
    {
        private const int FrameDeImpactoLanzador = 9;
        private const int PrimerFrameTrasElHitstop = 22;

        private static EstadoPartida LanzadorHastaElImpacto(EstadoPartida inicial, BotonesEntrada jugador2)
        {
            var estado = Simular(inicial, BotonesEntrada.Especial, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, FrameDeImpactoLanzador - 1);
        }

        private static EstadoPartida SimularHastaAccionDelJugador2(EstadoPartida estado, AccionLuchador accion)
        {
            for (var i = 0; i < 300; i++)
            {
                if (estado.Luchador2.Accion == accion)
                {
                    return estado;
                }

                estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
            }

            throw new InvalidOperationException($"El jugador 2 no llegó a {accion} en 300 frames.");
        }

        [Test]
        public void ElBotonEspecialEnTierraEsElLanzador()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Especial, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Especial, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElLanzadorLevantaAlRivalDelSuelo()
        {
            var estado = LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(Lanzador.VelocidadLanzamiento, estado.Luchador2.Velocidad.Y);
            Assert.AreEqual(1, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(VidaMaxima - 250, estado.Luchador2.Vida);
            Assert.AreEqual(12, estado.Luchador2.FramesHitstop);
        }

        [Test]
        public void ElLanzadorBloqueadoNoLevantaYHaceDanioChip()
        {
            var estado = LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Derecha);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Posicion.Y);
            Assert.AreEqual(VidaMaxima - 25, estado.Luchador2.Vida);
            Assert.IsTrue(estado.Luchador1.GolpeBloqueado);
        }

        [Test]
        public void ElLanzadorQueGolpeaSeCancelaConUnSalto()
        {
            var estado = LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno);
            estado = Simular(estado, BotonesEntrada.Arriba, BotonesEntrada.Ninguno, PrimerFrameTrasElHitstop - FrameDeImpactoLanzador);

            Assert.AreEqual(AccionLuchador.PreSalto, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElLanzadorBloqueadoNoSeCancelaConUnSalto()
        {
            var estado = LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Derecha);
            estado = Simular(estado, BotonesEntrada.Arriba, BotonesEntrada.Ninguno, PrimerFrameTrasElHitstop - FrameDeImpactoLanzador);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
        }

        [Test]
        public void ElLanzadorQueFallaNoSeCancelaConUnSalto()
        {
            var estado = LanzadorHastaElImpacto(Inicial(-5, 5), BotonesEntrada.Ninguno);
            estado = Simular(estado, BotonesEntrada.Arriba, BotonesEntrada.Ninguno, 10);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Especial, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElFuerteSeCancelaEnElLanzador()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Fuerte, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 9);

            Assert.IsTrue(estado.Luchador1.GolpeConectado);

            estado = Simular(estado, BotonesEntrada.Especial, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 12);

            Assert.AreEqual(TipoAtaque.Especial, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void ElRivalGolpeadoEnElAireCaeConLaGravedadDelJuggle()
        {
            var impacto = LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno);
            var estado = Simular(impacto, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, PrimerFrameTrasElHitstop - FrameDeImpactoLanzador);

            Assert.AreEqual(Fijo.Medio, estado.Luchador2.Posicion.Y);
            Assert.AreEqual(Fijo.Medio - Juggle.GravedadCon(1), estado.Luchador2.Velocidad.Y);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 32) + Fijo.DesdeFraccion(1, 128), Juggle.GravedadCon(1));
        }

        [Test]
        public void AlCaerQuedaDerribadoYLuegoSeLevanta()
        {
            var estado = SimularHastaAccionDelJugador2(LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno), AccionLuchador.Derribado);

            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Posicion.Y);
            Assert.AreEqual(1, estado.Luchador2.GolpesEnCombo);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, Juggle.FramesDerribado - 1);

            Assert.AreEqual(AccionLuchador.Derribado, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
        }

        [Test]
        public void UnRivalDerribadoEsInvulnerable()
        {
            var estado = SimularHastaAccionDelJugador2(LanzadorHastaElImpacto(Inicial(0, 1), BotonesEntrada.Ninguno), AccionLuchador.Derribado);
            var vida = estado.Luchador2.Vida;

            estado = Simular(estado, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            for (var i = 0; i < 16; i++)
            {
                estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

                Assert.IsFalse(estado.Luchador1.GolpeConectado);
                Assert.IsTrue(estado.Luchador2.Vida >= vida, "Derribado no pierde vida; solo puede regenerar la recuperable");
            }
        }

        [Test]
        public void ElLanzadorNoRelanzaAUnRivalQueYaEstaEnElAire()
        {
            var estado = LanzadorHastaElImpacto(ConJugador2EnElAire(Inicial(0, 1), 1), BotonesEntrada.Ninguno);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(Juggle.ImpulsoAlGolpear, estado.Luchador2.Velocidad.Y);
        }

        [Test]
        public void EnElAireCadaBotonLanzaSuAtaqueAereo()
        {
            var enElAire = ConJugador1EnElAire(Inicial(-5, 5), 5);
            var estado = SimularJugador1(enElAire, BotonesEntrada.Medio, 1);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void ElAtaqueAereoDuraSusFramesYVuelveAlSalto()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 5), BotonesEntrada.Debil, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 13);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
        }

        [Test]
        public void AtacarEnElAireNoCambiaLaTrayectoria()
        {
            var enElAire = ConJugador1EnElAire(Inicial(-5, 5), 5);
            var atacando = SimularJugador1(SimularJugador1(enElAire, BotonesEntrada.Fuerte, 1), BotonesEntrada.Ninguno, 9);
            var sinAtacar = SimularJugador1(enElAire, BotonesEntrada.Ninguno, 10);

            Assert.AreEqual(sinAtacar.Luchador1.Posicion, atacando.Luchador1.Posicion);
            Assert.AreEqual(sinAtacar.Luchador1.Velocidad, atacando.Luchador1.Velocidad);
        }

        [Test]
        public void AterrizarInterrumpeElAtaqueAereo()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 1), BotonesEntrada.Fuerte, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 10);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
            Assert.IsFalse(Parametros.CajaGolpeActiva(estado.Luchador1).HasValue);
        }

        [Test]
        public void ElBotonEspecialNoHaceNadaEnElAire()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 5), BotonesEntrada.Especial, 3);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
        }

        [Test]
        public void UnAtaqueAereoContraUnRivalEnTierraLoDejaEnHitstun()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 4);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 100, estado.Luchador2.Vida);
            Assert.IsTrue(estado.Luchador1.GolpeConectado);
        }

        [Test]
        public void LosAtaquesAereosTambienSeCancelanEnUnoMasFuerte()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 4);
            estado = SimularJugador1(estado, BotonesEntrada.Medio, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 8);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        /// <summary>
        /// El combo completo del juego: Débil, Medio, Fuerte y Lanzador en tierra, salto hacia
        /// adelante cancelando el lanzador y Débil aéreo en cuanto despega.
        /// </summary>
        [Test]
        public void ElComboDeTierraSigueEnElAireTrasElLanzador()
        {
            var enTierra = new[] { BotonesEntrada.Medio, BotonesEntrada.Fuerte, BotonesEntrada.Especial };
            var indice = 0;
            var enHitstop = false;
            var aereoPulsado = false;
            var comboMaximo = 0;
            var vidaMinima = VidaMaxima;
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            for (var i = 0; i < 200; i++)
            {
                var jugador1 = estado.Luchador1;
                var acabaDeConectar = jugador1.EnHitstop && !enHitstop;
                enHitstop = jugador1.EnHitstop;
                var botones = BotonesEntrada.Ninguno;

                if (indice < enTierra.Length)
                {
                    botones = acabaDeConectar ? enTierra[indice++] : BotonesEntrada.Ninguno;
                }
                else if (jugador1.Accion == AccionLuchador.EnAire && !aereoPulsado)
                {
                    botones = BotonesEntrada.Debil;
                    aereoPulsado = true;
                }
                else if (!aereoPulsado)
                {
                    botones = BotonesEntrada.Arriba | BotonesEntrada.Derecha;
                }

                estado = Simular(estado, botones, BotonesEntrada.Ninguno, 1);
                comboMaximo = Math.Max(comboMaximo, estado.Luchador2.GolpesEnCombo);
                vidaMinima = Math.Min(vidaMinima, estado.Luchador2.Vida);
            }

            Assert.AreEqual(5, comboMaximo);
            Assert.AreEqual(VidaMaxima - (100 + 180 + 240 + 175 + 60), vidaMinima);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
            Assert.AreEqual(0, estado.Luchador2.GolpesEnCombo);
        }

        [Test]
        public void UnSegundoGolpeEnElAireSigueElCombo()
        {
            var estado = Simular(ConJugador2EnElAire(Inicial(0, 1), 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(1, estado.Luchador2.GolpesEnCombo);

            estado = Simular(estado, BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 14);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(2, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(VidaMaxima - 100 - 180, estado.Luchador2.Vida);
            Assert.AreEqual(Juggle.ImpulsoAlGolpear, estado.Luchador2.Velocidad.Y);
        }
    }
}
