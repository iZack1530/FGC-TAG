using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Contenido
{
    /// <summary>
    /// Valores del prototipo con cajas grises (fase 1). Unidades en metros y velocidades por frame
    /// de 60 Hz: 0.06 m/frame equivale a 3.6 m/s. El salto sube unos 2.3 m y dura 43 frames,
    /// en el rango de Guilty Gear y Dragon Ball FighterZ; la caja de tierra mide 1.8 m, así que en
    /// lo alto del salto se puede pasar por encima del rival. El supersalto (saltar hasta 8 frames
    /// después de haber estado agachado) sube unos 4.6 m y dura 61 frames.
    /// </summary>
    public static class DatosPrueba
    {
        public static readonly ParametrosSalto SaltoCajaGris = new ParametrosSalto(
            framesPreSalto: 4,
            framesAterrizaje: 3,
            velocidadVertical: Fijo.DesdeFraccion(21, 100),
            gravedad: Fijo.DesdeFraccion(1, 100),
            velocidadAdelante: Fijo.DesdeFraccion(7, 100),
            velocidadAtras: Fijo.DesdeFraccion(6, 100),
            velocidadVerticalSupersalto: Fijo.DesdeFraccion(30, 100),
            ventanaSupersalto: 8);

        /// <summary>
        /// Golpe débil de pie: golpea en el frame 5, dura 16 frames, +3 al golpear y -1 al
        /// bloquear. El retroceso aleja al rival unos 0.21 m al golpear y 0.36 m al bloquear.
        /// </summary>
        public static readonly DatosMovimiento GolpeDebilCajaGris = new DatosMovimiento(
            arranque: 4,
            activos: 3,
            recuperacion: 9,
            hitstun: 14,
            hitstunAereo: 20,
            blockstun: 10,
            hitstop: 4,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(6, 100),
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
            hitstunAereo: 22,
            blockstun: 13,
            hitstop: 8,
            danio: 600,
            retrocesoAlGolpear: Fijo.DesdeFraccion(7, 100),
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
            hitstunAereo: 24,
            blockstun: 16,
            hitstop: 13,
            danio: 900,
            retrocesoAlGolpear: Fijo.DesdeFraccion(8, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(12, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(105, 100), Fijo.DesdeFraccion(100, 100)),
                Fijo.DesdeFraccion(80, 100),
                Fijo.DesdeFraccion(30, 100)));

        /// <summary>Débil agachado: golpe bajo; golpea en el frame 6, dura 17 frames, +2 al golpear y -1 al bloquear.</summary>
        public static readonly DatosMovimiento GolpeDebilAgachadoCajaGris = new DatosMovimiento(
            arranque: 5,
            activos: 3,
            recuperacion: 9,
            hitstun: 13,
            hitstunAereo: 20,
            blockstun: 10,
            hitstop: 4,
            danio: 250,
            retrocesoAlGolpear: Fijo.DesdeFraccion(6, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(8, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(70, 100), Fijo.DesdeFraccion(35, 100)),
                Fijo.DesdeFraccion(45, 100),
                Fijo.DesdeFraccion(20, 100)),
            altura: AlturaGolpe.Bajo);

        /// <summary>Medio agachado: golpe bajo de más alcance; golpea en el frame 8, dura 25 frames, 0 al golpear y -4 al bloquear.</summary>
        public static readonly DatosMovimiento GolpeMedioAgachadoCajaGris = new DatosMovimiento(
            arranque: 7,
            activos: 3,
            recuperacion: 15,
            hitstun: 17,
            hitstunAereo: 22,
            blockstun: 13,
            hitstop: 8,
            danio: 550,
            retrocesoAlGolpear: Fijo.DesdeFraccion(7, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(10, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(90, 100), Fijo.DesdeFraccion(30, 100)),
                Fijo.DesdeFraccion(65, 100),
                Fijo.DesdeFraccion(22, 100)),
            altura: AlturaGolpe.Bajo);

        /// <summary>Fuerte agachado: el golpe bajo de más alcance; golpea en el frame 11, dura 36 frames, -4 al golpear y -9 al bloquear.</summary>
        public static readonly DatosMovimiento GolpeFuerteAgachadoCajaGris = new DatosMovimiento(
            arranque: 10,
            activos: 4,
            recuperacion: 22,
            hitstun: 21,
            hitstunAereo: 24,
            blockstun: 16,
            hitstop: 13,
            danio: 850,
            retrocesoAlGolpear: Fijo.DesdeFraccion(8, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(12, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(105, 100), Fijo.DesdeFraccion(30, 100)),
                Fijo.DesdeFraccion(80, 100),
                Fijo.DesdeFraccion(25, 100)),
            altura: AlturaGolpe.Bajo);

        /// <summary>
        /// Lanzador (botón Especial): golpea en el frame 9, dura 34 frames y es muy castigable si
        /// lo bloquean (-10). Si golpea, levanta al rival unos 2.5 m y se cancela con un salto
        /// manteniendo arriba para seguir el combo en el aire.
        /// </summary>
        public static readonly DatosMovimiento LanzadorCajaGris = new DatosMovimiento(
            arranque: 8,
            activos: 4,
            recuperacion: 22,
            hitstun: 20,
            hitstunAereo: 30,
            blockstun: 15,
            hitstop: 13,
            danio: 800,
            retrocesoAlGolpear: Fijo.DesdeFraccion(2, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(10, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(80, 100), Fijo.DesdeFraccion(140, 100)),
                Fijo.DesdeFraccion(70, 100),
                Fijo.DesdeFraccion(90, 100)),
            velocidadLanzamiento: Fijo.DesdeFraccion(26, 100),
            cancelableConSalto: true);

        /// <summary>Débil aéreo: golpea en el frame 6 y dura 17 frames.</summary>
        public static readonly DatosMovimiento GolpeDebilAereoCajaGris = new DatosMovimiento(
            arranque: 5,
            activos: 4,
            recuperacion: 8,
            hitstun: 14,
            hitstunAereo: 22,
            blockstun: 10,
            hitstop: 4,
            danio: 300,
            retrocesoAlGolpear: Fijo.DesdeFraccion(3, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(6, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(55, 100), Fijo.DesdeFraccion(60, 100)),
                Fijo.DesdeFraccion(50, 100),
                Fijo.DesdeFraccion(55, 100)),
            altura: AlturaGolpe.Alto);

        /// <summary>Medio aéreo: golpea en el frame 8 y dura 23 frames.</summary>
        public static readonly DatosMovimiento GolpeMedioAereoCajaGris = new DatosMovimiento(
            arranque: 7,
            activos: 4,
            recuperacion: 12,
            hitstun: 17,
            hitstunAereo: 24,
            blockstun: 13,
            hitstop: 8,
            danio: 550,
            retrocesoAlGolpear: Fijo.DesdeFraccion(3, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(8, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(65, 100), Fijo.DesdeFraccion(55, 100)),
                Fijo.DesdeFraccion(60, 100),
                Fijo.DesdeFraccion(60, 100)),
            altura: AlturaGolpe.Alto);

        /// <summary>Fuerte aéreo: golpea en el frame 10 y dura 30 frames.</summary>
        public static readonly DatosMovimiento GolpeFuerteAereoCajaGris = new DatosMovimiento(
            arranque: 9,
            activos: 5,
            recuperacion: 16,
            hitstun: 21,
            hitstunAereo: 26,
            blockstun: 16,
            hitstop: 13,
            danio: 800,
            retrocesoAlGolpear: Fijo.DesdeFraccion(4, 100),
            retrocesoAlBloquear: Fijo.DesdeFraccion(10, 100),
            cajaGolpe: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(75, 100), Fijo.DesdeFraccion(50, 100)),
                Fijo.DesdeFraccion(70, 100),
                Fijo.DesdeFraccion(70, 100)),
            altura: AlturaGolpe.Alto);

        /// <summary>
        /// Un rival golpeado en el aire rebota hacia arriba con 0.10 por frame; cae con gravedad
        /// 0.012 más 0.0015 por cada golpe del combo, y al tocar el suelo queda 40 frames derribado.
        /// El aturdimiento aéreo de cada golpe dura 2 frames menos por golpe del combo (mínimo 12);
        /// al agotarse, el rival se recupera en el aire con un pequeño salto. La recuperación dura
        /// 16 frames: los 8 primeros es invencible y en los 8 restantes puede bloquear, pero no atacar.
        /// </summary>
        public static readonly ReglasJuggle JuggleCajasGrises = new ReglasJuggle(
            impulsoAlGolpear: Fijo.DesdeFraccion(10, 100),
            gravedad: Fijo.DesdeFraccion(12, 1000),
            gravedadExtraPorGolpe: Fijo.DesdeFraccion(15, 10000),
            framesDerribado: 40,
            reduccionAturdimientoPorGolpe: 2,
            aturdimientoMinimo: 12,
            velocidadRecuperacion: Fijo.DesdeFraccion(5, 100),
            impulsoRecuperacion: Fijo.DesdeFraccion(12, 100),
            framesRecuperacion: 16,
            framesRecuperacionInvencible: 8);

        public static readonly MedidasCajaDanio CajaDanioCajaGris = new MedidasCajaDanio(
            medioAncho: Fijo.DesdeFraccion(4, 10),
            medioAltoDePie: Fijo.DesdeFraccion(9, 10),
            medioAltoAgachado: Fijo.DesdeFraccion(5, 10),
            medioAltoAire: Fijo.DesdeFraccion(5, 10));

        /// <summary>
        /// Cada golpe del combo hace 10 puntos porcentuales menos de daño, con un mínimo del 30%.
        /// Un golpe bloqueado hace el 10% de su daño base (daño chip) y nunca noquea.
        /// </summary>
        public static readonly ReglasDanio DanioCajasGrises = new ReglasDanio(reduccionPorGolpe: 10, porcentajeMinimo: 30, porcentajeAlBloquear: 10);

        /// <summary>
        /// Vida azul: la mitad del daño de cada golpe no bloqueado se puede recuperar y cada golpe
        /// nuevo destruye un cuarto de la acumulada. En pantalla se regeneran 2 por frame (120 por
        /// segundo) tras 3 segundos sin recibir golpes; en reserva, 6 por frame (360 por segundo)
        /// y sin espera.
        /// </summary>
        public static readonly ReglasVidaRecuperable VidaRecuperableCajasGrises = new ReglasVidaRecuperable(
            porcentajeRecuperable: 50,
            porcentajePerdidoAlRecibirGolpe: 25,
            framesEsperaRegeneracion: 180,
            regeneracionPorFrame: 2,
            regeneracionEnReservaPorFrame: 6);

        /// <summary>
        /// Agarre normal: una dirección más el golpe fuerte cerca del rival. Activo en los frames
        /// 5 y 6, con un alcance de 0.75 m (menos que cualquier golpe); si falla, dura 30 frames.
        /// Si atrapa, sujeta al rival 18 frames, le hace 1000 de daño y lo lanza; quien agarra
        /// recupera el control 22 frames después del lanzamiento.
        /// </summary>
        public static readonly DatosAgarre AgarreCajaGris = new DatosAgarre(
            arranque: 4,
            activos: 2,
            recuperacion: 24,
            cajaAgarre: new CajaColision(
                new VectorFijo(Fijo.DesdeFraccion(50, 100), Fijo.DesdeFraccion(100, 100)),
                Fijo.DesdeFraccion(25, 100),
                Fijo.DesdeFraccion(40, 100)),
            framesSujecion: 18,
            recuperacionTrasLanzar: 22,
            danio: 1000,
            velocidadLanzamiento: new VectorFijo(Fijo.DesdeFraccion(7, 100), Fijo.DesdeFraccion(18, 100)),
            aturdimientoLanzado: 60);

        /// <summary>
        /// El agarre sale si el rival está en tierra a 1.3 m o menos; más lejos sale el golpe fuerte.
        /// Tras salir de un hitstun, un blockstun, un derribo o una recuperación aérea, el luchador
        /// no puede ser agarrado durante 8 frames. El rival sujeto tiene 9 frames para escapar
        /// pulsando una dirección más el golpe fuerte; un agarre roto separa a los dos 20 frames.
        /// </summary>
        public static readonly ReglasAgarre AgarresCajasGrises = new ReglasAgarre(
            framesProteccion: 8,
            distanciaActivacion: Fijo.DesdeFraccion(13, 10),
            framesVentanaEscape: 9,
            framesAgarreRoto: 20,
            retrocesoAlEscapar: Fijo.DesdeFraccion(9, 100));

        /// <summary>
        /// Dash: dos botones de golpe a la vez, hacia el rival o, manteniendo atrás, alejándose.
        /// En tierra arranca a 0.19 m/frame (0.16 hacia atrás) y pierde 0.012 por frame: recorre
        /// 1.6 m en sus 20 frames, los últimos ya parado, frente a 1.2 m caminando. Reiniciarlo
        /// mantiene la velocidad alta: con wavedash (agacharse desde el frame 7) se va a unos
        /// 0.135 m/frame y con plink dash (ataque desde el frame 3 y segundo botón en sus 2
        /// primeros frames) a unos 0.17. El ataque que cancela un dash hereda su impulso. El dash
        /// aéreo dura 14 frames a 0.13 m/frame, una vez por salto y a 0.7 m de altura o más; se
        /// cancela con un ataque aéreo desde el frame 4.
        /// </summary>
        public static readonly ParametrosDash DashCajaGris = new ParametrosDash(
            framesDash: 20,
            framesMinimosParaCancelar: 7,
            framesMinimosParaAtacar: 3,
            velocidadAdelante: Fijo.DesdeFraccion(19, 100),
            velocidadAtras: Fijo.DesdeFraccion(16, 100),
            friccion: Fijo.DesdeFraccion(12, 1000),
            ventanaPlink: 2,
            framesDashAereo: 14,
            framesMinimosDashAereo: 4,
            velocidadDashAereo: Fijo.DesdeFraccion(13, 100),
            alturaMinimaDashAereo: Fijo.DesdeFraccion(7, 10));

        /// <summary>
        /// Barra de súper: 7 barras de 1000 unidades. Del daño base de cada movimiento: 3 % al
        /// ejecutarlo (lo único que da un ataque al aire), 15 % más si conecta o 6 % más si lo
        /// bloquean; quien lo recibe gana 12 % y quien lo bloquea, 5 %.
        /// </summary>
        public static readonly ReglasBarra BarraCajasGrises = new ReglasBarra(
            unidadesPorBarra: 1000,
            barrasMaximas: 7,
            porcentajeAlEjecutar: 3,
            porcentajeAlConectar: 15,
            porcentajeAlSerBloqueado: 6,
            porcentajeAlRecibir: 12,
            porcentajeAlBloquear: 5);

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
            dash: DashCajaGris,
            medidasDanio: CajaDanioCajaGris,
            movimientos: new MovimientosLuchador(
                debil: GolpeDebilCajaGris,
                medio: GolpeMedioCajaGris,
                fuerte: GolpeFuerteCajaGris,
                lanzador: LanzadorCajaGris,
                debilAgachado: GolpeDebilAgachadoCajaGris,
                medioAgachado: GolpeMedioAgachadoCajaGris,
                fuerteAgachado: GolpeFuerteAgachadoCajaGris,
                debilAereo: GolpeDebilAereoCajaGris,
                medioAereo: GolpeMedioAereoCajaGris,
                fuerteAereo: GolpeFuerteAereoCajaGris,
                asistencia: GolpeMedioCajaGris,
                agarre: AgarreCajaGris));

        public static readonly ConfiguracionCombate ConfiguracionCajasGrises = new ConfiguracionCombate(
            LuchadorCajaGris,
            LuchadorCajaGris,
            Fijo.DesdeEntero(-7),
            Fijo.DesdeEntero(7),
            friccionRetroceso: Fijo.DesdeFraccion(1, 100),
            framesBuferEntradas: 5,
            danio: DanioCajasGrises,
            juggle: JuggleCajasGrises,
            vidaRecuperable: VidaRecuperableCajasGrises,
            barra: BarraCajasGrises,
            agarres: AgarresCajasGrises,
            framesPorRonda: FramesPorRonda);

        /// <summary>Por ahora los tres miembros de cada equipo son el mismo luchador de cajas grises.</summary>
        public static readonly ParametrosEquipo EquipoCajasGrises = new ParametrosEquipo(LuchadorCajaGris, LuchadorCajaGris, LuchadorCajaGris);

        /// <summary>
        /// Combate 3 contra 3. Mantener un botón de asistencia 10 frames cambia de luchador: el que
        /// sale se marcha saltando hacia atrás y el que entra llega con un salto de 24 frames desde
        /// 2.5 m detrás; cada equipo espera 2 segundos entre cambios. Soltar el botón antes llama
        /// al asistente: aparece hasta 0.9 m por delante, tarda 18 frames en atacar y se queda 20 frames más
        /// tras su ataque; mientras está en pantalla el rival puede golpearlo con un 50 % más de
        /// daño, todo recuperable. Hay 3 segundos de espera para llamar a otro.
        /// Tras un K.O., el luchador derrotado se queda en el suelo 3.5 segundos y entonces entra
        /// saltando el siguiente, a 4 m del rival.
        /// </summary>
        public static readonly ConfiguracionEquipos EquiposCajasGrises = new ConfiguracionEquipos(
            EquipoCajasGrises,
            EquipoCajasGrises,
            ConfiguracionCajasGrises,
            framesEsperaCambio: 120,
            framesPausaNocaut: 210,
            distanciaEntrada: Fijo.DesdeEntero(4),
            framesMantenerParaCambiar: 10,
            framesLlegadaAsistencia: 18,
            framesEsperaAsistencia: 180,
            framesEntrada: 24,
            desplazamientoEntrada: new VectorFijo(Fijo.DesdeFraccion(25, 10), Fijo.DesdeFraccion(22, 10)),
            framesSalida: 24,
            velocidadSalida: new VectorFijo(Fijo.DesdeFraccion(12, 100), Fijo.DesdeFraccion(12, 100)),
            framesRetiradaAsistencia: 20,
            distanciaAsistencia: Fijo.DesdeFraccion(9, 10),
            porcentajeDanioAAsistentes: 150);

        public static readonly EstadoCombateEquipos CombateInicial = EstadoCombateEquipos.Crear(
            VectorFijo.DesdeEnteros(-2, 0),
            VectorFijo.DesdeEnteros(2, 0),
            EquiposCajasGrises);
    }
}
