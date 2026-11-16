using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba y los luchadores pegados (en 0 y en 1): una dirección más el
    /// golpe fuerte pulsados en el frame 1 inician un agarre que atrapa en el frame 4; el rival
    /// sale lanzado en el 14 con 250 de daño y quien agarra recupera el control en el 29. Si el
    /// agarre falla, dura 25 frames. La protección tras un aturdimiento dura 8 frames. El rival sujeto
    /// puede escapar durante 9 frames (del 5 al 13); un agarre roto separa a los dos 12 frames.
    /// </summary>
    public class PruebasAgarres
    {
        private const int FrameDelAgarre = 4;
        private const int FrameDelLanzamiento = 14;
        private const BotonesEntrada AgarreAdelante = BotonesEntrada.Derecha | BotonesEntrada.Fuerte;
        private const BotonesEntrada AgarreAtras = BotonesEntrada.Izquierda | BotonesEntrada.Fuerte;
        private const BotonesEntrada AtrasJugador2 = BotonesEntrada.Derecha;

        private static EstadoPartida AgarreHasta(EstadoPartida inicial, BotonesEntrada agarre, BotonesEntrada jugador2, int frame)
        {
            var estado = Simular(inicial, agarre, jugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, jugador2, frame - 1);
        }

        [Test]
        public void UnaDireccionMasElGolpeFuerteCercaDelRivalEsUnAgarre()
        {
            var adelante = Simular(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, 1);
            var atras = Simular(Inicial(0, 1), AgarreAtras, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.IntentandoAgarre, adelante.Luchador1.Accion);
            Assert.IsFalse(adelante.Luchador1.AgarraHaciaAtras);
            Assert.AreEqual(AccionLuchador.IntentandoAgarre, atras.Luchador1.Accion);
            Assert.IsTrue(atras.Luchador1.AgarraHaciaAtras);
        }

        [Test]
        public void LejosDelRivalOSinDireccionSaleElGolpeFuerte()
        {
            var lejos = Simular(Inicial(0, 3), AgarreAdelante, BotonesEntrada.Ninguno, 1);
            var sinDireccion = Simular(Inicial(0, 1), BotonesEntrada.Fuerte, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Atacando, lejos.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Fuerte, lejos.Luchador1.Ataque);
            Assert.AreEqual(AccionLuchador.Atacando, sinDireccion.Luchador1.Accion);
        }

        [Test]
        public void LaCajaDeAgarreSoloExisteEnSusFramesActivos()
        {
            var arranque = AgarreHasta(Inicial(0, 5), AgarreAdelante, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Atacando, arranque.Luchador1.Accion, "A 5 unidades no hay agarre");

            var estado = Simular(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Arriba, 1);

            Assert.IsFalse(Parametros.CajaAgarreActiva(estado.Luchador1).HasValue);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 3);

            Assert.IsTrue(Parametros.CajaAgarreActiva(estado.Luchador1).HasValue);
            Assert.AreEqual(Fijo.DesdeFraccion(3, 2), Parametros.CajaAgarreActiva(estado.Luchador1).Value.Derecha);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 2);

            Assert.IsFalse(Parametros.CajaAgarreActiva(estado.Luchador1).HasValue);
        }

        [Test]
        public void ElAgarreAtrapaAUnRivalQueBloquea()
        {
            var antes = AgarreHasta(Inicial(0, 1), AgarreAdelante, AtrasJugador2, FrameDelAgarre - 1);

            Assert.AreEqual(AccionLuchador.IntentandoAgarre, antes.Luchador1.Accion);

            var estado = Simular(antes, BotonesEntrada.Ninguno, AtrasJugador2, 1);

            Assert.AreEqual(AccionLuchador.Agarrando, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Agarrado, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima, estado.Luchador2.Vida);
        }

        [Test]
        public void ElRivalSujetoSaleLanzadoConElDanioDelAgarre()
        {
            var sujeto = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelLanzamiento - 1);

            Assert.AreEqual(AccionLuchador.Agarrado, sujeto.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima, sujeto.Luchador2.Vida);

            var estado = Simular(sujeto, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 250, estado.Luchador2.Vida);
            Assert.AreEqual(125, estado.Luchador2.VidaRecuperable);
            Assert.AreEqual(1, estado.Luchador2.GolpesEnCombo);
            Assert.AreEqual(new VectorFijo(Fijo.DesdeFraccion(1, 8), Fijo.DesdeFraccion(1, 4)), estado.Luchador2.Velocidad);
        }

        [Test]
        public void ElRivalLanzadoCaeDerribadoLejos()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelLanzamiento + 30);

            Assert.AreEqual(AccionLuchador.Derribado, estado.Luchador2.Accion);
            Assert.IsTrue(estado.Luchador2.Posicion.X > Fijo.DesdeEntero(2));
        }

        [Test]
        public void AgarrarHaciaAtrasInvierteLasPosicionesAlLanzar()
        {
            var sujeto = AgarreHasta(Inicial(0, 1), AgarreAtras, BotonesEntrada.Ninguno, FrameDelLanzamiento - 1);

            Assert.AreEqual(AccionLuchador.Agarrando, sujeto.Luchador1.Accion);
            Assert.AreEqual(Fijo.Cero, sujeto.Luchador1.Posicion.X);
            Assert.IsTrue(sujeto.Luchador1.MiraDerecha);

            var estado = Simular(sujeto, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.Uno, estado.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.Cero, estado.Luchador2.Posicion.X);
            Assert.IsFalse(estado.Luchador1.MiraDerecha);
            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.IsTrue(estado.Luchador2.Velocidad.X < Fijo.Cero);
        }

        [Test]
        public void QuienAgarraRecuperaElControlTrasLanzar()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, 28);

            Assert.AreEqual(AccionLuchador.Agarrando, estado.Luchador1.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void NingunGolpeAlcanzaALosLuchadoresDuranteLaSujecion()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);

            Assert.IsFalse(ResolucionGolpes.PuedeSerGolpeado(estado.Luchador1, Juggle));
            Assert.IsFalse(ResolucionGolpes.PuedeSerGolpeado(estado.Luchador2, Juggle));
        }

        [Test]
        public void SiElRivalSaltaElAgarreFallaYDejaAlDescubierto()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Arriba, 25);

            Assert.AreEqual(AccionLuchador.IntentandoAgarre, estado.Luchador1.Accion);
            Assert.AreNotEqual(AccionLuchador.Agarrado, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima, estado.Luchador2.Vida);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void NoSeIntentaElAgarreContraUnRivalEnElAire()
        {
            var estado = ConJugador2EnElAire(Inicial(0, 1), 1);

            estado = Simular(estado, AgarreAdelante, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
        }

        /// <summary>
        /// Golpe débil bloqueado junto a la pared, para que el retroceso no saque al rival del
        /// alcance del agarre: el rival sigue en blockstun en el frame 23 y sale de él en el 24.
        /// </summary>
        private static EstadoPartida TrasBloquearUnGolpeDebil()
        {
            var estado = Simular(Inicial(8, 9), BotonesEntrada.Debil, AtrasJugador2, 1);

            return Simular(estado, BotonesEntrada.Ninguno, AtrasJugador2, 22);
        }

        /// <summary>
        /// El atacante recupera el control en el frame 25. Un agarre pulsado entonces está activo
        /// en los frames 28 y 29, dentro de los 8 frames de protección del rival.
        /// </summary>
        [Test]
        public void NoSeAgarraAUnRivalQueAcabaDeSalirDelBlockstun()
        {
            var estado = TrasBloquearUnGolpeDebil();

            Assert.AreEqual(AccionLuchador.Blockstun, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, AgarreAdelante, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.IntentandoAgarre, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 5);

            Assert.AreEqual(AccionLuchador.IntentandoAgarre, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
        }

        [Test]
        public void PasadaLaProteccionElRivalVuelveASerAgarrable()
        {
            var estado = Simular(TrasBloquearUnGolpeDebil(), BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 9);
            estado = AgarreHasta(estado, AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);

            Assert.AreEqual(AccionLuchador.Agarrado, estado.Luchador2.Accion);
        }

        [Test]
        public void LaProteccionSeRenuevaDuranteElAturdimientoYLuegoSeAgota()
        {
            var estado = TrasBloquearUnGolpeDebil();

            Assert.AreEqual(8, estado.Luchador2.FramesProteccionAgarre);
            Assert.IsFalse(ResolucionAgarres.EsAgarrable(estado.Luchador2));

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 7);

            Assert.AreEqual(1, estado.Luchador2.FramesProteccionAgarre);
            Assert.IsFalse(ResolucionAgarres.EsAgarrable(estado.Luchador2));

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.IsTrue(ResolucionAgarres.EsAgarrable(estado.Luchador2));
        }

        [Test]
        public void UnGolpeGanaAUnAgarreEnElMismoFrame()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Ninguno, BotonesEntrada.Debil, 1);
            estado = AgarreHasta(estado, AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador2.Accion);
        }

        [Test]
        public void DosAgarresEnElMismoFrameSeRompen()
        {
            var estado = Simular(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Izquierda | BotonesEntrada.Fuerte, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDelAgarre - 1);

            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador2.Accion);
        }

        [Test]
        public void AgarrarAQuienYaIntentabaSuAgarreLoRompe()
        {
            var estado = Simular(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Izquierda | BotonesEntrada.Fuerte, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, FrameDelAgarre - 2);

            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima, estado.Luchador2.Vida);
        }

        [Test]
        public void ElRivalSujetoEscapaPulsandoElComandoDelAgarre()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Derecha | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador2.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(-1, 8), estado.Luchador1.Velocidad.X);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 8), estado.Luchador2.Velocidad.X);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 40);

            Assert.AreEqual(VidaMaxima, estado.Luchador2.Vida);
            Assert.IsTrue(estado.Luchador2.Posicion.X - estado.Luchador1.Posicion.X > Fijo.Uno);
        }

        [Test]
        public void TrasUnAgarreRotoLosDosRecuperanElControlALaVez()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Derecha | BotonesEntrada.Fuerte, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 11);

            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador2.Accion);
        }

        [Test]
        public void SeEscapaHastaElUltimoFrameDeLaVentana()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre + 8);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Izquierda | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.AgarreRoto, estado.Luchador2.Accion);
        }

        [Test]
        public void PasadaLaVentanaYaNoSeEscapa()
        {
            var estado = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre + 9);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Izquierda | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.GolpeadoEnAire, estado.Luchador2.Accion);
            Assert.AreEqual(VidaMaxima - 250, estado.Luchador2.Vida);
        }

        [Test]
        public void SoloUnaDireccionMasElFuerteEscapa()
        {
            var sujeto = AgarreHasta(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelAgarre);
            var sinDireccion = Simular(sujeto, BotonesEntrada.Ninguno, BotonesEntrada.Fuerte, 1);
            var otroBoton = Simular(sujeto, BotonesEntrada.Ninguno, BotonesEntrada.Derecha | BotonesEntrada.Medio, 1);

            Assert.AreEqual(AccionLuchador.Agarrado, sinDireccion.Luchador2.Accion);
            Assert.AreEqual(AccionLuchador.Agarrado, otroBoton.Luchador2.Accion);
        }

        [Test]
        public void MantenerElFuerteDesdeAntesDelAgarreNoEscapa()
        {
            var estado = Simular(Inicial(0, 1), AgarreAdelante, BotonesEntrada.Fuerte, FrameDelAgarre);

            Assert.AreEqual(AccionLuchador.Agarrado, estado.Luchador2.Accion);

            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Derecha | BotonesEntrada.Fuerte, 3);

            Assert.AreEqual(AccionLuchador.Agarrado, estado.Luchador2.Accion);
        }

        [Test]
        public void UnAgarrePuedeNoquear()
        {
            var estado = AgarreHasta(ConVidas(Inicial(0, 1), VidaMaxima, 200), AgarreAdelante, BotonesEntrada.Ninguno, FrameDelLanzamiento);

            Assert.AreEqual(0, estado.Luchador2.Vida);
            Assert.AreEqual(ResultadoRonda.GanaJugador1, estado.Resultado);
        }

        [Test]
        public void CancelarUnGolpeEnElFuerteConDireccionNoEsUnAgarre()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Ninguno, 4);
            estado = Simular(estado, AgarreAdelante, BotonesEntrada.Ninguno, 1);
            estado = Simular(estado, BotonesEntrada.Derecha, BotonesEntrada.Ninguno, 8);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Fuerte, estado.Luchador1.Ataque);
        }
    }
}
