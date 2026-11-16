using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: los miembros de cada equipo tienen 1000, 1200 y 1400 de vida;
    /// quien entra tarda 10 frames en poder actuar; tras un cambio hay 60 frames de espera; el
    /// relevo llega 30 frames después del K.O. Un golpe débil pulsado en el frame 1 conecta en el 5.
    /// </summary>
    public class PruebasEquipos
    {
        private const int FrameDeImpacto = 5;
        private const int FrameDelRelevo = FrameDeImpacto + FramesPausaNocaut - 1;

        /// <summary>El jugador 1 pulsa Débil en el frame 1 y se simula hasta el frame indicado.</summary>
        private static EstadoCombateEquipos GolpeDebilHasta(EstadoCombateEquipos inicial, int frame)
        {
            var estado = SimularEquipos(inicial, BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);

            return SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, frame - 1);
        }

        private static EstadoEquipo SinCompaneros(EstadoEquipo equipo) =>
            equipo.ConVidaEnReserva(1, 0).ConVidaEnReserva(2, 0);

        private static EntradasFrame EntradasPseudoaleatorias(ref uint semilla)
        {
            semilla = unchecked(semilla * 1103515245u + 12345u);
            var jugador1 = (BotonesEntrada)((semilla >> 16) & 0x3FF);
            semilla = unchecked(semilla * 1103515245u + 12345u);
            var jugador2 = (BotonesEntrada)((semilla >> 16) & 0x3FF);

            return new EntradasFrame(jugador1, jugador2);
        }

        private static EstadoCombateEquipos SimularPseudoaleatorio(uint semilla, int frames)
        {
            var estado = InicialPorEquipos(-1, 1);

            for (var i = 0; i < frames; i++)
            {
                estado = SimuladorEquipos.Avanzar(estado, EntradasPseudoaleatorias(ref semilla), Equipos);
            }

            return estado;
        }

        [Test]
        public void ElCombateEmpiezaConElPrimeroDeCadaEquipoYTodosConVida()
        {
            var estado = InicialPorEquipos(-2, 2);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(0, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(VidaMaxima, estado.Duelo.Luchador1.Vida);
            Assert.AreEqual(VidaMaxima + VidaSegundo + VidaTercero, estado.Equipo1.VidaTotal(estado.Duelo.Luchador1));
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
        }

        [Test]
        public void MantenerAsistencia1CambiaAlSiguienteCompaneroQueEntraSaltandoDesdeAtras()
        {
            var inicial = InicialPorEquipos(-2, 2);
            var antes = SimularEquipos(inicial, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, FramesMantenerParaCambiar - 1);

            Assert.AreEqual(0, antes.Equipo1.IndiceActivo);

            var estado = SimularEquipos(antes, BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(VidaSegundo, estado.Duelo.Luchador1.Vida);
            Assert.AreEqual(AccionLuchador.Entrando, estado.Duelo.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(-15, 4), estado.Duelo.Luchador1.Posicion.X);
            Assert.IsTrue(estado.Duelo.Luchador1.Posicion.Y > Fijo.Uno);
            Assert.IsTrue(estado.Duelo.Luchador1.MiraDerecha);
            Assert.IsFalse(estado.Equipo1.Asistente.EnPantalla);
            Assert.AreEqual(0, estado.Equipo2.IndiceActivo);
        }

        [Test]
        public void QuienEntraAterrizaDondeEstabaSuCompanero()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntrada - 2);

            Assert.AreEqual(AccionLuchador.Entrando, estado.Duelo.Luchador1.Accion);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Duelo.Luchador1.Accion);
            Assert.AreEqual(VectorFijo.DesdeEnteros(-2, 0), estado.Duelo.Luchador1.Posicion);
        }

        [Test]
        public void ContraLaParedQuienEntraCaeEnVertical()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-9, 2), BotonesEntrada.Asistencia1);

            Assert.IsTrue(estado.Duelo.Luchador1.Posicion.X < Fijo.DesdeFraccion(-19, 2));

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntrada - 1);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Duelo.Luchador1.Accion);
            Assert.IsTrue(Fijo.Abs(estado.Duelo.Luchador1.Posicion.X - Fijo.DesdeEntero(-9)) < Fijo.DesdeFraccion(1, 100));
        }

        [Test]
        public void QuienSaleSeVeSaltarHaciaAtrasYDesaparece()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1);

            Assert.IsTrue(estado.Equipo1.Saliente.EnPantalla);
            Assert.AreEqual(0, estado.Equipo1.Saliente.Miembro);
            Assert.AreEqual(VectorFijo.DesdeEnteros(-2, 0), estado.Equipo1.Saliente.Origen);
            Assert.AreEqual(1, estado.Equipo1.Saliente.Frames);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 3);

            Assert.AreEqual(VectorFijo.DesdeEnteros(-3, 1), estado.Equipo1.Saliente.Posicion(Equipos.VelocidadSalida));

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesSalida - 5);

            Assert.IsTrue(estado.Equipo1.Saliente.EnPantalla);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.IsFalse(estado.Equipo1.Saliente.EnPantalla);
        }

        [Test]
        public void MantenerAsistencia2CambiaAlOtroCompanero()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia2);

            Assert.AreEqual(2, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(VidaTercero, estado.Duelo.Luchador1.Vida);
        }

        [Test]
        public void SeguirManteniendoElBotonNoVuelveACambiar()
        {
            var estado = SimularEquipos(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, FramesMantenerParaCambiar + FramesEsperaCambio + 20);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
        }

        [Test]
        public void QuienEntraNoTieneControlHastaAterrizar()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1);
            estado = SimularEquipos(estado, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, FramesHastaActuarTrasEntrar - 1);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Duelo.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeEntero(-2), estado.Duelo.Luchador1.Posicion.X);

            estado = SimularEquipos(estado, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.CaminandoAdelante, estado.Duelo.Luchador1.Accion);
        }

        [Test]
        public void MantenerUnGolpeDuranteElCambioNoLanzaUnAtaque()
        {
            var estado = SimularEquipos(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1, BotonesEntrada.Ninguno, FramesMantenerParaCambiar - 1);
            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1 | BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1 + FramesHastaActuarTrasEntrar);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Duelo.Luchador1.Accion);
        }

        [Test]
        public void ElCambioTieneEsperaAntesDeRepetirse()
        {
            var estado = CambiarJugador1(InicialPorEquipos(-2, 2), BotonesEntrada.Asistencia1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 20);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia1);

            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEsperaCambio);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia1);

            Assert.AreEqual(2, estado.Equipo1.IndiceActivo);
        }

        [Test]
        public void LaVidaDeQuienSaleSeConservaHastaQueVuelve()
        {
            var estado = CambiarJugador1(ConVidasActivas(InicialPorEquipos(-2, 2), 700, VidaMaxima), BotonesEntrada.Asistencia1);

            Assert.AreEqual(700, estado.Equipo1.VidaDe(0, estado.Duelo.Luchador1));

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEsperaCambio);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia2);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(700, estado.Duelo.Luchador1.Vida);
        }

        [Test]
        public void NoSeCambiaConUnAtaqueQueFallaNiEnElAire()
        {
            var atacando = SimularEquipos(InicialPorEquipos(-5, 5), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            atacando = CambiarJugador1(atacando, BotonesEntrada.Asistencia1);

            var saltando = SimularEquipos(InicialPorEquipos(-5, 5), BotonesEntrada.Arriba, BotonesEntrada.Ninguno, 8);
            saltando = CambiarJugador1(saltando, BotonesEntrada.Asistencia1);

            Assert.AreEqual(0, atacando.Equipo1.IndiceActivo);
            Assert.AreEqual(0, saltando.Equipo1.IndiceActivo);
        }

        [Test]
        public void NoSeCambiaAUnCompaneroSinVida()
        {
            var inicial = InicialPorEquipos(-2, 2);
            var conBaja = ConEquipo1(inicial, inicial.Equipo1.ConVidaEnReserva(1, 0));
            var estado = CambiarJugador1(conBaja, BotonesEntrada.Asistencia1);

            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
            estado = CambiarJugador1(estado, BotonesEntrada.Asistencia2);

            Assert.AreEqual(2, estado.Equipo1.IndiceActivo);
        }

        [Test]
        public void QuienEntraNoPuedeBloquearYRecibeElGolpeEnElAire()
        {
            var atrasYCambio = BotonesEntrada.Derecha | BotonesEntrada.Asistencia1;
            var estado = SimularEquipos(InicialPorEquipos(0, 1), BotonesEntrada.Fuerte, atrasYCambio, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, atrasYCambio, FramesMantenerParaCambiar - 1);

            Assert.AreEqual(1, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(AccionLuchador.Entrando, estado.Duelo.Luchador2.Accion);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Derecha, 6);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Duelo.Luchador2.Accion);
            Assert.AreEqual(VidaSegundo - 300, estado.Duelo.Luchador2.Vida);
        }

        [Test]
        public void TrasUnNocautElDueloSeDetieneYElCombateSigue()
        {
            var estado = GolpeDebilHasta(ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100), FrameDeImpacto);

            Assert.AreEqual(AccionLuchador.Noqueado, estado.Duelo.Luchador2.Accion);
            Assert.IsTrue(estado.EsperandoRelevo);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);

            var relojAlCaer = estado.Duelo.FramesRestantes;
            estado = SimularEquipos(estado, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, FramesPausaNocaut - 2);

            Assert.IsTrue(estado.EsperandoRelevo);
            Assert.AreEqual(0, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(relojAlCaer, estado.Duelo.FramesRestantes);
            Assert.AreEqual(Fijo.Cero, estado.Duelo.Luchador1.Posicion.X);
        }

        [Test]
        public void ElSiguienteLuchadorEntraTrasLaPausaDeNocaut()
        {
            var estado = GolpeDebilHasta(ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100), FrameDelRelevo);

            Assert.IsFalse(estado.EsperandoRelevo);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Duelo.Resultado);
            Assert.AreEqual(1, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(0, estado.Equipo2.VidaEnReserva(0));
            Assert.AreEqual(VidaSegundo, estado.Duelo.Luchador2.Vida);
            Assert.AreEqual(AccionLuchador.Entrando, estado.Duelo.Luchador2.Accion);
            Assert.AreEqual(VectorFijo.DesdeEnteros(6, 2), estado.Duelo.Luchador2.Posicion);
            Assert.IsFalse(estado.Duelo.Luchador2.MiraDerecha);
            Assert.AreEqual(0, estado.FramesDesdeNocaut);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntrada);

            Assert.AreEqual(AccionLuchador.Aterrizaje, estado.Duelo.Luchador2.Accion);
            Assert.AreEqual(VectorFijo.DesdeEnteros(4, 0), estado.Duelo.Luchador2.Posicion);
        }

        [Test]
        public void ElRelevoSaltaALosCompanerosSinVida()
        {
            var inicial = ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100);
            var estado = GolpeDebilHasta(ConEquipo2(inicial, inicial.Equipo2.ConVidaEnReserva(1, 0)), FrameDelRelevo);

            Assert.AreEqual(2, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(VidaTercero, estado.Duelo.Luchador2.Vida);
        }

        [Test]
        public void ElRelevoEntraDentroDelEscenarioAunqueElRivalEsteCercaDeLaPared()
        {
            var estado = GolpeDebilHasta(ConVidasActivas(InicialPorEquipos(8, 9), VidaMaxima, 100), FrameDelRelevo + FramesEntrada);

            Assert.IsTrue(estado.Duelo.Luchador2.Posicion.X <= Fijo.DesdeEntero(10) - Fijo.Medio);
            Assert.IsTrue(estado.Duelo.Luchador2.Posicion.X > estado.Duelo.Luchador1.Posicion.X);
        }

        [Test]
        public void PierdeElEquipoQueSeQuedaSinLuchadores()
        {
            var inicial = ConVidasActivas(InicialPorEquipos(0, 1), VidaMaxima, 100);
            var estado = GolpeDebilHasta(ConEquipo2(inicial, SinCompaneros(inicial.Equipo2)), FrameDeImpacto);

            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.IsTrue(estado.Terminado);

            estado = SimularEquipos(estado, BotonesEntrada.Asistencia1, BotonesEntrada.Asistencia1, FramesPausaNocaut + 10);

            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.AreEqual(0, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(0, estado.Equipo2.IndiceActivo);
            Assert.AreEqual(AccionLuchador.Noqueado, estado.Duelo.Luchador2.Accion);
        }

        [Test]
        public void UnDobleNocautRelevaALosDosEquipos()
        {
            var estado = SimularEquipos(ConVidasActivas(InicialPorEquipos(0, 1), 100, 100), BotonesEntrada.Debil, BotonesEntrada.Debil, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDelRelevo - 1);

            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
            Assert.AreEqual(1, estado.Equipo1.IndiceActivo);
            Assert.AreEqual(1, estado.Equipo2.IndiceActivo);

            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntrada);

            Assert.AreEqual(Fijo.DesdeEntero(-2), estado.Duelo.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.DesdeEntero(2), estado.Duelo.Luchador2.Posicion.X);
        }

        [Test]
        public void UnDobleNocautDeLosUltimosLuchadoresEsEmpate()
        {
            var inicial = ConVidasActivas(InicialPorEquipos(0, 1), 100, 100);
            var sinReservas = ConEquipo1(ConEquipo2(inicial, SinCompaneros(inicial.Equipo2)), SinCompaneros(inicial.Equipo1));
            var estado = SimularEquipos(sinReservas, BotonesEntrada.Debil, BotonesEntrada.Debil, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1);

            Assert.AreEqual(ResultadoRonda.Empate, estado.Resultado);
        }

        [Test]
        public void SiCaeElUltimoDeUnEquipoGanaElOtroAunqueSuLuchadorTambienCaiga()
        {
            var inicial = ConVidasActivas(InicialPorEquipos(0, 1), 100, 100);
            var estado = SimularEquipos(ConEquipo2(inicial, SinCompaneros(inicial.Equipo2)), BotonesEntrada.Debil, BotonesEntrada.Debil, 1);
            estado = SimularEquipos(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDeImpacto - 1);

            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
        }

        [Test]
        public void ConElTiempoAgotadoGanaElEquipoConMasVidaTotal()
        {
            var configuracion = EquiposConReloj(10);
            var inicial = ConVidasActivas(EstadoCombateEquipos.Crear(VectorFijo.DesdeEnteros(-2, 0), VectorFijo.DesdeEnteros(2, 0), configuracion), 100, 900);
            var estado = ConEquipo2(inicial, inicial.Equipo2.ConVidaEnReserva(1, 100).ConVidaEnReserva(2, 100));

            for (var i = 0; i < 10; i++)
            {
                estado = SimuladorEquipos.Avanzar(estado, EntradasFrame.Vacias, configuracion);
            }

            Assert.AreEqual(ResultadoRonda.GanaJugador2, estado.Duelo.Resultado);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
        }

        [Test]
        public void MismasEntradasProducenElMismoCombate()
        {
            var a = SimularPseudoaleatorio(2026u, 900);
            var b = SimularPseudoaleatorio(2026u, 900);

            Assert.AreEqual(a.CalcularSumaVerificacion().Valor, b.CalcularSumaVerificacion().Valor);
        }

        /// <summary>
        /// Valor dorado del combate por equipos: si cambia, cambió el comportamiento de la
        /// simulación. Actualizarlo exige una entrada en el registro de decisiones.
        /// </summary>
        [Test]
        public void RepeticionDeReferenciaPorEquiposConservaSuSumaDeVerificacion()
        {
            var estado = SimularPseudoaleatorio(1530u, 900);

            Assert.AreEqual("3EB19DC9D234B5E5", estado.CalcularSumaVerificacion().ToString());
        }
    }
}
