using FgcTag.Contenido;
using FgcTag.Entrada;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Encuentros;
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

        private ConfiguracionEncuentro _configuracionEncuentro;
        private ConfiguracionEquipos _configuracionEquipos;
        private EstadoEncuentro _encuentro;
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
        private static readonly Color ColorVidaJugador1 = new Color(0.25f, 0.55f, 0.95f);
        private static readonly Color ColorVidaJugador2 = new Color(0.95f, 0.35f, 0.3f);

        /// <summary>El duelo entre los dos luchadores que están en pantalla.</summary>
        private EstadoPartida Duelo => Combate.Duelo;

        /// <summary>El combate por equipos de la ronda actual.</summary>
        private EstadoCombateEquipos Combate => _encuentro.Ronda;

        /// <summary>Reglas del duelo con los parámetros de los dos luchadores activos.</summary>
        private ConfiguracionCombate ReglasDuelo =>
            _configuracionEquipos.DueloEntre(Combate.Equipo1.IndiceActivo, Combate.Equipo2.IndiceActivo);

        private void Awake()
        {
            _configuracionEncuentro = DatosPrueba.EncuentroCajasGrises;
            _configuracionEquipos = _configuracionEncuentro.Equipos;
            _encuentro = DatosPrueba.EncuentroInicial;
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
                _encuentro = DatosPrueba.EncuentroInicial;
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
            _vista.Mostrar(Duelo, ReglasDuelo, Combate.Equipo1.IndiceActivo, Combate.Equipo2.IndiceActivo);
            _vista.MostrarAsistentes(Combate.Equipo1, Combate.Equipo2, _configuracionEquipos);
        }

        private void OnDestroy() => _vista?.Destruir();

        private void AvanzarUnFrame()
        {
            var entradas = new EntradasFrame(LectorEntradas.LeerJugador1(), LectorEntradas.LeerJugador2());
            var anterior = Duelo;
            _encuentro = SimuladorEncuentro.Avanzar(_encuentro, entradas, _configuracionEncuentro);
            _medidor.Observar(anterior, Duelo);

            if (EstadosAccion.EstaAtacando(Duelo.Luchador1.Accion))
            {
                _ultimoAtaqueJugador1 = DescribirMovimiento(
                    NombreAtaque(Duelo.Luchador1),
                    ReglasDuelo.Jugador1.MovimientoEnCurso(Duelo.Luchador1));
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

            GUILayout.BeginArea(new Rect(12, 12, Screen.width - 24, 255), GUI.skin.box);
            GUILayout.Label($"Frame {Duelo.Frame}   Suma {_encuentro.CalcularSumaVerificacion()}   {modo}", _estiloTexto);
            GUILayout.Label(DescribirLuchador("J1", Duelo.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirLuchador("J2", Duelo.Luchador2), _estiloTexto);
            GUILayout.Label(DescribirEquipo("Equipo J1", Combate.Equipo1, Duelo.Luchador1), _estiloTexto);
            GUILayout.Label(DescribirEquipo("Equipo J2", Combate.Equipo2, Duelo.Luchador2), _estiloTexto);
            GUILayout.Label($"Último ataque de J1: {_ultimoAtaqueJugador1}", _estiloTexto);
            GUILayout.Label(_medidor.Resumen, _estiloTexto);
            GUILayout.Label($"Daño en combo: -{reglas.Danio.ReduccionPorGolpe}% por golpe, mínimo {reglas.Danio.PorcentajeMinimo}%; bloqueado {reglas.Danio.PorcentajeAlBloquear}%   F3 encuentro nuevo", _estiloTexto);
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

            var anchoBarra = (Screen.width - 2f * Margen - AnchoReloj) / 2f;
            var y = Screen.height - Margen - AltoBarra;

            DibujarVidaEquipo(Margen, y, anchoBarra, AltoBarra, Combate.Equipo1, Duelo.Luchador1, _configuracionEquipos.Equipo1, ColorVidaJugador1, false);
            DibujarVidaEquipo(Screen.width - Margen - anchoBarra, y, anchoBarra, AltoBarra, Combate.Equipo2, Duelo.Luchador2, _configuracionEquipos.Equipo2, ColorVidaJugador2, true);

            var segundos = (Duelo.FramesRestantes + ConstantesSimulacion.FramesPorSegundoLogica - 1) / ConstantesSimulacion.FramesPorSegundoLogica;
            GUI.Label(new Rect(Margen + anchoBarra, y - 10f, AnchoReloj, AltoBarra + 20f), segundos.ToString(), _estiloAviso);

            GUI.Label(
                new Rect(0f, y - 62f, Screen.width, 30f),
                $"Ronda {_encuentro.NumeroDeRonda}   rondas ganadas: J1 {_encuentro.Victorias1} - {_encuentro.Victorias2} J2   (gana quien llegue a {_configuracionEncuentro.VictoriasNecesarias})",
                _estiloMarcador);

            if (_encuentro.Terminado)
            {
                GUI.Label(new Rect(0f, y - 130f, Screen.width, 60f), DescribirFinal(_encuentro.Resultado), _estiloAviso);
            }
            else if (_encuentro.EntreRondas)
            {
                GUI.Label(new Rect(0f, y - 130f, Screen.width, 60f), DescribirResultado(Combate.Resultado, Duelo.FramesRestantes == 0), _estiloAviso);
            }
            else if (Combate.EsperandoRelevo)
            {
                GUI.Label(new Rect(0f, y - 130f, Screen.width, 60f), "K.O.: entra el siguiente luchador", _estiloAviso);
            }
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
                    return $"{motivo}: la ronda es del jugador 1";
                case ResultadoRonda.GanaJugador2:
                    return $"{motivo}: la ronda es del jugador 2";
                default:
                    return $"{motivo}: ronda empatada";
            }
        }

        private static string DescribirFinal(ResultadoRonda resultado)
        {
            switch (resultado)
            {
                case ResultadoRonda.GanaJugador1:
                    return "Encuentro para el jugador 1   (F3: encuentro nuevo)";
                case ResultadoRonda.GanaJugador2:
                    return "Encuentro para el jugador 2   (F3: encuentro nuevo)";
                default:
                    return "Encuentro empatado   (F3: encuentro nuevo)";
            }
        }

        private static string DescribirLuchador(string etiqueta, in EstadoLuchador luchador) =>
            $"{etiqueta}: {(MedidorVentaja.TieneControl(luchador) ? "LIBRE" : "sin control")}   {DescribirAccion(luchador)} ({luchador.FramesEnAccion} f)   hitstop {luchador.FramesHitstop}   aturdido {luchador.FramesAturdimiento}   combo {luchador.GolpesEnCombo}   vida {luchador.Vida}   búfer {DescribirBufer(luchador)}   pos {luchador.Posicion}   vel {luchador.Velocidad}   mira {(luchador.MiraDerecha ? "derecha" : "izquierda")}";

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
