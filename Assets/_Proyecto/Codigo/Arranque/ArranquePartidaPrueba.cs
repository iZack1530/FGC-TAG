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

        private void Awake()
        {
            _configuracion = DatosPrueba.ConfiguracionCajasGrises;
            _estado = DatosPrueba.EstadoInicial;
            _vista = new VistaCajasPrueba(transform, _configuracion);
            _vista.Mostrar(_estado);

            if (_colocarCamara)
            {
                ColocarCamara();
            }
        }

        private void Update()
        {
            _acumulado += Time.unscaledDeltaTime;
            var pasos = 0;

            while (_acumulado >= SegundosPorFrame && pasos < MaximoPasosPorActualizacion)
            {
                var entradas = new EntradasFrame(LectorEntradas.LeerJugador1(), LectorEntradas.LeerJugador2());
                _estado = SimuladorCombate.Avanzar(_estado, entradas, _configuracion);
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

        private void OnGUI()
        {
            if (!_mostrarDepuracion)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12, 12, 520, 120), GUI.skin.box);
            GUILayout.Label($"Frame {_estado.Frame}   Suma {_estado.CalcularSumaVerificacion()}");
            GUILayout.Label(DescribirLuchador("J1", _estado.Luchador1));
            GUILayout.Label(DescribirLuchador("J2", _estado.Luchador2));
            GUILayout.EndArea();
        }

        private static string DescribirLuchador(string etiqueta, in EstadoLuchador luchador) =>
            $"{etiqueta}: x {luchador.Posicion.X}   {luchador.Accion} ({luchador.FramesEnAccion} f)   mira {(luchador.MiraDerecha ? "derecha" : "izquierda")}";

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
