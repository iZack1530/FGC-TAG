using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Luchador y escenario de referencia para las pruebas, con números redondos para que los
    /// resultados esperados se puedan calcular a mano.
    /// </summary>
    public static class EscenarioPrueba
    {
        /// <summary>
        /// Dura 16 frames y golpea en el frame 5. Ventaja: +3 al golpear y -1 al bloquear.
        /// Alcanza a un rival cuyo centro esté a menos de 2 unidades.
        /// </summary>
        public static readonly DatosMovimiento GolpeDebil = new DatosMovimiento(
            arranque: 4,
            activos: 3,
            recuperacion: 9,
            hitstun: 14,
            blockstun: 10,
            hitstop: 8,
            danio: 100,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(VectorFijo.DesdeEnteros(1, 1), Fijo.Medio, Fijo.DesdeFraccion(1, 4)));

        /// <summary>Dura 23 frames y golpea en el frame 7. Ventaja: +1 al golpear y -3 al bloquear. Alcance: 2.5 unidades.</summary>
        public static readonly DatosMovimiento GolpeMedio = new DatosMovimiento(
            arranque: 6,
            activos: 3,
            recuperacion: 14,
            hitstun: 17,
            blockstun: 13,
            hitstop: 10,
            danio: 200,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(VectorFijo.DesdeEnteros(1, 1), Fijo.Uno, Fijo.DesdeFraccion(1, 4)));

        /// <summary>Dura 33 frames y golpea en el frame 10. Ventaja: -2 al golpear y -7 al bloquear. Alcance: 3.5 unidades.</summary>
        public static readonly DatosMovimiento GolpeFuerte = new DatosMovimiento(
            arranque: 9,
            activos: 4,
            recuperacion: 20,
            hitstun: 21,
            blockstun: 16,
            hitstop: 12,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.DesdeFraccion(3, 2), Fijo.Uno), Fijo.DesdeFraccion(3, 2), Fijo.DesdeFraccion(1, 4)));

        /// <summary>Un ataque pulsado se recuerda 5 frames, contando el de la pulsación.</summary>
        public const int FramesBufer = 5;

        public const int VidaMaxima = 1000;

        /// <summary>Cada golpe del combo hace 10 puntos porcentuales menos, con un mínimo del 30%.</summary>
        public static readonly ReglasDanio Danio = new ReglasDanio(reduccionPorGolpe: 10, porcentajeMinimo: 30);

        /// <summary>99 segundos.</summary>
        public const int FramesPorRonda = 5940;

        public static readonly ParametrosLuchador Parametros = new ParametrosLuchador(
            vidaMaxima: VidaMaxima,
            velocidadAdelante: Fijo.DesdeFraccion(1, 4),
            velocidadAtras: Fijo.DesdeFraccion(1, 8),
            medioAnchoEmpuje: Fijo.Medio,
            medioAltoEmpuje: Fijo.Uno,
            medioAnchoEmpujeAire: Fijo.DesdeFraccion(1, 4),
            medioAltoEmpujeAire: Fijo.Medio,
            salto: new ParametrosSalto(
                framesPreSalto: 4,
                framesAterrizaje: 3,
                velocidadVertical: Fijo.DesdeFraccion(2, 5),
                gravedad: Fijo.DesdeFraccion(1, 50),
                velocidadAdelante: Fijo.DesdeFraccion(1, 10),
                velocidadAtras: Fijo.DesdeFraccion(1, 10)),
            medidasDanio: new MedidasCajaDanio(
                medioAncho: Fijo.Medio,
                medioAltoDePie: Fijo.Uno,
                medioAltoAgachado: Fijo.Medio,
                medioAltoAire: Fijo.Medio),
            golpeDebil: GolpeDebil,
            golpeMedio: GolpeMedio,
            golpeFuerte: GolpeFuerte);

        public static readonly ConfiguracionCombate Configuracion = ConfiguracionConReloj(FramesPorRonda);

        public static ConfiguracionCombate ConfiguracionConReloj(int framesPorRonda) => new ConfiguracionCombate(
            Parametros,
            Parametros,
            Fijo.DesdeEntero(-10),
            Fijo.DesdeEntero(10),
            friccionRetroceso: Fijo.DesdeFraccion(1, 64),
            framesBuferEntradas: FramesBufer,
            danio: Danio,
            framesPorRonda: framesPorRonda);

        public static EstadoPartida Inicial(int x1, int x2) =>
            EstadoPartida.Crear(VectorFijo.DesdeEnteros(x1, 0), VectorFijo.DesdeEnteros(x2, 0), Configuracion);

        /// <summary>Copia del estado con la vida de cada luchador cambiada.</summary>
        public static EstadoPartida ConVidas(EstadoPartida estado, int vida1, int vida2) =>
            new EstadoPartida(estado.Frame, estado.Luchador1.ConVida(vida1), estado.Luchador2.ConVida(vida2), estado.FramesRestantes, estado.Resultado);

        public static EstadoPartida Simular(EstadoPartida estado, BotonesEntrada jugador1, BotonesEntrada jugador2, int frames)
        {
            var entradas = new EntradasFrame(jugador1, jugador2);

            for (var i = 0; i < frames; i++)
            {
                estado = SimuladorCombate.Avanzar(estado, entradas, Configuracion);
            }

            return estado;
        }

        public static EstadoPartida SimularJugador1(EstadoPartida estado, BotonesEntrada jugador1, int frames) =>
            Simular(estado, jugador1, BotonesEntrada.Ninguno, frames);
    }
}
