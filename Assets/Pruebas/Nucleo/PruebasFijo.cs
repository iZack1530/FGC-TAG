using System;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;

namespace FgcTag.Pruebas.Nucleo
{
    public class PruebasFijo
    {
        [Test]
        public void DesdeEnteroGuardaElValorDesplazado()
        {
            Assert.AreEqual(3L << Fijo.BitsFraccion, Fijo.DesdeEntero(3).Crudo);
        }

        [Test]
        public void DesdeFraccionEsExacta()
        {
            Assert.AreEqual(49152L, Fijo.DesdeFraccion(3, 4).Crudo);
            Assert.AreEqual(-49152L, Fijo.DesdeFraccion(-3, 4).Crudo);
        }

        [Test]
        public void SumaYRestaOperanSobreElValorCrudo()
        {
            var resultado = Fijo.DesdeEntero(5) + Fijo.Medio - Fijo.DesdeEntero(2);

            Assert.AreEqual(Fijo.DesdeFraccion(7, 2), resultado);
        }

        [Test]
        public void MultiplicacionConservaLaEscala()
        {
            Assert.AreEqual(Fijo.DesdeEntero(10), Fijo.DesdeFraccion(5, 2) * Fijo.DesdeEntero(4));
            Assert.AreEqual(Fijo.DesdeEntero(-6), Fijo.DesdeEntero(-2) * Fijo.DesdeEntero(3));
            Assert.AreEqual(Fijo.DesdeEntero(9), Fijo.DesdeEntero(3) * 3);
        }

        [Test]
        public void DivisionTruncaHaciaCero()
        {
            Assert.AreEqual(21845L, (Fijo.Uno / Fijo.DesdeEntero(3)).Crudo);
            Assert.AreEqual(-21845L, (Fijo.MenosUno / Fijo.DesdeEntero(3)).Crudo);
            Assert.AreEqual(Fijo.DesdeFraccion(1, 2), Fijo.Uno / 2);
        }

        [Test]
        public void DivisionEntreCeroLanzaExcepcion()
        {
            Assert.Throws<DivideByZeroException>(() => _ = Fijo.Uno / Fijo.Cero);
            Assert.Throws<DivideByZeroException>(() => _ = Fijo.Uno / 0);
            Assert.Throws<DivideByZeroException>(() => Fijo.DesdeFraccion(1, 0));
        }

        [Test]
        public void PisoRedondeaHaciaMenosInfinito()
        {
            Assert.AreEqual(1, Fijo.DesdeFraccion(3, 2).Piso());
            Assert.AreEqual(-2, Fijo.DesdeFraccion(-3, 2).Piso());
        }

        [Test]
        public void RedondearLlevaLosEmpatesHaciaArriba()
        {
            Assert.AreEqual(3, Fijo.DesdeFraccion(5, 2).Redondear());
            Assert.AreEqual(-2, Fijo.DesdeFraccion(-5, 2).Redondear());
            Assert.AreEqual(2, Fijo.DesdeFraccion(9, 4).Redondear());
        }

        [Test]
        public void FuncionesAuxiliaresDevuelvenElValorEsperado()
        {
            Assert.AreEqual(Fijo.DesdeEntero(4), Fijo.Abs(Fijo.DesdeEntero(-4)));
            Assert.AreEqual(Fijo.Uno, Fijo.Min(Fijo.Uno, Fijo.DesdeEntero(2)));
            Assert.AreEqual(Fijo.DesdeEntero(2), Fijo.Max(Fijo.Uno, Fijo.DesdeEntero(2)));
            Assert.AreEqual(Fijo.Uno, Fijo.Limitar(Fijo.DesdeEntero(9), Fijo.Cero, Fijo.Uno));
            Assert.AreEqual(-1, Fijo.Signo(Fijo.MenosUno));
            Assert.AreEqual(0, Fijo.Signo(Fijo.Cero));
        }

        [Test]
        public void LimitarRechazaUnRangoInvertido()
        {
            Assert.Throws<ArgumentException>(() => Fijo.Limitar(Fijo.Cero, Fijo.Uno, Fijo.Cero));
        }

        [Test]
        public void ComparacionesUsanElValorCrudo()
        {
            Assert.IsTrue(Fijo.Medio < Fijo.Uno);
            Assert.IsTrue(Fijo.Uno >= Fijo.DesdeEntero(1));
            Assert.IsTrue(Fijo.MenosUno != Fijo.Uno);
            Assert.Less(Fijo.Cero.CompareTo(Fijo.Uno), 0);
        }

        [Test]
        public void TextoUsaPuntoDecimalSinImportarLaCultura()
        {
            Assert.AreEqual("1.5", Fijo.DesdeFraccion(3, 2).ToString());
        }

        /// <summary>
        /// Valor dorado: si esta prueba falla, la aritmética cambió y todas las repeticiones
        /// grabadas dejarán de reproducirse igual.
        /// </summary>
        [Test]
        public void SecuenciaDeOperacionesProduceSiempreElMismoResultado()
        {
            var x = Fijo.DesdeFraccion(7, 3);
            var y = x * x - Fijo.DesdeFraccion(5, 2);
            var z = y / Fijo.DesdeFraccion(-3, 4);
            var w = z * 13;

            Assert.AreEqual(152917L, x.Crudo);
            Assert.AreEqual(192965L, y.Crudo);
            Assert.AreEqual(-257286L, z.Crudo);
            Assert.AreEqual(-3344718L, w.Crudo);
        }
    }
}
