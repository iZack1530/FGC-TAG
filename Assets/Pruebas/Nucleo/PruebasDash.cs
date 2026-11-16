using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;
using static FgcTag.Pruebas.Nucleo.EscenarioPrueba;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Con el escenario de prueba: el dash en tierra dura 12 frames, empieza a 1/2 por frame (3/8
    /// hacia atrás) y pierde 1/32 por frame; se cancela con un ataque desde su frame 2 y
    /// agachándose o saltando desde el 4. El plink admite 2 frames. El dash aéreo dura 8 frames a
    /// 1/2 por frame, se cancela con un ataque aéreo desde el frame 3 y exige 1 unidad de altura.
    /// </summary>
    public class PruebasDash
    {
        private const BotonesEntrada DosGolpes = BotonesEntrada.Debil | BotonesEntrada.Medio;

        private static EstadoPartida DashHasta(EstadoPartida inicial, BotonesEntrada comando, int frame)
        {
            var estado = SimularJugador1(inicial, comando, 1);

            return SimularJugador1(estado, BotonesEntrada.Ninguno, frame - 1);
        }

        [Test]
        public void DosGolpesALaVezHacenUnDashHaciaElRivalEnVezDeUnAtaque()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
            Assert.AreEqual(Fijo.DesdeFraccion(-9, 2), estado.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.DesdeFraccion(15, 32), estado.Luchador1.Velocidad.X);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(Fijo.DesdeFraccion(-9, 2) + Fijo.DesdeFraccion(15, 32), estado.Luchador1.Posicion.X);
            Assert.AreEqual(Fijo.DesdeFraccion(14, 32), estado.Luchador1.Velocidad.X);
        }

        [Test]
        public void CualquierParDeGolpesSirveYElEspecialNoCuenta()
        {
            var medioFuerte = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Medio | BotonesEntrada.Fuerte, 1);
            var conEspecial = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Debil | BotonesEntrada.Especial, 1);

            Assert.AreEqual(AccionLuchador.Dash, medioFuerte.Luchador1.Accion);
            Assert.AreEqual(AccionLuchador.Atacando, conEspecial.Luchador1.Accion);
        }

        [Test]
        public void ManteniendoAtrasElDashSeAlejaDelRival()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Izquierda | DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeEntero(-5) - Fijo.DesdeFraccion(3, 8), estado.Luchador1.Posicion.X);
        }

        [Test]
        public void ElJugador2HaceElDashHaciaLaIzquierda()
        {
            var estado = Simular(Inicial(-5, 5), BotonesEntrada.Ninguno, DosGolpes, 1);

            Assert.AreEqual(Fijo.DesdeFraccion(9, 2), estado.Luchador2.Posicion.X);
        }

        [Test]
        public void ElDashTerminaSoloYRecorreMasQueCaminar()
        {
            var estado = DashHasta(Inicial(-5, 5), DosGolpes, 12);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Velocidad.X);
            Assert.IsTrue(estado.Luchador1.Posicion.X > Fijo.DesdeEntero(-5) + Fijo.DesdeFraccion(1, 4) * 12);
        }

        [Test]
        public void MantenerLosDosBotonesNoEncadenaOtroDash()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 14);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
        }

        [Test]
        public void ConAbajoMantenidoSaleElAtaqueAgachado()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Abajo | DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.AtacandoAgachado, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElDashSeCancelaAgachandoseYDesdeAhiSeRepite()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Abajo, 3);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion, "Antes del frame 4 del dash no se cancela");

            estado = SimularJugador1(estado, BotonesEntrada.Abajo, 1);

            Assert.AreEqual(AccionLuchador.Agachado, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Velocidad.X);

            var posicionAgachado = estado.Luchador1.Posicion.X;
            estado = SimularJugador1(estado, DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);
            Assert.AreEqual(posicionAgachado + Fijo.Medio, estado.Luchador1.Posicion.X);
        }

        [Test]
        public void ElDashSeCancelaConUnAtaqueAntesQueAgachandose()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Fuerte, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElAtaqueQueCancelaUnDashHeredaSuImpulsoHastaFrenarse()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Fuerte, 2);
            var alEmpezar = estado.Luchador1.Posicion.X;

            Assert.AreEqual(Fijo.DesdeFraccion(13, 32), estado.Luchador1.Velocidad.X);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 20);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.Cero, estado.Luchador1.Velocidad.X);
            Assert.IsTrue(estado.Luchador1.Posicion.X > alEmpezar + Fijo.DesdeEntero(2));
        }

        [Test]
        public void UnAtaqueSinDashNoSeDesliza()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Fuerte, 10);

            Assert.AreEqual(Fijo.DesdeEntero(-5), estado.Luchador1.Posicion.X);
        }

        [Test]
        public void ElDashSeCancelaConUnSalto()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Arriba, 4);

            Assert.AreEqual(AccionLuchador.PreSalto, estado.Luchador1.Accion);
        }

        [Test]
        public void UnSegundoBotonEnLosPrimerosFramesDeUnAtaqueLoConvierteEnDash()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Medio, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Medio, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Medio | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Ninguno, estado.Luchador1.Ataque);
            Assert.AreEqual(0, estado.Luchador1.Barra, "Un ataque cancelado en dash no da barra");
        }

        [Test]
        public void PasadaLaVentanaDelPlinkElSegundoBotonNoDaDash()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Medio, 3);
            estado = SimularJugador1(estado, BotonesEntrada.Medio | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Medio, estado.Luchador1.Ataque);
        }

        [Test]
        public void ElPlinkDashReiniciaElDashAntesQueElWavedash()
        {
            var estado = SimularJugador1(Inicial(-5, 5), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Medio, 1);

            Assert.AreEqual(AccionLuchador.Atacando, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, BotonesEntrada.Medio | BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.Dash, estado.Luchador1.Accion);
            Assert.AreEqual(Fijo.DesdeFraccion(15, 32), estado.Luchador1.Velocidad.X);
            Assert.AreEqual(4, estado.Frame, "El segundo dash sale en el frame 4; con wavedash, como pronto en el 6");
        }

        [Test]
        public void UnGolpeAlcanzaAlLuchadorQueHaceDashPorqueNoPuedeBloquear()
        {
            var estado = Simular(Inicial(0, 1), BotonesEntrada.Debil, BotonesEntrada.Ninguno, 3);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Izquierda | DosGolpes, 1);
            estado = Simular(estado, BotonesEntrada.Ninguno, BotonesEntrada.Derecha, 1);

            Assert.AreEqual(AccionLuchador.Hitstun, estado.Luchador2.Accion);
        }

        [Test]
        public void DosGolpesEnUnSaltoHacenUnDashAereoSinCaer()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 3), DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.DashAereo, estado.Luchador1.Accion);
            Assert.AreEqual(new VectorFijo(Fijo.DesdeFraccion(-9, 2), Fijo.DesdeEntero(3)), estado.Luchador1.Posicion);
            Assert.IsTrue(estado.Luchador1.DashAereoUsado);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 7);

            Assert.AreEqual(AccionLuchador.DashAereo, estado.Luchador1.Accion);
            Assert.AreEqual(new VectorFijo(Fijo.DesdeEntero(-1), Fijo.DesdeEntero(3)), estado.Luchador1.Posicion);

            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 3);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.IsTrue(estado.Luchador1.Posicion.Y < Fijo.DesdeEntero(3));
        }

        [Test]
        public void ElDashAereoHaciaAtrasSeAlejaDelRival()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 3), BotonesEntrada.Izquierda | DosGolpes, 1);

            Assert.AreEqual(Fijo.DesdeFraccion(-11, 2), estado.Luchador1.Posicion.X);
        }

        [Test]
        public void SoloHayUnDashAereoPorSalto()
        {
            var estado = DashHasta(ConJugador1EnElAire(Inicial(-5, 5), 5), DosGolpes, 10);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);

            estado = SimularJugador1(estado, DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);
        }

        [Test]
        public void AlAterrizarSeRecuperaElDashAereo()
        {
            var estado = DashHasta(ConJugador1EnElAire(Inicial(-5, 5), 1), DosGolpes, 40);

            Assert.AreEqual(AccionLuchador.Quieto, estado.Luchador1.Accion);
            Assert.IsFalse(estado.Luchador1.DashAereoUsado);
        }

        [Test]
        public void MuyCercaDelSueloNoHayDashAereo()
        {
            var estado = SimularJugador1(Inicial(-5, 5), BotonesEntrada.Arriba, 5);

            Assert.AreEqual(AccionLuchador.EnAire, estado.Luchador1.Accion);
            Assert.IsTrue(estado.Luchador1.Posicion.Y < Fijo.Uno);

            estado = SimularJugador1(estado, DosGolpes, 1);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);
        }

        [Test]
        public void ElDashAereoSeCancelaConUnAtaqueQueConservaElImpulso()
        {
            var estado = SimularJugador1(ConJugador1EnElAire(Inicial(-5, 5), 3), DosGolpes, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Ninguno, 1);
            estado = SimularJugador1(estado, BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.DashAereo, estado.Luchador1.Accion, "Antes del frame 3 del dash aéreo no se cancela");

            estado = SimularJugador1(estado, BotonesEntrada.Fuerte, 1);

            Assert.AreEqual(AccionLuchador.AtacandoEnAire, estado.Luchador1.Accion);
            Assert.AreEqual(TipoAtaque.Fuerte, estado.Luchador1.Ataque);
            Assert.AreEqual(Fijo.Medio, estado.Luchador1.Velocidad.X);
        }
    }
}
