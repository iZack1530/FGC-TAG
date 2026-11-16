using FgcTag.Contenido;
using FgcTag.Entrada;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Simulacion;
using FgcTag.Presentacion;
using UnityEngine;

namespace FgcTag.Arranque
{
    /// <summary>
    /// Une las capas para el prototipo de cajas grises: lee entradas, avanza la simulación a
    /// 60 Hz fijos sin importar los fps de render y muestra el resultado. Basta con añadirlo a
    /// un GameObject vacío de una escena con cámara y luz.
    /// </summary>
    /// <remarks>F1 pausa la simulación, F2 la avanza de un frame en un frame y F3 empieza una ronda nueva.</remarks>
    public sealed class ArranquePartidaPrueba : MonoBehaviour
    {
        private const double SegundosPorFrame = 1.0 / ConstantesSimulacion.FramesPorSegundoLogica;

        /// <summary>Tope de pasos por actualización para que una pausa larga no congele el juego intentando recuperarse.</summary>
        private const int MaximoPasosPorActualizacion = 5;

        [SerializeField]
        [Tooltip("Coloca la cámara principal con campo de visión bajo para ver todo el escenario.")]
        private bool _colocarCamara = true;

        [SerializeField]
        [Tooltip("Muestra frame, suma de verificación y estado de cada luchador.")]
        private bool _mostrarDepuracion = true;

        private ConfiguracionEquipos _configuracionEquipos;
        private EstadoCombateEquipos _combate;
        private VistaCajasPrueba _vista;
        private double _acumulado;
        private bool _pausado;
        private MedidorVentaja _medidor;
        private GUIStyle _estiloTexto;
        private GUIStyle _estiloAviso;
        private GUIStyle _estiloMarcador;
        private string _ultimoAtaqueJugador1 = string.Empty;

        private static readonly Color ColorFondoBarra = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color ColorVidaRecuperable = new Color(0.15f, 0.3f, 0.75f);
        private static readonly Color ColorBarraSuper = new Color(0.85f, 0.65f, 0.1f);
        private static readonly Color ColorBarraSuperCompleta = new Color(1f, 0.9f, 0.3f);
        private static readonly Color ColorVidaJugador1 = new Color(0.25f, 0.55f, 0.95f);
        private static readonly Color ColorVidaJugador2 = new Color(0.95f, 0.35f, 0.3f);

        /// <summary>El duelo entre los dos luchadores que están en pantalla.</summary>
        private EstadoPartida Duelo => _combate.Duelo;

        /// <summary>Reglas del duelo con los parámetros de los dos luchadores activos.</summary>
        private ConfiguracionCombate ReglasDuelo =>
            _configuracionEquipos.DueloEntre(_combate.Equipo1.IndiceActivo, _combate.Equipo2.IndiceActivo);

        private void Awake()
        {
            _configuracionEquipos = DatosPrueba.EquiposCajasGrises;
            _combate = DatosPrueba.CombateInicial;
            _vista = new VistaCajasPrueba(transform, _configuracionEquipos.Reglas);
            MostrarCombate();
            _medidor = new MedidorVentaja();

            if (_colocarCamara)
            {
                ColocarCamara();
            }
        }

        private void Update()
        {
            if (TeclasDepuracion.ReiniciarRondaPulsado())
            {
                _combate = DatosPrueba.CombateInicial;
                _medidor = new MedidorVentaja();
                _ultimoAtaqueJugador1 = string.Empty;
                _acumulado = 0;
            }

            if (TeclasDepuracion.AlternarPausaPulsada())
            {
                _pausado = !_pausado;
                _acumulado = 0;
            }

            if (_pausado)
            {
                if (TeclasDepuracion.AvanzarFramePulsado())
                {
                    AvanzarUnFrame();
                }

                MostrarCombate();
                return;
            }

            _acumulado += Time.unscaledDeltaTime;
            var pasos = 0;

            while (_acumulado >= SegundosPorFrame && pasos < MaximoPasosPorActualizacion)
            {
                AvanzarUnFrame();
                _acumulado -= SegundosPorFrame;
                pasos++;
            }

            if (pasos == MaximoPasosPorActualizacion)
            {
                _acumulado = 0;
            }

            MostrarCombate();
        }

        private void MostrarCombate()
        {
            _vista.Mostrar(Duelo, ReglasDuelo, _combate.Equipo1.IndiceActivo, _combate.Equipo2.IndiceActivo);
            _vista.MostrarAsistentes(_combate.Equipo1, _combate.Equipo2, _configuracionEquipos);
            _vista.MostrarSalientes(_combate.Equipo1, _combate.Equipo2, _configuracionEquipos);
        }

        private void OnDestroy() => _vista?.Destruir();

        private void AvanzarUnFrame()
        {
            var entradas = new EntradasFrame(LectorEntradas.LeerJugador1(), LectorEntradas.LeerJugador2());
            var anterior = Duelo;
            _combate = SimuladorEquipos.Avanzar(_combate, entradas, _configuracionEquipos);
            _medidor.Observar(anterior, Duelo);

            if (EstadosAccion.EstaAtacando(Duelo.Luchador1.Accion))
            {
                _ultimoAtaqueJugador1 = DescribirMovimiento(
                    NombreAtaque(Duelo.Luchador1),
                    ReglasDuelo.Jugador1.MovimientoEnCurso(Duelo.Luchador1));
            }
            else if (Duelo.Luchador1.Accion == AccionLuchador.IntentandoAgarre)
            {
                _ultimoAtaqueJugador1 = DescribirAgarre(ReglasDuelo.Jugador1.AgarreEnCurso(Duelo.Luchador1), Duelo.Luchador1.AgarraHaciaAtras);
            }
        }

        private void OnGUI()
        {
            if (_estiloTexto == null)
            {
                _estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                _estiloAviso = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
                _estiloMarcador = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            }

            DibujarMarcador();

            if (!_mostrarDepuracion)
            {
                return;
            }

            var modo = _pausado ? "EN PAUSA (F2 avanza un frame, F1 reanuda)" : "F1 pausa";

            var reglas = ReglasDuelo;

            GUILayout.BeginArea(new Rect(12, 12, Screen.width - 24, 280), GUI.skin.box);
            GUILayout.Label($"Frame {Duelo.Frame}   Suma {_combate.CalcularSumaVerificacion()}   {modo}", _estiloTexto);
            GUILayout.Label(DescribirLuchador("J1", Duelo.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirLuchador("J2", Duelo.Luchador2), _estiloTexto);
            GUILayout.Label(DescribirEquipo("Equipo J1", _combate.Equipo1, Duelo.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirEquipo("Equipo J2", _combate.Equipo2, Duelo.Luchador2), _estiloTexto);
            GUILayout.Label($"Último ataque de J1: {_ultimoAtaqueJugador1}", _estiloTexto);
            GUILayout.Label(_medidor.Resumen, _estiloTexto);
            GUILayout.Label($"Daño en combo: -{reglas.Danio.ReduccionPorGolpe}% por golpe, mínimo {reglas.Danio.PorcentajeMinimo}%; bloqueado {reglas.Danio.PorcentajeAlBloquear}%   F3 combate nuevo", _estiloTexto);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Barras de vida, reloj y resultado, en la parte baja de la pantalla. La barra grande es la
        /// del luchador en pantalla; encima, dos barras pequeñas con la vida de sus compañeros.
        /// </summary>
        private void DibujarMarcador()
        {
            const float Margen = 24f;
            const float AltoBarra = 26f;
            const float AnchoReloj = 90f;
            const float AltoBarraSuper = 16f;
            const float SeparacionBarraSuper = 6f;

            var anchoBarra = (Screen.width - 2f * Margen - AnchoReloj) / 2f;
            var y = Screen.height - Margen - AltoBarra - SeparacionBarraSuper - AltoBarraSuper;

            DibujarVidaEquipo(Margen, y, anchoBarra, AltoBarra, _combate.Equipo1, Duelo.Luchador1, _configuracionEquipos.Equipo1, ColorVidaJugador1, false);
            DibujarVidaEquipo(Screen.width - Margen - anchoBarra, y, anchoBarra, AltoBarra, _combate.Equipo2, Duelo.Luchador2, _configuracionEquipos.Equipo2, ColorVidaJugador2, true);

            var segundos = (Duelo.FramesRestantes + ConstantesSimulacion.FramesPorSegundoLogica - 1) / ConstantesSimulacion.FramesPorSegundoLogica;
            GUI.Label(new Rect(Margen + anchoBarra, y - 10f, AnchoReloj, AltoBarra + 20f), segundos.ToString(), _estiloAviso);

            var reglasBarra = _configuracionEquipos.Reglas.Barra;
            DibujarBarraSuper(new Rect(Margen, y + AltoBarra + SeparacionBarraSuper, anchoBarra, AltoBarraSuper), Duelo.Luchador1.Barra, reglasBarra, false);
            DibujarBarraSuper(new Rect(Screen.width - Margen - anchoBarra, y + AltoBarra + SeparacionBarraSuper, anchoBarra, AltoBarraSuper), Duelo.Luchador2.Barra, reglasBarra, true);

            if (_combate.Terminado)
            {
                GUI.Label(new Rect(0f, y - 130f, Screen.width, 60f), DescribirResultado(_combate.Resultado, Duelo.FramesRestantes == 0), _estiloAviso);
            }
            else if (_combate.EsperandoRelevo)
            {
                GUI.Label(new Rect(0f, y - 130f, Screen.width, 60f), "K.O.: entra el siguiente luchador", _estiloAviso);
            }
        }

        /// <summary>
        /// Barra de súper bajo la vida: un segmento por barra, relleno según lo acumulado, y el
        /// número de barras completas. Se llena desde el borde exterior hacia el centro.
        /// </summary>
        private void DibujarBarraSuper(Rect zona, int unidades, in ReglasBarra reglas, bool desdeLaDerecha)
        {
            const float Separacion = 3f;

            var colorAnterior = GUI.color;
            var anchoSegmento = (zona.width - Separacion * (reglas.BarrasMaximas - 1)) / reglas.BarrasMaximas;

            for (var barra = 0; barra < reglas.BarrasMaximas; barra++)
            {
                var posicion = desdeLaDerecha ? reglas.BarrasMaximas - 1 - barra : barra;
                var x = zona.x + posicion * (anchoSegmento + Separacion);
                var lleno = Mathf.Clamp01((unidades - barra * reglas.UnidadesPorBarra) / (float)reglas.UnidadesPorBarra);
                var anchoLleno = anchoSegmento * lleno;

                GUI.color = ColorFondoBarra;
                GUI.DrawTexture(new Rect(x, zona.y, anchoSegmento, zona.height), Texture2D.whiteTexture);
                GUI.color = lleno >= 1f ? ColorBarraSuperCompleta : ColorBarraSuper;
                GUI.DrawTexture(new Rect(desdeLaDerecha ? x + anchoSegmento - anchoLleno : x, zona.y, anchoLleno, zona.height), Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
            GUI.Label(
                new Rect(zona.x, zona.y - 3f, zona.width, zona.height + 6f),
                $"Súper {reglas.BarrasCompletas(unidades)} / {reglas.BarrasMaximas}",
                _estiloMarcador);
            GUI.color = colorAnterior;
        }

        private static void DibujarVidaEquipo(
            float x,
            float y,
            float ancho,
            float alto,
            in EstadoEquipo equipo,
            in EstadoLuchador activo,
            in ParametrosEquipo parametros,
            Color color,
            bool pegadaALaIzquierda)
        {
            const float AltoReserva = 12f;
            const float Separacion = 4f;

            DibujarBarraVida(new Rect(x, y, ancho, alto), activo.Vida, activo.VidaRecuperable, parametros.Obtener(equipo.IndiceActivo).VidaMaxima, color, pegadaALaIzquierda, true);

            for (var pasos = 1; pasos < EstadoEquipo.Miembros; pasos++)
            {
                var miembro = equipo.IndiceDelCompanero(pasos);
                var anchoReserva = ancho * 0.45f;
                var xReserva = pegadaALaIzquierda ? x : x + ancho - anchoReserva;
                var yReserva = y - pasos * (AltoReserva + Separacion);

                DibujarBarraVida(
                    new Rect(xReserva, yReserva, anchoReserva, AltoReserva),
                    equipo.VidaEnReserva(miembro),
                    equipo.Reserva(miembro).Recuperable,
                    parametros.Obtener(miembro).VidaMaxima,
                    color,
                    pegadaALaIzquierda,
                    false);
            }
        }

        private static string DescribirEquipo(string etiqueta, in EstadoEquipo equipo, in EstadoLuchador activo)
        {
            var cambio = equipo.FramesEsperaCambio > 0 ? $"cambio en {equipo.FramesEsperaCambio} f" : "cambio listo";
            var asistencia = equipo.Asistente.EnPantalla
                ? $"asistente {equipo.Asistente.Miembro + 1} en pantalla"
                : equipo.FramesEsperaAsistencia > 0 ? $"asistencia en {equipo.FramesEsperaAsistencia} f" : "asistencia lista";

            return $"{etiqueta}: en pantalla el luchador {equipo.IndiceActivo + 1} de {EstadoEquipo.Miembros}   vidas {DescribirVida(equipo, 0, activo)} | {DescribirVida(equipo, 1, activo)} | {DescribirVida(equipo, 2, activo)}   {cambio}   {asistencia}";
        }

        private static string DescribirVida(in EstadoEquipo equipo, int miembro, in EstadoLuchador activo) =>
            $"{equipo.VidaDe(miembro, activo)} (+{equipo.RecuperableDe(miembro, activo)})";

        /// <summary>La vida recuperable se dibuja en azul oscuro a continuación de la vida, hacia el borde exterior.</summary>
        private static void DibujarBarraVida(Rect zona, int vida, int recuperable, int vidaMaxima, Color color, bool pegadaALaIzquierda, bool conNumeros)
        {
            var colorAnterior = GUI.color;
            var anchoVida = zona.width * vida / vidaMaxima;
            var anchoRecuperable = zona.width * recuperable / vidaMaxima;
            var xVida = pegadaALaIzquierda ? zona.x : zona.x + zona.width - anchoVida;
            var xRecuperable = pegadaALaIzquierda ? xVida + anchoVida : xVida - anchoRecuperable;

            GUI.color = ColorFondoBarra;
            GUI.DrawTexture(zona, Texture2D.whiteTexture);
            GUI.color = ColorVidaRecuperable;
            GUI.DrawTexture(new Rect(xRecuperable, zona.y, anchoRecuperable, zona.height), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(xVida, zona.y, anchoVida, zona.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (conNumeros)
            {
                GUI.Label(new Rect(zona.x + 8f, zona.y + 2f, zona.width, zona.height), $"{vida} (+{recuperable}) / {vidaMaxima}");
            }

            GUI.color = colorAnterior;
        }

        private static string DescribirResultado(ResultadoRonda resultado, bool porTiempo)
        {
            var motivo = porTiempo ? "Tiempo agotado" : "Equipo eliminado";

            switch (resultado)
            {
                case ResultadoRonda.GanaJugador1:
                    return $"{motivo}: gana el jugador 1   (F3: combate nuevo)";
                case ResultadoRonda.GanaJugador2:
                    return $"{motivo}: gana el jugador 2   (F3: combate nuevo)";
                default:
                    return $"{motivo}: empate   (F3: combate nuevo)";
            }
        }

        private static string DescribirLuchador(string etiqueta, in EstadoLuchador luchador) =>
            $"{etiqueta}: {(MedidorVentaja.TieneControl(luchador) ? "LIBRE" : "sin control")}   {DescribirAccion(luchador)} ({luchador.FramesEnAccion} f)   hitstop {luchador.FramesHitstop}   aturdido {luchador.FramesAturdimiento}   combo {luchador.GolpesEnCombo}   vida {luchador.Vida}   barra {luchador.Barra}   protección de agarre {luchador.FramesProteccionAgarre}   búfer {DescribirBufer(luchador)}   pos {luchador.Posicion}   vel {luchador.Velocidad}   mira {(luchador.MiraDerecha ? "derecha" : "izquierda")}";

        private static string DescribirAccion(in EstadoLuchador luchador) =>
            EstadosAccion.EstaAtacando(luchador.Accion)
                ? $"Atacando: {NombreAtaque(luchador)}"
                : luchador.Supersalto ? $"{luchador.Accion} (supersalto)" : luchador.Accion.ToString();

        private static string DescribirAltura(AlturaGolpe altura)
        {
            switch (altura)
            {
                case AlturaGolpe.Bajo:
                    return "bajo: se bloquea agachado";
                case AlturaGolpe.Alto:
                    return "alto: se bloquea de pie";
                default:
                    return "medio: se bloquea de pie o agachado";
            }
        }

        private static string NombreAtaque(in EstadoLuchador luchador)
        {
            if (luchador.Ataque == TipoAtaque.Especial)
            {
                return "Lanzador";
            }

            switch (EstadosAccion.PosturaDeAtaque(luchador.Accion))
            {
                case PosturaAtaque.Aire:
                    return $"{luchador.Ataque} aéreo";
                case PosturaAtaque.Agachado:
                    return $"{luchador.Ataque} agachado";
                default:
                    return luchador.Ataque.ToString();
            }
        }

        private static string DescribirBufer(in EstadoLuchador luchador) =>
            luchador.TieneAtaqueEnBufer ? $"{luchador.AtaqueEnBufer} ({luchador.FramesBufer} f)" : "vacío";

        private static string DescribirMovimiento(string nombre, in DatosMovimiento movimiento) =>
            $"{nombre}{(movimiento.Lanza ? " (lanza)" : string.Empty)} [{DescribirAltura(movimiento.Altura)}]: arranque {movimiento.Arranque}   activos {movimiento.Activos}   recuperación {movimiento.Recuperacion}   hitstop {movimiento.Hitstop}   daño {movimiento.Danio}   ventaja {movimiento.VentajaAlGolpear:+0;-0;0} al golpear, {movimiento.VentajaAlBloquear:+0;-0;0} al bloquear";

        private static string DescribirAgarre(in DatosAgarre agarre, bool haciaAtras) =>
            $"Agarre {(haciaAtras ? "hacia atrás" : "hacia adelante")}: arranque {agarre.Arranque}   activos {agarre.Activos}   recuperación si falla {agarre.Recuperacion}   sujeción {agarre.FramesSujecion}   daño {agarre.Danio}";

        private static void ColocarCamara()
        {
            var camara = Camera.main;

            if (camara == null)
            {
                return;
            }

            camara.orthographic = false;
            camara.fieldOfView = 20f;
            camara.transform.SetPositionAndRotation(new Vector3(0f, 2f, -24f), Quaternion.Euler(2f, 0f, 0f));
        }
    }
}
