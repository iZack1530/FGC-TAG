using System;
using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Matematica;
using NUnit.Framework;

namespace FgcTag.Pruebas.Nucleo
{
    public class PruebasCajaColision
    {
        private static CajaColision Caja(int x, int y, int medioAncho, int medioAlto) =>
            new CajaColision(VectorFijo.DesdeEnteros(x, y), Fijo.DesdeEntero(medioAncho), Fijo.DesdeEntero(medioAlto));

        [Test]
        public void CajasSolapadasIntersectan()
        {
            Assert.IsTrue(Caja(0, 0, 2, 2).Intersecta(Caja(3, 1, 2, 2)));
        }

        [Test]
        public void CajasQueSoloSeTocanNoIntersectan()
        {
            Assert.IsFalse(Caja(0, 0, 2, 2).Intersecta(Caja(4, 0, 2, 2)));
            Assert.IsFalse(Caja(0, 0, 2, 2).Intersecta(Caja(0, 4, 2, 2)));
        }

        [Test]
        public void CajasSeparadasNoIntersectan()
        {
            Assert.IsFalse(Caja(0, 0, 1, 1).Intersecta(Caja(10, 0, 1, 1)));
        }

        [Test]
        public void LaInterseccionEsSimetrica()
        {
            var a = Caja(0, 0, 3, 1);
            var b = Caja(2, 1, 1, 1);

            Assert.AreEqual(a.Intersecta(b), b.Intersecta(a));
        }

        [Test]
        public void OrientarALaIzquierdaReflejaElCentro()
        {
            var hitbox = Caja(5, 3, 2, 1);

            Assert.AreEqual(Caja(-5, 3, 2, 1), hitbox.Orientada(false));
            Assert.AreEqual(hitbox, hitbox.Orientada(true));
        }

        [Test]
        public void DesplazarMueveElCentroSinCambiarElTamano()
        {
            var resultado = Caja(1, 1, 2, 3).Desplazada(VectorFijo.DesdeEnteros(10, -1));

            Assert.AreEqual(Caja(11, 0, 2, 3), resultado);
        }

        [Test]
        public void TamanoNegativoLanzaExcepcion()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Caja(0, 0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Caja(0, 0, 1, -1));
        }
    }
}
