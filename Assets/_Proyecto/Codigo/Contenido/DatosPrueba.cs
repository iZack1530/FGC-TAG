using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Contenido
{
    /// <summary>
    /// Valores del prototipo con cajas grises (fase 1). Unidades en metros y velocidades por frame
    /// de 60 Hz: 0.06 m/frame equivale a 3.6 m/s. El salto sube unos 2.3 m y dura 43 frames,
    /// en el rango de Guilty Gear y Dragon Ball FighterZ; la caja de tierra mide 1.8 m, así que en
    /// lo alto del salto se puede pasar por encima del rival.
    /// </summary>
    public static class DatosPrueba
    {
        public static readonly ParametrosSalto SaltoCajaGris = new ParametrosSalto(
            framesPreSalto: 4,
            framesAterrizaje: 3,
            velocidadVertical: Fijo.DesdeFraccion(21, 100),
            gravedad: Fijo.DesdeFraccion(1, 100),
            velocidadAdelante: Fijo.DesdeFraccion(7, 100),
            velocidadAtras: Fijo.DesdeFraccion(6, 100));

        public static readonly ParametrosLuchador LuchadorCajaGris = new ParametrosLuchador(
            velocidadAdelante: Fijo.DesdeFraccion(6, 100),
            velocidadAtras: Fijo.DesdeFraccion(45, 1000),
            medioAnchoEmpuje: Fijo.DesdeFraccion(4, 10),
            medioAltoEmpuje: Fijo.DesdeFraccion(9, 10),
            medioAnchoEmpujeAire: Fijo.DesdeFraccion(3, 10),
            medioAltoEmpujeAire: Fijo.DesdeFraccion(5, 10),
            salto: SaltoCajaGris);

        public static readonly ConfiguracionCombate ConfiguracionCajasGrises = new ConfiguracionCombate(
            LuchadorCajaGris,
            LuchadorCajaGris,
            Fijo.DesdeEntero(-7),
            Fijo.DesdeEntero(7));

        public static readonly EstadoPartida EstadoInicial = EstadoPartida.Crear(
            VectorFijo.DesdeEnteros(-2, 0),
            VectorFijo.DesdeEnteros(2, 0));
    }
}
