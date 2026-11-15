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
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(10, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(8, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(75, 100), Fijo.DesdeFraccion(115, 100)),
                Fijo.DesdeFraccion(45, 100),
                Fijo.DesdeFraccion(2, 10)));

        /// <summary>
        /// Golpe medio: golpea en el frame 7, dura 23 frames, +1 al golpear y -3 al bloquear.
        /// Más alcance que el débil; es el enlace natural de un combo.
        /// </summary>
        public static readonly DatosMovimiento GolpeMedioCajaGris = new DatosMovimiento(
            arranque: 6,
            activos: 3,
            recuperacion: 14,
            hitstun: 17,
            blockstun: 13,
            hitstop: 10,
            danio: 600,
            retrocesoAlGolpear: Fijo.DesdeFraccion(12, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(10, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(90, 100), Fijo.DesdeFraccion(110, 100)),
                Fijo.DesdeFraccion(60, 100),
                Fijo.DesdeFraccion(22, 100)));

        /// <summary>
        /// Golpe fuerte: golpea en el frame 10, dura 33 frames, -2 al golpear y -7 al bloquear.
        /// El de más alcance y más hitstop; castigable si lo bloquean.
        /// </summary>
        public static readonly DatosMovimiento GolpeFuerteCajaGris = new DatosMovimiento(
            arranque: 9,
            activos: 4,
            recuperacion: 20,
            hitstun: 21,
            blockstun: 16,
            hitstop: 12,
            danio: 900,
            retrocesoAlGolpear: Fijo.DesdeFraccion(15, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(12, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(105, 100), Fijo.DesdeFraccion(100, 100)),
                Fijo.DesdeFraccion(80, 100),
                Fijo.DesdeFraccion(30, 100)));

        public static readonly MedidasCajaDanio CajaDanioCajaGris = new MedidasCajaDanio(
            medioAncho: Fijo.DesdeFraccion(4, 10),
            medioAltoDePie: Fijo.DesdeFraccion(9, 10),
            medioAltoAgachado: Fijo.DesdeFraccion(5, 10),
            medioAltoAire: Fijo.DesdeFraccion(5, 10));

        /// <summary>Cada golpe del combo hace 10 puntos porcentuales menos de daño, con un mínimo del 30%.</summary>
        public static readonly ReglasDanio DanioCajasGrises = new ReglasDanio(reduccionPorGolpe: 10, porcentajeMinimo: 30);

        /// <summary>99 segundos de reloj.</summary>
        public const int FramesPorRonda = 99 * 60;

        public static readonly ParametrosLuchador LuchadorCajaGris = new ParametrosLuchador(
            vidaMaxima: 10000,
            velocidadAdelante: Fijo.DesdeFraccion(6, 100),
            velocidadAtras: Fijo.DesdeFraccion(45, 1000),
            medioAnchoEmpuje: Fijo.DesdeFraccion(4, 10),
            medioAltoEmpuje: Fijo.DesdeFraccion(9, 10),
            medioAnchoEmpujeAire: Fijo.DesdeFraccion(3, 10),
            medioAltoEmpujeAire: Fijo.DesdeFraccion(5, 10),
            salto: SaltoCajaGris,
            medidasDanio: CajaDanioCajaGris,
            golpeDebil: GolpeDebilCajaGris,
            golpeMedio: GolpeMedioCajaGris,
            golpeFuerte: GolpeFuerteCajaGris);

        public static readonly ConfiguracionCombate ConfiguracionCajasGrises = new ConfiguracionCombate(
            LuchadorCajaGris,
            LuchadorCajaGris,
            Fijo.DesdeEntero(-7),
            Fijo.DesdeEntero(7),
            friccionRetroceso: Fijo.DesdeFraccion(1, 100),
            framesBuferEntradas: 5,
            danio: DanioCajasGrises,
            framesPorRonda: FramesPorRonda);

        public static readonly EstadoPartida EstadoInicial = EstadoPartida.Crear(
            VectorFijo.DesdeEnteros(-2, 0),
            VectorFijo.DesdeEnteros(2, 0),
            ConfiguracionCajasGrises);
    }
}
