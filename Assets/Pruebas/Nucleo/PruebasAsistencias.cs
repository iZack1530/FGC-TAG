using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: pulsar y soltar un botón de asistencia llama al asistente en el
    /// frame 2; tarda 6 frames en llegar y su golpe medio (200 de daño, 10 de hitstop) conecta en
    /// el frame 14. Mantener el botón 4 frames cambia de luchador. La reserva regenera 5 por frame.
    /// </summary>
    public class PruebasAsistencias
    {
        private const int FrameDeLlamada = 2;
        private const int FrameDelGolpeDelAsistente = FrameDeLlamada + FramesLlegadaAsistencia + 6;
        private const BotonesEntrada AtrasJugador2 = BotonesEntrada.Derecha;

        private static EstadoCombateEquipos AsistenciaHastaElFrame(EstadoCombateEquipos inicial, BotonesEntrada jugador2, int frame)
        {
            var estado = LlamarAsistenteJugador1(inicial, BotonesEntrada.Asistencia1, jugador2);

            return SimularEquipos(estado, BotonesEntrada.Ninguno, jugador2, frame - FrameDeLlamada);
        }

        [Test]
        public void PulsarYSoltarLlamaAlCompaneroSinCambiarDeLuchador()
        {
            var estado = LlamarAsistenteJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);
            Assert.AreEqual(1, estado.Equipo1.Asistente.Miembro);
            Assert.AreEqual(VectorFijo.DesdeEnteros(-2, 0), estado.Equipo1.Asistente.Posicion);
            Assert.IsTrue(estado.Equipo1.Asistente.MiraDerecha);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Duelo.Luchador1.Accion);
        }

        [Test]
        public void Asistencia2LlamaAlOtroCompanero()
        {
            var estado = LlamarAsistenteJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia2, BotonesEntrada.Ninguno);

            Assert.AreEqual(2, estado.Equipo1.Asistente.Miembro);
        }

        [Test]
        public void ElAsistenteGolpeaAlRivalConSuAtaque()
        {
            var antes = AsistenciaHastaElFrame(InicialPorEquipos(0, 1), BotonesEntrada.Ninguno, FrameDelGolpeDelAsistente - 1);

            Assert.AreEqual(VidaMaxima, antes.Duelo.Luchador2.Vida);

            var estado = SimularEquipos(antes, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Duelo.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 200, estado.Duelo.Luchador2.Vida);
            Assert.AreEqual(100, estado.Duelo.Luchador2.VidaRecuperable);
            Assert.IsTrue(estado.Equipo1.Asistente.GolpeConectado);
            Assert.AreEqual(10, estado.Equipo1.Asistente.FramesHitstop);
            Assert.AreEqual(0, estado.Duelo.Luchador1.FramesHitstop);
        }

        [Test]
        public void ElGolpeDelAsistenteSePuedeBloquear()
        {
            var estado = AsistenciaHastaElFrame(InicialPorEquipos(0, 1), BotonesEntrada.Ninguno, FrameDelGolpeDelAsistente - 2);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, AtrasJugador2, 2);

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Duelo.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 20, estado.Duelo.Luchador2.Vida);
        }

        [Test]
        public void ElAsistenteSeMarchaAlTerminarSuAtaque()
        {
            var ultimoFrame = FrameDeLlamada + FramesLlegadaAsistencia + GolpeMedio.DuracionTotal - 1;
            var estado = AsistenciaHastaElFrame(InicialPorEquipos(-5, 5), BotonesEntrada.Ninguno, ultimoFrame);

            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void LaAsistenciaTieneEsperaAntesDeRepetirse()
        {
            var estado = AsistenciaHastaElFrame(InicialPorEquipos(-5, 5), BotonesEntrada.Ninguno, 40);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);

            estado = LlamarAsistenteJugador1(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEsperaAsistencia);
            estado = LlamarAsistenteJugador1(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);

            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void NoSeLlamaAUnCompaneroSinVida()
        {
            var inicial = InicialPorEquipos(-2, 2);
            var estado = LlamarAsistenteJugador1(ConEquipo1(inicial, inicial.Equipo1.ConVidaEnReserva(1, 0)), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void NoSeLlamaAUnAsistenteMientrasSeRecibeUnGolpe()
        {
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Ninguno, BotonesEntrada.Debil, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Duelo.Luchador1.Accion);

            estado = LlamarAsistenteJugador1(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void SePuedeLlamarAlAsistenteMientrasSeAtaca()
        {
            var estado = SimularEquipos(InicialPorEquipos(-5, 5), BotonesEntrada.Debil | BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Duelo.Luchador1.Accion);
            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void ElGolpeDelAsistenteCuentaParaElCombo()
        {
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Debil | BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDelGolpeDelAsistente - 1);

            Assert.AreEqual(2, estado.Duelo.Luchador2.GolpesEnCombo);
            Assert.AreEqual(VidaMaxima - 100 - 180, estado.Duelo.Luchador2.Vida);
        }

        [Test]
        public void NoSeCambiaAlCompaneroQueEstaAsistiendo()
        {
            var estado = LlamarAsistenteJugador1(InicialPorEquipos(-5, 5), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia1);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia2);

            Assert.AreEqual(2, estado.Equipo1.IndiceActivo);
            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void ElAsistenteSeRetiraCuandoElDueloSeDetiene()
        {
            var inicial = ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100);
            var estado = SimularEquipos(inicial, BotonesEntrada.Debil | BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            Assert.IsTrue(estado.EsperandoRelevo);
            Assert.IsTrue(estado.Equipo1.Asistente.EnPantalla);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);
        }

        [Test]
        public void UnGolpeQueConectaSeInterrumpeConUnCambioYElQueEntraTieneControl()
        {
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);
            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 8);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(0, estado.Duelo.Luchador1.FramesHitstop);

            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Duelo.Luchador1.Accion);
            Assert.AreEqual(VidaSegundo, estado.Duelo.Luchador1.Vida);
            Assert.AreEqual(AccionLuchador.Hitstun, estado.Duelo.Luchador2.Accion);
        }

        [Test]
        public void ElQueEntraEnMitadDelComboPuedeSeguirlo()
        {
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);
            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 9);
            estado = SimularEquipos(estado, BotonesEntrada.Medio, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 6);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(2, estado.Duelo.Luchador2.GolpesEnCombo);
            Assert.AreEqual(VidaMaxima - 100 - 180, estado.Duelo.Luchador2.Vida);
        }

        [Test]
        public void UnGolpeBloqueadoNoPermiteElCambioEnCombo()
        {
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Debil, AtrasJugador2, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, AtrasJugador2, 4);
            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1, AtrasJugador2, 9);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(AccionLuchador.Atacando, estado.Duelo.Luchador1.Accion);
        }

        [Test]
        public void LaVidaRecuperableSeRegeneraEnReserva()
        {
            var inicial = ConRecuperableActivo1(ConVidasActivas(InicialPorEquipos(-5, 5), 700, VidaMaxima), 200);
            var estado = CambiarJugador1(inicial, BotonesEntrada.Asistencia1);

            Assert.AreEqual(705, estado.Equipo1.VidaDe(0, estado.Duelo.Luchador1));
            Assert.AreEqual(195, estado.Equipo1.RecuperableDe(0, estado.Duelo.Luchador1));

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 39);

            Assert.AreEqual(900, estado.Equipo1.VidaDe(0, estado.Duelo.Luchador1));
            Assert.AreEqual(0, estado.Equipo1.RecuperableDe(0, estado.Duelo.Luchador1));

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 30);

            Assert.AreEqual(900, estado.Equipo1.VidaDe(0, estado.Duelo.Luchador1));
        }

        [Test]
        public void QuienVuelveDeLaReservaTraeSuVidaRegenerada()
        {
            var inicial = ConRecuperableActivo1(ConVidasActivas(InicialPorEquipos(-5, 5), 700, VidaMaxima), 600);
            var estado = CambiarJugador1(inicial, BotonesEntrada.Asistencia1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEsperaCambio);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia2);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(700 + 5 * (FramesEsperaCambio + FramesMantenerParaCambiar), estado.Duelo.Luchador1.Vida);
            Assert.AreEqual(600 - 5 * (FramesEsperaCambio + FramesMantenerParaCambiar), estado.Duelo.Luchador1.VidaRecuperable);
        }

        [Test]
        public void LaReservaNoRegeneraConElDueloDetenido()
        {
            var inicial = ConRecuperableActivo1(ConVidasActivas(InicialPorEquipos(0, 1), 700, VidaMaxima), 600);
            var estado = CambiarJugador1(inicial, BotonesEntrada.Asistencia1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntrada);
            estado = ConVidasActivas(estado, estado.Duelo.Luchador1.Vida, 100);
            estado = SimularEquipos(estado, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);

            Assert.IsTrue(estado.EsperandoRelevo);

            var vidaEnReserva = estado.Equipo1.VidaEnReserva(0);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 10);

            Assert.AreEqual(vidaEnReserva, estado.Equipo1.VidaEnReserva(0));
        }
    }
}
