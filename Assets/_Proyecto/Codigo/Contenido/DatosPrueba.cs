using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Contenido
{
    /// <summary>
    /// Valores del prototipo con cajas grises (fase 1). Unidades en metros; velocidades por frame
    /// de 60 Hz: 0.06 m/frame equivale a 3.6 m/s.
    /// </summary>
    public static class DatosPrueba
    {
        public static readonly ParametrosLuchador LuchadorCajaGris = new ParametrosLuchador(
            velocidadAdelante: Fijo.DesdeFraccion(6, 100),
            velocidadAtras: Fijo.DesdeFraccion(45, 1000),
            medioAnchoEmpuje: Fijo.DesdeFraccion(4, 10),
            medioAltoEmpuje: Fijo.DesdeFraccion(9, 10));

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
