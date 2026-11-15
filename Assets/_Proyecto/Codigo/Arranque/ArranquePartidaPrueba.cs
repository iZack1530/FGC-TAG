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
    /// <remarks>F1 pausa la simulación y F2 la avanza de un frame en un frame.</remarks>
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
            if (!_mostrarDepuracion)
            {
                return;
            }

            if (_estiloTexto == null)
            {
                _estiloTexto = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            }

            var modo = _pausado ? "EN PAUSA (F2 avanza un frame, F1 reanuda)" : "F1 pausa";

            GUILayout.BeginArea(new Rect(12, 12, Screen.width - 24, 190), GUI.skin.box);
            GUILayout.Label($"Frame {_estado.Frame}   Suma {_estado.CalcularSumaVerificacion()}   {modo}", _estiloTexto);
            GUILayout.Label(DescribirLuchador("J1", _estado.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirLuchador("J2", _estado.Luchador2), _estiloTexto);
            GUILayout.Label(DescribirMovimiento("Golpe débil", _configuracion.Jugador1.GolpeDebil), _estiloTexto);
            GUILayout.Label(_medidor.Resumen, _estiloTexto);
            GUILayout.EndArea();
        }

        private static string DescribirLuchador(string etiqueta, in EstadoLuchador luchador) =>
            $"{etiqueta}: {(MedidorVentaja.TieneControl(luchador) ? "LIBRE" : "sin control")}   {luchador.Accion} ({luchador.FramesEnAccion} f)   hitstop {luchador.FramesHitstop}   aturdido {luchador.FramesAturdimiento}   pos {luchador.Posicion}   vel {luchador.Velocidad}   mira {(luchador.MiraDerecha ? "derecha" : "izquierda")}";

        private static string DescribirMovimiento(string nombre, in DatosMovimiento movimiento) =>
            $"{nombre}: arranque {movimiento.Arranque}   activos {movimiento.Activos}   recuperación {movimiento.Recuperacion}   hitstop {movimiento.Hitstop}   ventaja {movimiento.VentajaAlGolpear:+0;-0;0} al golpear, {movimiento.VentajaAlBloquear:+0;-0;0} al bloquear";

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
