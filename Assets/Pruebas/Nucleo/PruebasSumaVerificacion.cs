using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;
using NUnit.Framework;

namespace FgcTag.Pruebas.Nucleo
{
    public class PruebasSumaVerificacion
    {
        [Test]
        public void MismosDatosProducenLaMismaSuma()
        {
            var a = SumaVerificacion.Inicial.Agregar(VectorFijo.DesdeEnteros(3, 4)).Agregar(true);
            var b = SumaVerificacion.Inicial.Agregar(VectorFijo.DesdeEnteros(3, 4)).Agregar(true);

            Assert.AreEqual(a.Valor, b.Valor);
        }

        [Test]
        public void ElOrdenDeLosDatosCambiaLaSuma()
        {
            var a = SumaVerificacion.Inicial.Agregar(1).Agregar(2);
            var b = SumaVerificacion.Inicial.Agregar(2).Agregar(1);

            Assert.AreNotEqual(a.Valor, b.Valor);
        }

        /// <summary>Valores dorados de FNV-1a: deben coincidir en cualquier máquina y ejecución.</summary>
        [Test]
        public void ValoresConocidosSonEstables()
        {
            Assert.AreEqual("A8C7F832281A39C5", SumaVerificacion.Inicial.Agregar(0L).ToString());
            Assert.AreEqual("7717980363C8E066", SumaVerificacion.Inicial.Agregar(1).Agregar(2).ToString());
        }

        [Test]
        public void UnFijoSumaIgualQueSuValorCrudo()
        {
            var fijo = Fijo.DesdeFraccion(7, 3);

            Assert.AreEqual(
                SumaVerificacion.Inicial.Agregar(fijo.Crudo).Valor,
                SumaVerificacion.Inicial.Agregar(fijo).Valor);
        }
    }
}
