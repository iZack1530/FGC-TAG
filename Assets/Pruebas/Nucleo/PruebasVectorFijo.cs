using FgcTag.Nucleo.Matematica;
using NUnit.Framework;

namespace FgcTag.Pruebas.Nucleo
{
    public class PruebasVectorFijo
    {
        [Test]
        public void SumaYRestaOperanPorComponente()
        {
            var a = VectorFijo.DesdeEnteros(1, 2);
            var b = VectorFijo.DesdeEnteros(3, -5);

            Assert.AreEqual(VectorFijo.DesdeEnteros(4, -3), a + b);
            Assert.AreEqual(VectorFijo.DesdeEnteros(-2, 7), a - b);
            Assert.AreEqual(VectorFijo.DesdeEnteros(-1, -2), -a);
        }

        [Test]
        public void MultiplicacionPorEscalarEscalaAmbosEjes()
        {
            var resultado = VectorFijo.DesdeEnteros(2, -4) * Fijo.Medio;

            Assert.AreEqual(VectorFijo.DesdeEnteros(1, -2), resultado);
        }

        [Test]
        public void ReflejoHorizontalSoloInvierteX()
        {
            Assert.AreEqual(VectorFijo.DesdeEnteros(-3, 4), VectorFijo.DesdeEnteros(3, 4).ReflejadoHorizontal());
        }

        [Test]
        public void ConXYConYReemplazanUnSoloEje()
        {
            var vector = VectorFijo.DesdeEnteros(1, 1);

            Assert.AreEqual(VectorFijo.DesdeEnteros(9, 1), vector.ConX(Fijo.DesdeEntero(9)));
            Assert.AreEqual(VectorFijo.DesdeEnteros(1, 9), vector.ConY(Fijo.DesdeEntero(9)));
        }
    }
}
