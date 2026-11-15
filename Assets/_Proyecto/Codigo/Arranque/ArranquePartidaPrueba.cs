using FgcTag.Contenido;
using FgcTag.Entrada;
using FgcTag.Nucleo.Combate;
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

        private ConfiguracionCombate _configuracion;
        private EstadoPartida _estado;
        private VistaCajasPrueba _vista;
        private double _acumulado;
        private bool _pausado;
        private MedidorVentaja _medidor;
        private GUIStyle _estiloTexto;
        private GUIStyle _estiloAviso;

        private static readonly Color ColorFondoBarra = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color ColorVidaJugador1 = new Color(0.25f, 0.55f, 0.95f);
        private static readonly Color ColorVidaJugador2 = new Color(0.95f, 0.35f, 0.3f);

        private void Awake()
        {
            _configuracion = DatosPrueba.ConfiguracionCajasGrises;
            _estado = DatosPrueba.EstadoInicial;
            _vista = new VistaCajasPrueba(transform, _configuracion);
            _vista.Mostrar(_estado);
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
                _estado = DatosPrueba.EstadoInicial;
                _medidor = new MedidorVentaja();
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

                _vista.Mostrar(_estado);
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

            _vista.Mostrar(_estado);
        }

        private void OnDestroy() => _vista?.Destruir();

        private void AvanzarUnFrame()
        {
            var entradas = new EntradasFrame(LectorEntradas.LeerJugador1(), LectorEntradas.LeerJugador2());
            var anterior = _estado;
            _estado = SimuladorCombate.Avanzar(_estado, entradas, _configuracion);
            _medidor.Observar(anterior, _estado);
        }

        private void OnGUI()
        {
            if (_estiloTexto == null)
            {
                _estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                _estiloAviso = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
            }

            DibujarMarcador();

            if (!_mostrarDepuracion)
            {
                return;
            }

            var modo = _pausado ? "EN PAUSA (F2 avanza un frame, F1 reanuda)" : "F1 pausa";

            GUILayout.BeginArea(new Rect(12, 12, Screen.width - 24, 275), GUI.skin.box);
            GUILayout.Label($"Frame {_estado.Frame}   Suma {_estado.CalcularSumaVerificacion()}   {modo}", _estiloTexto);
            GUILayout.Label(DescribirLuchador("J1", _estado.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirLuchador("J2", _estado.Luchador2), _estiloTexto);
            GUILayout.Label(DescribirMovimiento("Débil", _configuracion.Jugador1.GolpeDebil), _estiloTexto);
            GUILayout.Label(DescribirMovimiento("Medio", _configuracion.Jugador1.GolpeMedio), _estiloTexto);
            GUILayout.Label(DescribirMovimiento("Fuerte", _configuracion.Jugador1.GolpeFuerte), _estiloTexto);
            GUILayout.Label(_medidor.Resumen, _estiloTexto);
            GUILayout.Label($"Daño en combo: -{_configuracion.Danio.ReduccionPorGolpe}% por golpe, mínimo {_configuracion.Danio.PorcentajeMinimo}%   F3 nueva ronda", _estiloTexto);
            GUILayout.EndArea();
        }

        /// <summary>Barras de vida, reloj y resultado de la ronda, en la parte baja de la pantalla.</summary>
        private void DibujarMarcador()
        {
            const float Margen = 24f;
            const float AltoBarra = 26f;
            const float AnchoReloj = 90f;

            var anchoBarra = (Screen.width - 2f * Margen - AnchoReloj) / 2f;
            var y = Screen.height - Margen - AltoBarra;

            DibujarBarraVida(new Rect(Margen, y, anchoBarra, AltoBarra), _estado.Luchador1.Vida, _configuracion.Jugador1.VidaMaxima, ColorVidaJugador1, false);
            DibujarBarraVida(new Rect(Screen.width - Margen - anchoBarra, y, anchoBarra, AltoBarra), _estado.Luchador2.Vida, _configuracion.Jugador2.VidaMaxima, ColorVidaJugador2, true);

            var segundos = (_estado.FramesRestantes + ConstantesSimulacion.FramesPorSegundoLogica - 1) / ConstantesSimulacion.FramesPorSegundoLogica;
            GUI.Label(new Rect(Margen + anchoBarra, y - 10f, AnchoReloj, AltoBarra + 20f), segundos.ToString(), _estiloAviso);

            if (_estado.RondaTerminada)
            {
                GUI.Label(new Rect(0f, y - 80f, Screen.width, 60f), DescribirResultado(_estado), _estiloAviso);
            }
        }

        /// <summary>La vida restante queda pegada al centro de la pantalla: cada barra se vacía desde su borde exterior.</summary>
        private static void DibujarBarraVida(Rect zona, int vida, int vidaMaxima, Color color, bool pegadaALaIzquierda)
        {
            var colorAnterior = GUI.color;
            var anchoVida = zona.width * vida / vidaMaxima;
            var xVida = pegadaALaIzquierda ? zona.x : zona.x + zona.width - anchoVida;

            GUI.color = ColorFondoBarra;
            GUI.DrawTexture(zona, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(xVida, zona.y, anchoVida, zona.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(zona.x + 8f, zona.y + 2f, zona.width, zona.height), $"{vida} / {vidaMaxima}");
            GUI.color = colorAnterior;
        }

        private static string DescribirResultado(in EstadoPartida estado)
        {
            var motivo = estado.Luchador1.SinVida || estado.Luchador2.SinVida ? "K.O." : "Tiempo agotado";

            switch (estado.Resultado)
            {
                case ResultadoRonda.GanaJugador1:
                    return $"{motivo}: gana el jugador 1   (F3: nueva ronda)";
                case ResultadoRonda.GanaJugador2:
                    return $"{motivo}: gana el jugador 2   (F3: nueva ronda)";
                default:
                    return $"{motivo}: empate   (F3: nueva ronda)";
            }
        }

        private static string DescribirLuchador(string etiqueta, in EstadoLuchador luchador) =>
            $"{etiqueta}: {(MedidorVentaja.TieneControl(luchador) ? "LIBRE" : "sin control")}   {DescribirAccion(luchador)} ({luchador.FramesEnAccion} f)   hitstop {luchador.FramesHitstop}   aturdido {luchador.FramesAturdimiento}   combo {luchador.GolpesEnCombo}   vida {luchador.Vida}   búfer {DescribirBufer(luchador)}   pos {luchador.Posicion}   vel {luchador.Velocidad}   mira {(luchador.MiraDerecha ? "derecha" : "izquierda")}";

        private static string DescribirAccion(in EstadoLuchador luchador) =>
            luchador.Accion == AccionLuchador.Atacando ? $"Atacando {luchador.Ataque}" : luchador.Accion.ToString();

        private static string DescribirBufer(in EstadoLuchador luchador) =>
            luchador.TieneAtaqueEnBufer ? $"{luchador.AtaqueEnBufer} ({luchador.FramesBufer} f)" : "vacío";

        private static string DescribirMovimiento(string nombre, in DatosMovimiento movimiento) =>
            $"{nombre}: arranque {movimiento.Arranque}   activos {movimiento.Activos}   recuperación {movimiento.Recuperacion}   hitstop {movimiento.Hitstop}   daño {movimiento.Danio}   ventaja {movimiento.VentajaAlGolpear:+0;-0;0} al golpear, {movimiento.VentajaAlBloquear:+0;-0;0} al bloquear";

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
