using System;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba, el golpe débil pulsado en el frame 1 conecta en el frame 5
    /// contra un rival pegado (centros a 1 unidad).
    /// </summary>
    public class PruebasAtaque
    {
        private const int FrameDeImpacto = 5;

        /// <summary>El jugador 1 pulsa Débil en el primer frame; el jugador 2 mantiene sus botones hasta el impacto.</summary>
        private static EstadoPartida SimularHastaElImpacto(BotonesEntrada jugador2)
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, FrameDeImpacto - 1);
        }

        /// <summary>Frame en el que cada luchador recupera el control, detectado al agacharse con abajo mantenido.</summary>
        private static (int jugador1, int jugador2) FramesDeRecuperacion(EstadoPartida estado)
        {
            var frame1 = 0;
            var frame2 = 0;

            for (var i = 0; i < 120 && (frame1 == 0 || frame2 == 0); i++)
            {
                estado = Simular(estado, BotonesEntrada.Abajo, BotonesEntrada.Abajo, 1);

                if (frame1 == 0 && estado.Luchador1.Accion == AccionLuchador.Agachado)
                {
                    frame1 = estado.Frame;
                }

                if (frame2 == 0 && estado.Luchador2.Accion == AccionLuchador.Agachado)
                {
                    frame2 = estado.Frame;
                }
            }

            if (frame1 == 0 || frame2 == 0)
            {
                throw new InvalidOperationException("Algún luchador no recuperó el control en 120 frames.");
            }

            return (frame1, frame2);
        }

        [Test]
        public void LaFrameDataCalculaDuracionYVentaja()
        {
            Assert.AreEqual(16, GolpeDebil.DuracionTotal);
            Assert.AreEqual(3, GolpeDebil.VentajaAlGolpear);
            Assert.AreEqual(-1, GolpeDebil.VentajaAlBloquear);
        }

        [Test]
        public void ElAtaqueDuraSusFramesYMantenerElBotonNoLoRepite()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);

            estado = SimularJugador1(estado, BotonesEntrada.Debil, 15);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(15, estado.Luchador1.FramesEnAccion);

            estado = SimularJugador1(estado, BotonesEntrada.Debil, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void SoltarYVolverAPulsarLanzaOtroAtaque()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 16);
            estado = SimularJugador1(estado, BotonesEntrada.Debil, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(0, estado.Luchador1.FramesEnAccion);
        }

        [Test]
        public void LaCajaDeGolpeSoloExisteEnLosFramesActivos()
        {
            var estado = Inicial(-5, 5);

            for (var frame = 1; frame <= 16; frame++)
            {
                estado = SimularJugador1(estado, frame == 1 ? BotonesEntrada.Debil : BotonesEntrada.Ninguno, 1);
                var esperada = frame >= 5 && frame <= 7;

                Assert.AreEqual(esperada, Parametros.CajaGolpeActiva(estado.Luchador1).HasValue, $"Frame {frame}");
            }
        }

        [Test]
        public void LaCajaDeGolpeSeReflejaAlMirarALaIzquierda()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Ninguno, BotonesEntrada.Debil, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            var caja = Parametros.CajaGolpeActiva(estado.Luchador2);

            Assert.IsTrue(caja.HasValue);
            Assert.AreEqual(VectorFijo.DesdeEnteros(4, 1), caja.Value.Centro);
        }

        [Test]
        public void ElGolpeConectaEnSuPrimerFrameActivo()
        {
            var antes = Simular(Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 2);

            Assert.AreEqual(AccionLuchador.Quieto, antes.Luchador2.Accion);

            var estado = SimularHastaElImpacto(BotonesEntrada.Ninguno);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
            Assert.AreEqual(14, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(8, estado.Luchador2.FramesHitstop);
            Assert.AreEqual(8, estado.Luchador1.FramesHitstop);
            Assert.IsTrue(estado.Luchador1.GolpeConectado);
        }

        [Test]
        public void ElHitstopCongelaAAmbosLuchadores()
        {
            var impacto = SimularHastaElImpacto(BotonesEntrada.Ninguno);
            var estado = Simular(impacto, BotonesEntrada.Derecha, BotonesEntrada.Izquierda, 8);

            Assert.AreEqual(impacto.Frame + 8, estado.Frame);
            Assert.AreEqual(impacto.Luchador1.Posicion, estado.Luchador1.Posicion);
            Assert.AreEqual(impacto.Luchador2.Posicion, estado.Luchador2.Posicion);
            Assert.AreEqual(impacto.Luchador1.FramesEnAccion, estado.Luchador1.FramesEnAccion);
            Assert.AreEqual(impacto.Luchador2.FramesAturdimiento, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(0, estado.Luchador1.FramesHitstop);
            Assert.AreEqual(0, estado.Luchador2.FramesHitstop);
        }

        [Test]
        public void UnAtaqueSoloGolpeaUnaVez()
        {
            var estado = Simular(SimularHastaElImpacto(BotonesEntrada.Ninguno), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 9);

            Assert.IsTrue(Parametros.CajaGolpeActiva(estado.Luchador1).HasValue, "El ataque sigue en frames activos");
            Assert.AreEqual(13, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(0, estado.Luchador2.FramesHitstop);
        }

        [Test]
        public void LaVentajaAlGolpearCoincideConLaFrameData()
        {
            var (jugador1, jugador2) = FramesDeRecuperacion(SimularHastaElImpacto(BotonesEntrada.Ninguno));

            Assert.AreEqual(GolpeDebil.VentajaAlGolpear, jugador2 - jugador1);
        }

        [Test]
        public void LaVentajaAlBloquearCoincideConLaFrameData()
        {
            var (jugador1, jugador2) = FramesDeRecuperacion(SimularHastaElImpacto(BotonesEntrada.Derecha));

            Assert.AreEqual(GolpeDebil.VentajaAlBloquear, jugador2 - jugador1);
        }

        [Test]
        public void MantenerAtrasBloqueaElGolpe()
        {
            var estado = SimularHastaElImpacto(BotonesEntrada.Derecha);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);
            Assert.AreEqual(10, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(8, estado.Luchador2.FramesHitstop);
        }

        [Test]
        public void MantenerAbajoAtrasTambienBloquea()
        {
            var estado = SimularHastaElImpacto(BotonesEntrada.Abajo | BotonesEntrada.Derecha);

            Assert.AreEqual(AccionLuchador.BlockstunAgachado, estado.Luchador2.Accion);
        }

        [Test]
        public void CaminarHaciaElAtacanteNoBloquea()
        {
            var estado = SimularHastaElImpacto(BotonesEntrada.Izquierda);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
        }

        [Test]
        public void ElRetrocesoAlejaAlGolpeadoYLaFriccionLoDetiene()
        {
            var impacto = SimularHastaElImpacto(BotonesEntrada.Ninguno);
            var estado = Simular(impacto, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 8 + 14);

            Assert.AreEqual(impacto.Luchador2.Posicion.X + Fijo.DesdeFraccion(9, 16), estado.Luchador2.Posicion.X);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Velocidad.X);
            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
        }

        [Test]
        public void FueraDeAlcanceElGolpeNoConecta()
        {
            var estado = SimularJugador1(Inicial(0, 5), BotonesEntrada.Debil, 1);

            for (var i = 0; i < 16; i++)
            {
                estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

                Assert.IsFalse(estado.Luchador1.GolpeConectado);
                Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
                Assert.AreEqual(0, estado.Luchador1.FramesHitstop);
            }
        }

        [Test]
        public void DosGolpesSimultaneosSeIntercambian()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Debil, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
            Assert.IsTrue(estado.Luchador1.Velocidad.X < Fijo.Cero);
            Assert.IsTrue(estado.Luchador2.Velocidad.X > Fijo.Cero);
        }

        [Test]
        public void UnGolpeAUnRivalQueSaltaLoDejaGolpeadoEnElAire()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Arriba, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Arriba, FrameDeImpacto - 1);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(8, estado.Luchador2.FramesHitstop);
            Assert.AreEqual(1, estado.Luchador2.GolpesEnCombo);
            Assert.IsTrue(estado.Luchador2.Velocidad.X > Fijo.Cero);
            Assert.AreEqual(Juggle.ImpulsoAlGolpear, estado.Luchador2.Velocidad.Y);
        }

        [Test]
        public void UnSegundoGolpeDuranteElHitstunReiniciaElAturdimiento()
        {
            var estado = SimularHastaElImpacto(BotonesEntrada.Ninguno);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 8 + 11);
            estado = Simular(estado, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
            Assert.AreEqual(14, estado.Luchador2.FramesAturdimiento);
            Assert.AreEqual(0, estado.Luchador2.FramesEnAccion);
        }
    }
}
