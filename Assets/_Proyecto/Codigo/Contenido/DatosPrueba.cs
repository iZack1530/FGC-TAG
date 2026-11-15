using FgcTag.Nucleo.Colisiones;
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

        /// <summary>
        /// Golpe débil de pie: golpea en el frame 5, dura 16 frames, +3 al golpear y -1 al
        /// bloquear. El retroceso aleja al rival unos 0.55 m al golpear y 0.36 m al bloquear.
        /// </summary>
        public static readonly DatosMovimiento GolpeDebilCajaGris = new DatosMovimiento(
            arranque: 4,
            activos: 3,
            recuperacion: 9,
            hitstun: 14,
            blockstun: 10,
            hitstop: 8,
            retrocesoAlGolpear: Fijo.DesdeFraccion(10, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(8, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(75, 100), Fijo.DesdeFraccion(115, 100)),
                Fijo.DesdeFraccion(45, 100),
                Fijo.DesdeFraccion(2, 10)));

        public static readonly MedidasCajaDanio CajaDanioCajaGris = new MedidasCajaDanio(
            medioAncho: Fijo.DesdeFraccion(4, 10),
            medioAltoDePie: Fijo.DesdeFraccion(9, 10),
            medioAltoAgachado: Fijo.DesdeFraccion(5, 10),
            medioAltoAire: Fijo.DesdeFraccion(5, 10));

        public static readonly ParametrosLuchador LuchadorCajaGris = new ParametrosLuchador(
            velocidadAdelante: Fijo.DesdeFraccion(6, 100),
            velocidadAtras: Fijo.DesdeFraccion(45, 1000),
            medioAnchoEmpuje: Fijo.DesdeFraccion(4, 10),
            medioAltoEmpuje: Fijo.DesdeFraccion(9, 10),
            medioAnchoEmpujeAire: Fijo.DesdeFraccion(3, 10),
            medioAltoEmpujeAire: Fijo.DesdeFraccion(5, 10),
            salto: SaltoCajaGris,
            medidasDanio: CajaDanioCajaGris,
            golpeDebil: GolpeDebilCajaGris);

        public static readonly ConfiguracionCombate ConfiguracionCajasGrises = new ConfiguracionCombate(
            LuchadorCajaGris,
            LuchadorCajaGris,
            Fijo.DesdeEntero(-7),
            Fijo.DesdeEntero(7),
            friccionRetroceso: Fijo.DesdeFraccion(1, 100));

        public static readonly EstadoPartida EstadoInicial = EstadoPartida.Crear(
            VectorFijo.DesdeEnteros(-2, 0),
            VectorFijo.DesdeEnteros(2, 0));
    }
}
