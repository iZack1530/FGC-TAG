using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Pruebas.Nucleo
{
    /// <summary>
    /// Luchador y escenario de referencia para las pruebas, con números redondos para que los
    /// resultados esperados se puedan calcular a mano.
    /// </summary>
    public static class EscenarioPrueba
    {
        /// <summary>Caja de golpe de los ataques aéreos: de 0 a 2 unidades por delante y de -1/2 a 3/2 de alto respecto a los pies.</summary>
        private static readonly CajaColision CajaGolpeAerea = new CajaColision(new VectorFijo(Fijo.Uno, Fijo.Medio), Fijo.Uno, Fijo.Uno);

        /// <summary>
        /// Dura 16 frames y golpea en el frame 5. Ventaja: +3 al golpear y -1 al bloquear.
        /// Alcanza a un rival cuyo centro esté a menos de 2 unidades.
        /// </summary>
        public static readonly DatosMovimiento GolpeDebil = new DatosMovimiento(
            arranque: 4,
            activos: 3,
            recuperacion: 9,
            hitstun: 14,
            hitstunAereo: 60,
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
            hitstunAereo: 60,
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
            hitstunAereo: 60,
            blockstun: 16,
            hitstop: 12,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.DesdeFraccion(3, 2), Fijo.Uno), Fijo.DesdeFraccion(3, 2), Fijo.DesdeFraccion(1, 4)));

        /// <summary>
        /// Golpe bajo: mismos tiempos que el débil de pie, pero solo se bloquea agachado. Es el
        /// único golpe con aturdimiento aéreo corto (4 frames): sirve para probar la recuperación
        /// en el aire; con los demás (60), el rival siempre llega antes al suelo.
        /// </summary>
        public static readonly DatosMovimiento GolpeDebilAgachado = new DatosMovimiento(
            arranque: 4,
            activos: 3,
            recuperacion: 9,
            hitstun: 14,
            hitstunAereo: 4,
            blockstun: 10,
            hitstop: 8,
            danio: 100,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.Uno, Fijo.DesdeFraccion(1, 4)), Fijo.Medio, Fijo.DesdeFraccion(1, 4)),
            altura: AlturaGolpe.Bajo);

        /// <summary>Golpe bajo con los tiempos del medio de pie.</summary>
        public static readonly DatosMovimiento GolpeMedioAgachado = new DatosMovimiento(
            arranque: 6,
            activos: 3,
            recuperacion: 14,
            hitstun: 17,
            hitstunAereo: 60,
            blockstun: 13,
            hitstop: 10,
            danio: 200,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.Uno, Fijo.DesdeFraccion(1, 4)), Fijo.Uno, Fijo.DesdeFraccion(1, 4)),
            altura: AlturaGolpe.Bajo);

        /// <summary>Golpe bajo con los tiempos del fuerte de pie.</summary>
        public static readonly DatosMovimiento GolpeFuerteAgachado = new DatosMovimiento(
            arranque: 9,
            activos: 4,
            recuperacion: 20,
            hitstun: 21,
            hitstunAereo: 60,
            blockstun: 16,
            hitstop: 12,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 8),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.DesdeFraccion(3, 2), Fijo.DesdeFraccion(1, 4)), Fijo.DesdeFraccion(3, 2), Fijo.DesdeFraccion(1, 4)),
            altura: AlturaGolpe.Bajo);

        /// <summary>Un ataque pulsado se recuerda 5 frames, contando el de la pulsación.</summary>
        public const int FramesBufer = 5;

        /// <summary>
        /// Lanzador: golpea en el frame 9 y dura 32 frames. Lanza al rival con velocidad vertical
        /// 1/2 y, si golpea sin ser bloqueado, se cancela con un salto.
        /// </summary>
        public static readonly DatosMovimiento Lanzador = new DatosMovimiento(
            arranque: 8,
            activos: 4,
            recuperacion: 20,
            hitstun: 20,
            hitstunAereo: 60,
            blockstun: 15,
            hitstop: 12,
            danio: 250,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 64),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: new CajaColision(new VectorFijo(Fijo.DesdeFraccion(3, 2), Fijo.DesdeFraccion(3, 2)), Fijo.DesdeFraccion(3, 2), Fijo.Uno),
            velocidadLanzamiento: Fijo.Medio,
            cancelableConSalto: true);

        /// <summary>Dura 14 frames y golpea en el frame 5.</summary>
        public static readonly DatosMovimiento GolpeDebilAereo = new DatosMovimiento(
            arranque: 4,
            activos: 4,
            recuperacion: 6,
            hitstun: 14,
            hitstunAereo: 60,
            blockstun: 10,
            hitstop: 8,
            danio: 100,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 16),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: CajaGolpeAerea,
            altura: AlturaGolpe.Alto);

        /// <summary>Dura 20 frames y golpea en el frame 7.</summary>
        public static readonly DatosMovimiento GolpeMedioAereo = new DatosMovimiento(
            arranque: 6,
            activos: 4,
            recuperacion: 10,
            hitstun: 17,
            hitstunAereo: 60,
            blockstun: 13,
            hitstop: 10,
            danio: 200,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 16),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: CajaGolpeAerea,
            altura: AlturaGolpe.Alto);

        /// <summary>Dura 26 frames y golpea en el frame 9.</summary>
        public static readonly DatosMovimiento GolpeFuerteAereo = new DatosMovimiento(
            arranque: 8,
            activos: 4,
            recuperacion: 14,
            hitstun: 21,
            hitstunAereo: 60,
            blockstun: 16,
            hitstop: 12,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(1, 16),
            retrocesoAlBloquear: Fijo.DesdeFraccion(1, 16),
            cajaGolpe: CajaGolpeAerea,
            altura: AlturaGolpe.Alto);

        /// <summary>
        /// Un rival golpeado en el aire sube con velocidad 1/8; su gravedad es 1/32 más 1/128 por
        /// cada golpe del combo; al caer queda 30 frames derribado.
        /// </summary>
        public static readonly ReglasJuggle Juggle = new ReglasJuggle(
            impulsoAlGolpear: Fijo.DesdeFraccion(1, 8),
            gravedad: Fijo.DesdeFraccion(1, 32),
            gravedadExtraPorGolpe: Fijo.DesdeFraccion(1, 128),
            framesDerribado: 30,
            reduccionAturdimientoPorGolpe: 2,
            aturdimientoMinimo: 4,
            velocidadRecuperacion: Fijo.DesdeFraccion(1, 8),
            impulsoRecuperacion: Fijo.DesdeFraccion(1, 4),
            framesRecuperacion: 16,
            framesRecuperacionInvencible: 8);

        public const int VidaMaxima = 1000;

        /// <summary>Cada golpe del combo hace 10 puntos porcentuales menos, con un mínimo del 30%. Bloqueado pasa el 10%.</summary>
        public static readonly ReglasDanio Danio = new ReglasDanio(reduccionPorGolpe: 10, porcentajeMinimo: 30, porcentajeAlBloquear: 10);

        /// <summary>
        /// Agarre: activo en los frames 3 y 4, alcanza hasta 1.5 unidades y falla en 25 frames. Si
        /// atrapa, sujeta 10 frames, hace 250 de daño y lanza al rival; quien agarra se recupera 15 frames después.
        /// </summary>
        public static readonly DatosAgarre Agarre = new DatosAgarre(
            arranque: 3,
            activos: 2,
            recuperacion: 20,
            cajaAgarre: new CajaColision(VectorFijo.DesdeEnteros(1, 1), Fijo.Medio, Fijo.Medio),
            framesSujecion: 10,
            recuperacionTrasLanzar: 15,
            danio: 250,
            velocidadLanzamiento: new VectorFijo(Fijo.DesdeFraccion(1, 8), Fijo.DesdeFraccion(1, 4)),
            aturdimientoLanzado: 60);

        /// <summary>
        /// Una dirección más el golpe fuerte es un agarre a 1.5 unidades o menos; 8 frames de
        /// protección tras un aturdimiento. El rival sujeto tiene 9 frames para escapar; un agarre
        /// roto separa a los dos durante 12 frames, empezando a 1/8 de unidad por frame.
        /// </summary>
        public static readonly ReglasAgarre Agarres = new ReglasAgarre(
            framesProteccion: 8,
            distanciaActivacion: Fijo.DesdeFraccion(3, 2),
            framesVentanaEscape: 9,
            framesAgarreRoto: 12,
            retrocesoAlEscapar: Fijo.DesdeFraccion(1, 8));

        /// <summary>
        /// Dash en tierra: 12 frames, empieza a 1/2 por frame (3/8 hacia atrás) y pierde 1/32 por
        /// frame; se cancela con un ataque desde el frame 2 y agachándose o saltando desde el 4.
        /// Plink: 2 frames. Dash aéreo: 8 frames a 1/2 por frame, cancelable desde el frame 3, a
        /// 1 unidad de altura o más.
        /// </summary>
        public static readonly ParametrosDash Dash = new ParametrosDash(
            framesDash: 12,
            framesMinimosParaCancelar: 4,
            framesMinimosParaAtacar: 2,
            velocidadAdelante: Fijo.Medio,
            velocidadAtras: Fijo.DesdeFraccion(3, 8),
            friccion: Fijo.DesdeFraccion(1, 32),
            ventanaPlink: 2,
            framesDashAereo: 8,
            framesMinimosDashAereo: 3,
            velocidadDashAereo: Fijo.Medio,
            alturaMinimaDashAereo: Fijo.Uno);

        /// <summary>
        /// Barras de 100 unidades, 7 como máximo. Del daño base: 5 % al ejecutar, 20 % más al
        /// conectar o 10 % si es bloqueado; quien recibe gana 15 % y quien bloquea, 5 %.
        /// </summary>
        public static readonly ReglasBarra Barra = new ReglasBarra(
            unidadesPorBarra: 100,
            barrasMaximas: 7,
            porcentajeAlEjecutar: 5,
            porcentajeAlConectar: 20,
            porcentajeAlSerBloqueado: 10,
            porcentajeAlRecibir: 15,
            porcentajeAlBloquear: 5);

        /// <summary>99 segundos.</summary>
        public const int FramesPorRonda = 5940;

        public static readonly ParametrosLuchador Parametros = CrearParametros(VidaMaxima);

        /// <summary>El luchador de referencia con otra vida máxima; así se distingue a cada miembro de un equipo.</summary>
        public static ParametrosLuchador CrearParametros(int vidaMaxima) => new ParametrosLuchador(
            vidaMaxima: vidaMaxima,
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
                velocidadAtras: Fijo.DesdeFraccion(1, 10),
                velocidadVerticalSupersalto: Fijo.DesdeFraccion(3, 5),
                ventanaSupersalto: 6),
            dash: Dash,
            medidasDanio: new MedidasCajaDanio(
                medioAncho: Fijo.Medio,
                medioAltoDePie: Fijo.Uno,
                medioAltoAgachado: Fijo.Medio,
                medioAltoAire: Fijo.Medio),
            movimientos: new MovimientosLuchador(
                debil: GolpeDebil,
                medio: GolpeMedio,
                fuerte: GolpeFuerte,
                lanzador: Lanzador,
                debilAgachado: GolpeDebilAgachado,
                medioAgachado: GolpeMedioAgachado,
                fuerteAgachado: GolpeFuerteAgachado,
                debilAereo: GolpeDebilAereo,
                medioAereo: GolpeMedioAereo,
                fuerteAereo: GolpeFuerteAereo,
                asistencia: GolpeMedio,
                agarre: Agarre));

        /// <summary>
        /// La mitad del daño de un golpe queda como vida recuperable y cada golpe nuevo destruye
        /// un cuarto de la acumulada. En pantalla se regeneran 2 por frame tras 30 frames sin
        /// recibir golpes; en reserva, 5 por frame.
        /// </summary>
        public static readonly ReglasVidaRecuperable Recuperable = new ReglasVidaRecuperable(
            porcentajeRecuperable: 50,
            porcentajePerdidoAlRecibirGolpe: 25,
            framesEsperaRegeneracion: 30,
            regeneracionPorFrame: 2,
            regeneracionEnReservaPorFrame: 5);

        public static readonly ConfiguracionCombate Configuracion = ConfiguracionConReloj(FramesPorRonda);

        public static ConfiguracionCombate ConfiguracionConReloj(int framesPorRonda) => new ConfiguracionCombate(
            Parametros,
            Parametros,
            Fijo.DesdeEntero(-10),
            Fijo.DesdeEntero(10),
            friccionRetroceso: Fijo.DesdeFraccion(1, 64),
            framesBuferEntradas: FramesBufer,
            danio: Danio,
            juggle: Juggle,
            vidaRecuperable: Recuperable,
            barra: Barra,
            agarres: Agarres,
            framesPorRonda: framesPorRonda);

        /// <summary>
        /// Quien entra salta 8 frames desde 2 unidades detrás y 2 de altura. El primer paso del
        /// salto ocurre en el mismo frame del cambio, así que aterriza 7 frames después.
        /// </summary>
        public const int FramesEntrada = 8;

        /// <summary>Frames que hay que simular tras el frame del cambio para que quien entra ya tenga control: el resto del salto y 3 de aterrizaje.</summary>
        public const int FramesHastaActuarTrasEntrar = FramesEntrada + 2;

        /// <summary>El que sale se ve 10 frames alejándose 1/4 de unidad por frame hacia atrás y hacia arriba.</summary>
        public const int FramesSalida = 10;

        public const int FramesEsperaCambio = 60;
        public const int FramesPausaNocaut = 30;

        /// <summary>Mantener un botón de asistencia 4 frames cambia de luchador; soltarlo antes llama al asistente.</summary>
        public const int FramesMantenerParaCambiar = 4;

        /// <summary>El asistente tarda 6 frames en llegar y después hace el golpe medio (golpea 6 frames más tarde).</summary>
        public const int FramesLlegadaAsistencia = 6;

        /// <summary>Tras su ataque, el asistente se queda 10 frames más en pantalla. Aparece 1 unidad por delante del activo y recibe un 50 % más de daño.</summary>
        public const int FramesRetiradaAsistencia = 10;

        public const int FramesEsperaAsistencia = 90;

        /// <summary>Vida máxima de los tres miembros de cada equipo de prueba.</summary>
        public const int VidaSegundo = 1200;
        public const int VidaTercero = 1400;

        public static readonly ParametrosEquipo EquipoDePrueba = new ParametrosEquipo(
            CrearParametros(VidaMaxima),
            CrearParametros(VidaSegundo),
            CrearParametros(VidaTercero));

        public static readonly ConfiguracionEquipos Equipos = EquiposConReloj(FramesPorRonda);

        public static ConfiguracionEquipos EquiposConReloj(int framesPorRonda) => new ConfiguracionEquipos(
            EquipoDePrueba,
            EquipoDePrueba,
            ConfiguracionConReloj(framesPorRonda),
            framesEsperaCambio: FramesEsperaCambio,
            framesPausaNocaut: FramesPausaNocaut,
            distanciaEntrada: Fijo.DesdeEntero(4),
            framesMantenerParaCambiar: FramesMantenerParaCambiar,
            framesLlegadaAsistencia: FramesLlegadaAsistencia,
            framesEsperaAsistencia: FramesEsperaAsistencia,
            framesEntrada: FramesEntrada,
            desplazamientoEntrada: VectorFijo.DesdeEnteros(2, 2),
            framesSalida: FramesSalida,
            velocidadSalida: new VectorFijo(Fijo.DesdeFraccion(1, 4), Fijo.DesdeFraccion(1, 4)),
            framesRetiradaAsistencia: FramesRetiradaAsistencia,
            distanciaAsistencia: Fijo.Uno,
            porcentajeDanioAAsistentes: 150);

        public static EstadoCombateEquipos InicialPorEquipos(int x1, int x2) =>
            EstadoCombateEquipos.Crear(VectorFijo.DesdeEnteros(x1, 0), VectorFijo.DesdeEnteros(x2, 0), Equipos);

        public static EstadoCombateEquipos SimularEquipos(EstadoCombateEquipos estado, BotonesEntrada jugador1, BotonesEntrada jugador2, int frames)
        {
            var entradas = new EntradasFrame(jugador1, jugador2);

            for (var i = 0; i < frames; i++)
            {
                estado = SimuladorEquipos.Avanzar(estado, entradas, Equipos);
            }

            return estado;
        }

        /// <summary>El jugador 1 mantiene un botón de asistencia los frames justos para cambiar de luchador en el último.</summary>
        public static EstadoCombateEquipos CambiarJugador1(EstadoCombateEquipos estado, BotonesEntrada botonDeAsistencia) =>
            SimularEquipos(estado, botonDeAsistencia, BotonesEntrada.Ninguno, FramesMantenerParaCambiar);

        /// <summary>El jugador 1 pulsa y suelta un botón de asistencia: el asistente queda llamado en el segundo frame.</summary>
        public static EstadoCombateEquipos LlamarAsistenteJugador1(EstadoCombateEquipos estado, BotonesEntrada botonDeAsistencia, BotonesEntrada jugador2)
        {
            var pulsado = SimularEquipos(estado, botonDeAsistencia, jugador2, 1);

            return SimularEquipos(pulsado, BotonesEntrada.Ninguno, jugador2, 1);
        }

        /// <summary>Copia del estado con vida recuperable en el luchador en pantalla del jugador 1.</summary>
        public static EstadoCombateEquipos ConRecuperableActivo1(EstadoCombateEquipos estado, int recuperable) =>
            new EstadoCombateEquipos(
                new EstadoPartida(estado.Duelo.Frame, estado.Duelo.Luchador1.ConVidaRecuperable(recuperable), estado.Duelo.Luchador2, estado.Duelo.FramesRestantes, estado.Duelo.Resultado),
                estado.Equipo1,
                estado.Equipo2,
                estado.Resultado,
                estado.FramesDesdeNocaut);

        /// <summary>Copia del estado con la vida de los luchadores en pantalla cambiada.</summary>
        public static EstadoCombateEquipos ConVidasActivas(EstadoCombateEquipos estado, int vida1, int vida2) =>
            new EstadoCombateEquipos(ConVidas(estado.Duelo, vida1, vida2), estado.Equipo1, estado.Equipo2, estado.Resultado, estado.FramesDesdeNocaut);

        /// <summary>Copia del estado con otro equipo 2: sirve para dejar compañeros sin vida.</summary>
        public static EstadoCombateEquipos ConEquipo2(EstadoCombateEquipos estado, EstadoEquipo equipo2) =>
            new EstadoCombateEquipos(estado.Duelo, estado.Equipo1, equipo2, estado.Resultado, estado.FramesDesdeNocaut);

        /// <summary>Copia del estado con otro equipo 1.</summary>
        public static EstadoCombateEquipos ConEquipo1(EstadoCombateEquipos estado, EstadoEquipo equipo1) =>
            new EstadoCombateEquipos(estado.Duelo, equipo1, estado.Equipo2, estado.Resultado, estado.FramesDesdeNocaut);

        public static EstadoPartida Inicial(int x1, int x2) =>
            EstadoPartida.Crear(VectorFijo.DesdeEnteros(x1, 0), VectorFijo.DesdeEnteros(x2, 0), Configuracion);

        /// <summary>Copia del estado con el jugador 1 saltando, quieto en el aire a la altura indicada.</summary>
        public static EstadoPartida ConJugador1EnElAire(EstadoPartida estado, int altura) =>
            new EstadoPartida(estado.Frame, EnElAire(estado.Luchador1, altura), estado.Luchador2, estado.FramesRestantes, estado.Resultado);

        /// <summary>Copia del estado con el jugador 2 saltando, quieto en el aire a la altura indicada.</summary>
        public static EstadoPartida ConJugador2EnElAire(EstadoPartida estado, int altura) =>
            new EstadoPartida(estado.Frame, estado.Luchador1, EnElAire(estado.Luchador2, altura), estado.FramesRestantes, estado.Resultado);

        /// <summary>Copia del estado con el jugador 2 en el primer frame de su recuperación en el aire, a la altura indicada.</summary>
        public static EstadoPartida ConJugador2RecuperandoseEnElAire(EstadoPartida estado, int altura) =>
            new EstadoPartida(
                estado.Frame,
                estado.Luchador1,
                EnElAire(estado.Luchador2, altura).ConAccionNueva(AccionLuchador.RecuperandoEnAire),
                estado.FramesRestantes,
                estado.Resultado);

        private static EstadoLuchador EnElAire(EstadoLuchador luchador, int altura) =>
            luchador.ConPosicion(luchador.Posicion.ConY(Fijo.DesdeEntero(altura))).ConAccionNueva(AccionLuchador.EnAire);

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
