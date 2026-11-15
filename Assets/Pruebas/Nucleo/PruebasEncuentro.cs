using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Encuentros;
using FgcTag.Nucleo.Equipos;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: encuentro al mejor de 3 (2 victorias) y 20 frames de pausa
    /// entre rondas. Para acabar una ronda rápido se deja a un equipo con un solo luchador a un
    /// golpe del K.O.: el golpe débil pulsado en el primer frame lo decide 5 frames después.
    /// </summary>
    public class PruebasEncuentro
    {
        private const int FramesHastaElNocaut = 5;

        private static EstadoEquipo AUnGolpe(EstadoEquipo equipo) =>
            equipo.ConVidaEnReserva(1, 0).ConVidaEnReserva(2, 0);

        /// <summary>Deja la ronda actual lista para que el jugador indicado (o los dos, con 0) pierda con un solo golpe.</summary>
        private static EstadoEncuentro ConPerdedorAUnGolpe(EstadoEncuentro estado, int perdedor)
        {
            var ronda = estado.Ronda;
            var cae1 = perdedor != 2;
            var cae2 = perdedor != 1;

            var preparada = new EstadoCombateEquipos(
                ConVidas(ronda.Duelo, cae1 ? 100 : ronda.Duelo.Luchador1.Vida, cae2 ? 100 : ronda.Duelo.Luchador2.Vida),
                cae1 ? AUnGolpe(ronda.Equipo1) : ronda.Equipo1,
                cae2 ? AUnGolpe(ronda.Equipo2) : ronda.Equipo2,
                ronda.Resultado,
                ronda.FramesDesdeNocaut);

            return estado.ConRonda(preparada);
        }

        /// <summary>Juega la ronda actual hasta su K.O.: gana el jugador indicado, o empatan con 0.</summary>
        private static EstadoEncuentro JugarRonda(EstadoEncuentro estado, int ganador)
        {
            var preparado = ConPerdedorAUnGolpe(estado, ganador == 1 ? 2 : ganador == 2 ? 1 : 0);
            var golpea1 = ganador != 2 ? BotonesEntrada.Debil : BotonesEntrada.Ninguno;
            var golpea2 = ganador != 1 ? BotonesEntrada.Debil : BotonesEntrada.Ninguno;

            return SimularEncuentro(SimularEncuentro(preparado, golpea1, golpea2, 1), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesHastaElNocaut - 1);
        }

        private static EstadoEncuentro EsperarLaSiguienteRonda(EstadoEncuentro estado) =>
            SimularEncuentro(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntreRondas);

        [Test]
        public void ElEncuentroEmpiezaEnLaPrimeraRondaConElMarcadorACero()
        {
            var estado = EstadoEncuentro.Crear(Encuentro);

            Assert.AreEqual(1, estado.NumeroDeRonda);
            Assert.AreEqual(0, estado.Victorias1);
            Assert.AreEqual(0, estado.Victorias2);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
            Assert.IsFalse(estado.EntreRondas);
        }

        [Test]
        public void GanarUnaRondaSumaUnaVictoriaSinAcabarElEncuentro()
        {
            var estado = JugarRonda(EstadoEncuentro.Crear(Encuentro), 1);

            Assert.AreEqual(1, estado.Victorias1);
            Assert.AreEqual(0, estado.Victorias2);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
            Assert.IsTrue(estado.EntreRondas);
        }

        [Test]
        public void LaVictoriaSeAnotaUnaSolaVez()
        {
            var estado = JugarRonda(EstadoEncuentro.Crear(Encuentro), 1);
            estado = SimularEncuentro(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FramesEntreRondas - 1);

            Assert.AreEqual(1, estado.Victorias1);
            Assert.AreEqual(1, estado.NumeroDeRonda);
            Assert.IsTrue(estado.EntreRondas);
        }

        [Test]
        public void TrasLaPausaEmpiezaUnaRondaNuevaConLosEquiposCompletos()
        {
            var estado = EsperarLaSiguienteRonda(JugarRonda(EstadoEncuentro.Crear(Encuentro), 1));

            Assert.AreEqual(2, estado.NumeroDeRonda);
            Assert.AreEqual(1, estado.Victorias1);
            Assert.IsFalse(estado.EntreRondas);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Ronda.Resultado);
            Assert.AreEqual(VidaMaxima, estado.Ronda.Duelo.Luchador2.Vida);
            Assert.AreEqual(VidaSegundo, estado.Ronda.Equipo2.VidaEnReserva(1));
            Assert.AreEqual(0, estado.Ronda.Equipo2.IndiceActivo);
            Assert.AreEqual(FramesPorRonda, estado.Ronda.Duelo.FramesRestantes);
            Assert.AreEqual(0, estado.FramesTrasLaRonda);
        }

        [Test]
        public void DosVictoriasGananElEncuentroYNoHayMasRondas()
        {
            var estado = JugarRonda(EsperarLaSiguienteRonda(JugarRonda(EstadoEncuentro.Crear(Encuentro), 1)), 1);

            Assert.AreEqual(2, estado.Victorias1);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
            Assert.IsTrue(estado.Terminado);

            estado = SimularEncuentro(estado, BotonesEntrada.Debil, BotonesEntrada.Debil, FramesEntreRondas + 30);

            Assert.AreEqual(2, estado.NumeroDeRonda);
            Assert.AreEqual(2, estado.Victorias1);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
        }

        [Test]
        public void ConUnaVictoriaCadaUnoLaTerceraRondaDecide()
        {
            var estado = JugarRonda(EstadoEncuentro.Crear(Encuentro), 1);
            estado = JugarRonda(EsperarLaSiguienteRonda(estado), 2);

            Assert.AreEqual(1, estado.Victorias1);
            Assert.AreEqual(1, estado.Victorias2);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);

            estado = JugarRonda(EsperarLaSiguienteRonda(estado), 2);

            Assert.AreEqual(3, estado.NumeroDeRonda);
            Assert.AreEqual(ResultadoRonda.GanaJugador2, estado.Resultado);
        }

        [Test]
        public void UnaRondaEmpatadaSumaUnaVictoriaACadaJugador()
        {
            var estado = JugarRonda(EstadoEncuentro.Crear(Encuentro), 0);

            Assert.AreEqual(ResultadoRonda.Empate, estado.Ronda.Resultado);
            Assert.AreEqual(1, estado.Victorias1);
            Assert.AreEqual(1, estado.Victorias2);
            Assert.AreEqual(ResultadoRonda.EnCurso, estado.Resultado);
        }

        [Test]
        public void SiLosDosCompletanSusVictoriasALaVezElEncuentroEsEmpate()
        {
            var estado = JugarRonda(EsperarLaSiguienteRonda(JugarRonda(EstadoEncuentro.Crear(Encuentro), 0)), 0);

            Assert.AreEqual(2, estado.Victorias1);
            Assert.AreEqual(2, estado.Victorias2);
            Assert.AreEqual(ResultadoRonda.Empate, estado.Resultado);
        }

        [Test]
        public void ConLaRondaTerminadaLasEntradasNoMuevenANadie()
        {
            var terminada = JugarRonda(EstadoEncuentro.Crear(Encuentro), 1);
            var estado = SimularEncuentro(terminada, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 10);

            Assert.AreEqual(terminada.Ronda.Duelo.Luchador1.Posicion, estado.Ronda.Duelo.Luchador1.Posicion);
        }

        /// <summary>
        /// Valor dorado del encuentro: con un reloj de 5 segundos, las entradas pseudoaleatorias
        /// recorren varias rondas, sus pausas y el final. Si cambia, cambió el comportamiento.
        /// </summary>
        [Test]
        public void RepeticionDeReferenciaDelEncuentroConservaSuSumaDeVerificacion()
        {
            var configuracion = EncuentroConReloj(300);
            var estado = EstadoEncuentro.Crear(configuracion);
            var semilla = 1530u;

            for (var i = 0; i < 1500; i++)
            {
                semilla = unchecked(semilla * 1103515245u + 12345u);
                var jugador1 = (BotonesEntrada)((semilla >> 16) & 0x3FF);
                semilla = unchecked(semilla * 1103515245u + 12345u);
                var jugador2 = (BotonesEntrada)((semilla >> 16) & 0x3FF);
                estado = SimuladorEncuentro.Avanzar(estado, new EntradasFrame(jugador1, jugador2), configuracion);
            }

            Assert.IsTrue(estado.Terminado, "El encuentro de referencia debe llegar a decidirse");
            Assert.AreEqual("F57684EF664BD019", estado.CalcularSumaVerificacion().ToString());
        }
    }
}
