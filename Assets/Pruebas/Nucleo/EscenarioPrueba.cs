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
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(VectorFijo.DesdeEnteros(1, 1), Fijo.Medio, Fijo.DesdeFraccion(1, 4)));

        public static readonly ParametrosLuchador Parametros = new ParametrosLuchador(
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
            golpeDebil: GolpeDebil);

        public static readonly ConfiguracionCombate Configuracion = new ConfiguracionCombate(
            Parametros,
            Parametros,
            Fijo.DesdeEntero(-10),
            Fijo.DesdeEntero(10),
            friccionRetroceso: Fijo.DesdeFraccion(1, 64));

        public static EstadoPartida Inicial(int x1, int x2) =>
            EstadoPartida.Crear(VectorFijo.DesdeEnteros(x1, 0), VectorFijo.DesdeEnteros(x2, 0));

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
