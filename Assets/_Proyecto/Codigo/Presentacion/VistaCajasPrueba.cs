using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;
using UnityEngine;

namespace FgcTag.Presentacion
{
    /// <summary>
    /// Representación provisional de la fase 1: cada luchador es su caja de empuje real (la misma
    /// que usa la simulación), con una «nariz» que indica hacia dónde mira. La caja de golpe se
    /// dibuja en rojo durante los frames activos, y la de agarre en verde; mientras dura el hitstop, el atacante se pinta
    /// de blanco y el golpeado de negro: la versión mínima del impact frame. Solo lee el estado.
    /// </summary>
    public sealed class VistaCajasPrueba
    {
        private const float GrosorProfundidad = 0.6f;
        private const float TamanoNariz = 0.18f;

        /// <summary>Un tono por miembro del equipo, para ver quién está en pantalla: azules el jugador 1 y rojizos el jugador 2.</summary>
        private static readonly Color[] ColoresJugador1 =
        {
            new Color(0.25f, 0.55f, 0.95f),
            new Color(0.2f, 0.8f, 0.85f),
            new Color(0.45f, 0.4f, 0.95f),
        };

        private static readonly Color[] ColoresJugador2 =
        {
            new Color(0.95f, 0.35f, 0.3f),
            new Color(0.95f, 0.6f, 0.2f),
            new Color(0.9f, 0.3f, 0.65f),
        };
        private static readonly Color ColorPiso = new Color(0.35f, 0.35f, 0.38f);
        private static readonly Color ColorCajaGolpe = new Color(1f, 0.15f, 0.1f);
        private static readonly Color ColorCajaAgarre = new Color(0.15f, 0.9f, 0.25f);
        private static readonly Color ColorImpactoAtacante = new Color(1f, 1f, 1f);
        private static readonly Color ColorImpactoGolpeado = new Color(0.05f, 0.05f, 0.05f);

        private readonly GameObject _raiz;
        private readonly Material _materialPiso;
        private readonly LuchadorVisual _luchador1;
        private readonly LuchadorVisual _luchador2;
        private readonly AsistenteVisual _asistente1;
        private readonly AsistenteVisual _asistente2;
        private readonly SalienteVisual _saliente1;
        private readonly SalienteVisual _saliente2;

        public VistaCajasPrueba(Transform padre, in ConfiguracionCombate configuracion)
        {
            _raiz = new GameObject("VistaCajasPrueba");
            _raiz.transform.SetParent(padre, false);

            _materialPiso = CrearPiso(_raiz.transform, configuracion);
            _luchador1 = new LuchadorVisual("Jugador1", _raiz.transform, configuracion.Jugador1, ColoresJugador1[0]);
            _luchador2 = new LuchadorVisual("Jugador2", _raiz.transform, configuracion.Jugador2, ColoresJugador2[0]);
            _asistente1 = new AsistenteVisual("Asistente1", _raiz.transform);
            _asistente2 = new AsistenteVisual("Asistente2", _raiz.transform);
            _saliente1 = new SalienteVisual("Saliente1", _raiz.transform);
            _saliente2 = new SalienteVisual("Saliente2", _raiz.transform);
        }

        /// <summary>Dibuja al luchador de cada equipo que se marcha tras un cambio, mientras dura su salto de salida.</summary>
        public void MostrarSalientes(in EstadoEquipo equipo1, in EstadoEquipo equipo2, in ConfiguracionEquipos configuracion)
        {
            _saliente1.Mostrar(equipo1.Saliente, configuracion.Equipo1, ColoresJugador1, configuracion.VelocidadSalida);
            _saliente2.Mostrar(equipo2.Saliente, configuracion.Equipo2, ColoresJugador2, configuracion.VelocidadSalida);
        }

        /// <summary>Dibuja al asistente de cada equipo, si lo hay, con el tono de ese miembro y su caja de golpe.</summary>
        public void MostrarAsistentes(in EstadoEquipo equipo1, in EstadoEquipo equipo2, in ConfiguracionEquipos configuracion)
        {
            _asistente1.Mostrar(equipo1.Asistente, configuracion.Equipo1, ColoresJugador1, configuracion.FramesLlegadaAsistencia);
            _asistente2.Mostrar(equipo2.Asistente, configuracion.Equipo2, ColoresJugador2, configuracion.FramesLlegadaAsistencia);
        }

        /// <param name="reglas">Configuración del duelo entre los dos luchadores que están en pantalla.</param>
        /// <param name="miembro1">Posición en su equipo, de 0 a 2, del luchador del jugador 1; decide su color.</param>
        /// <param name="miembro2">Lo mismo para el jugador 2.</param>
        public void Mostrar(in EstadoPartida estado, in ConfiguracionCombate reglas, int miembro1, int miembro2)
        {
            _luchador1.Mostrar(estado.Luchador1, reglas.Jugador1, ColoresJugador1[miembro1]);
            _luchador2.Mostrar(estado.Luchador2, reglas.Jugador2, ColoresJugador2[miembro2]);
        }

        public void Destruir()
        {
            _luchador1.DestruirMateriales();
            _luchador2.DestruirMateriales();
            _asistente1.DestruirMateriales();
            _asistente2.DestruirMateriales();
            _saliente1.DestruirMateriales();
            _saliente2.DestruirMateriales();
            Object.Destroy(_materialPiso);
            Object.Destroy(_raiz);
        }

        /// <summary>Usa una instancia del material para no teñir el material por defecto del proyecto.</summary>
        private static Material CrearPiso(Transform padre, in ConfiguracionCombate configuracion)
        {
            var izquierda = configuracion.LimiteIzquierdo.AFlotante();
            var derecha = configuracion.LimiteDerecho.AFlotante();
            var piso = GameObject.CreatePrimitive(PrimitiveType.Cube);

            piso.name = "Piso";
            piso.transform.SetParent(padre, false);
            piso.transform.localPosition = new Vector3((izquierda + derecha) / 2f, -0.05f, 0f);
            piso.transform.localScale = new Vector3(derecha - izquierda, 0.1f, 2f);

            var material = piso.GetComponent<Renderer>().material;
            material.color = ColorPiso;

            return material;
        }

        /// <summary>
        /// Asistente: su caja de daño real, detrás del plano de los luchadores. Con su tono mientras
        /// ataca, aclarada mientras llega o se retira y en negro si lo golpean; su caja de golpe se
        /// dibuja en rojo durante los frames activos.
        /// </summary>
        private sealed class AsistenteVisual
        {
            private const float AclaradoFueraDelAtaque = 0.45f;

            private readonly GameObject _cuerpo;
            private readonly GameObject _cajaGolpe;
            private readonly Material _material;
            private readonly Material _materialCajaGolpe;

            public AsistenteVisual(string nombre, Transform padre)
            {
                _cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _cuerpo.name = nombre;
                _cuerpo.transform.SetParent(padre, false);
                _material = _cuerpo.GetComponent<Renderer>().material;
                _cuerpo.SetActive(false);

                _cajaGolpe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _cajaGolpe.name = nombre + "CajaGolpe";
                _cajaGolpe.transform.SetParent(padre, false);
                _materialCajaGolpe = _cajaGolpe.GetComponent<Renderer>().material;
                _materialCajaGolpe.color = ColorCajaGolpe;
                _cajaGolpe.SetActive(false);
            }

            public void Mostrar(in EstadoAsistente asistente, in ParametrosEquipo equipo, Color[] colores, int framesLlegada)
            {
                _cuerpo.SetActive(asistente.EnPantalla);

                if (!asistente.EnPantalla)
                {
                    _cajaGolpe.SetActive(false);
                    return;
                }

                var parametros = equipo.Obtener(asistente.Miembro);
                var movimiento = parametros.Movimientos.Asistencia;
                var frameDelAtaque = asistente.Frames - framesLlegada;
                var atacando = !asistente.FueGolpeado && frameDelAtaque >= 0 && frameDelAtaque < movimiento.DuracionTotal;
                var cajaDanio = SimuladorEquipos.CajaDanioDelAsistente(asistente, parametros);
                var ancho = cajaDanio.MedioAncho.AFlotante() * 2f;
                var alto = cajaDanio.MedioAlto.AFlotante() * 2f;

                _material.color = asistente.FueGolpeado
                    ? ColorImpactoGolpeado
                    : atacando ? colores[asistente.Miembro] : Color.Lerp(colores[asistente.Miembro], Color.white, AclaradoFueraDelAtaque);
                _cuerpo.transform.localPosition = new Vector3(cajaDanio.Centro.X.AFlotante(), cajaDanio.Centro.Y.AFlotante(), GrosorProfundidad);
                _cuerpo.transform.localScale = new Vector3(ancho, alto, GrosorProfundidad);

                var golpeando = atacando && movimiento.EstaActivo(frameDelAtaque);
                _cajaGolpe.SetActive(golpeando);

                if (!golpeando)
                {
                    return;
                }

                var caja = SimuladorEquipos.CajaGolpeDelAsistente(asistente, movimiento);
                _cajaGolpe.transform.localPosition = new Vector3(caja.Centro.X.AFlotante(), caja.Centro.Y.AFlotante(), -GrosorProfundidad / 2f);
                _cajaGolpe.transform.localScale = new Vector3(caja.MedioAncho.AFlotante() * 2f, caja.MedioAlto.AFlotante() * 2f, GrosorProfundidad / 2f);
            }

            public void DestruirMateriales()
            {
                Object.Destroy(_material);
                Object.Destroy(_materialCajaGolpe);
            }
        }

        /// <summary>Coloca un cubo sobre una caja de colisión que ya está en coordenadas de escenario, o lo oculta si no hay caja.</summary>
        private static void MostrarCaja(GameObject cubo, CajaColision? caja)
        {
            cubo.SetActive(caja.HasValue);

            if (!caja.HasValue)
            {
                return;
            }

            var centro = caja.Value.Centro;
            cubo.transform.localPosition = new Vector3(centro.X.AFlotante(), centro.Y.AFlotante(), -GrosorProfundidad / 2f);
            cubo.transform.localScale = new Vector3(caja.Value.MedioAncho.AFlotante() * 2f, caja.Value.MedioAlto.AFlotante() * 2f, GrosorProfundidad / 2f);
        }

        private static GameObject CrearCaja(string nombre, Transform padre, Color color, out Material material)
        {
            var cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubo.name = nombre;
            cubo.transform.SetParent(padre, false);
            material = cubo.GetComponent<Renderer>().material;
            material.color = color;
            cubo.SetActive(false);

            return cubo;
        }

        /// <summary>Luchador que se marcha: una caja con su tono, oscurecida, que se aleja hacia atrás y hacia arriba.</summary>
        private sealed class SalienteVisual
        {
            private const float Oscurecido = 0.4f;

            private readonly GameObject _cuerpo;
            private readonly Material _material;

            public SalienteVisual(string nombre, Transform padre)
            {
                _cuerpo = CrearCaja(nombre, padre, Color.white, out _material);
            }

            public void Mostrar(in EstadoSaliente saliente, in ParametrosEquipo equipo, Color[] colores, VectorFijo velocidadSalida)
            {
                _cuerpo.SetActive(saliente.EnPantalla);

                if (!saliente.EnPantalla)
                {
                    return;
                }

                var parametros = equipo.Obtener(saliente.Miembro);
                var posicion = saliente.Posicion(velocidadSalida);
                var ancho = parametros.MedioAnchoEmpujeAire.AFlotante() * 2f;
                var alto = parametros.MedioAltoEmpujeAire.AFlotante() * 2f;

                _material.color = Color.Lerp(colores[saliente.Miembro], Color.black, Oscurecido);
                _cuerpo.transform.localPosition = new Vector3(posicion.X.AFlotante(), posicion.Y.AFlotante() + alto / 2f, GrosorProfundidad);
                _cuerpo.transform.localScale = new Vector3(ancho, alto, GrosorProfundidad);
            }

            public void DestruirMateriales() => Object.Destroy(_material);
        }

        private sealed class LuchadorVisual
        {
            private ParametrosLuchador _parametros;
            private readonly Transform _raiz;
            private readonly Transform _cuerpo;
            private readonly Transform _nariz;
            private readonly GameObject _cajaGolpe;
            private readonly Material _material;
            private readonly GameObject _cajaAgarre;
            private readonly Material _materialCajaGolpe;
            private readonly Material _materialCajaAgarre;
            private Color _color;

            public LuchadorVisual(string nombre, Transform padre, in ParametrosLuchador parametros, Color color)
            {
                _parametros = parametros;
                _color = color;

                _raiz = new GameObject(nombre).transform;
                _raiz.SetParent(padre, false);

                var cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cuerpo.name = "Cuerpo";
                _cuerpo = cuerpo.transform;
                _cuerpo.SetParent(_raiz, false);

                _material = cuerpo.GetComponent<Renderer>().material;
                _material.color = color;

                var nariz = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nariz.name = "Nariz";
                _nariz = nariz.transform;
                _nariz.SetParent(_raiz, false);
                _nariz.localScale = Vector3.one * TamanoNariz;
                nariz.GetComponent<Renderer>().sharedMaterial = _material;

                _cajaGolpe = CrearCaja(nombre + "CajaGolpe", padre, ColorCajaGolpe, out _materialCajaGolpe);
                _cajaAgarre = CrearCaja(nombre + "CajaAgarre", padre, ColorCajaAgarre, out _materialCajaAgarre);
            }

            /// <summary>Los parámetros y el color llegan en cada llamada porque el luchador en pantalla cambia con el tag.</summary>
            public void Mostrar(in EstadoLuchador estado, in ParametrosLuchador parametros, Color color)
            {
                _parametros = parametros;
                _color = color;

                var caja = _parametros.CajaEmpuje(estado);
                var ancho = caja.MedioAncho.AFlotante() * 2f;
                var alto = caja.MedioAlto.AFlotante() * 2f * FactorPose(estado.Accion);

                _raiz.localPosition = new Vector3(estado.Posicion.X.AFlotante(), estado.Posicion.Y.AFlotante(), 0f);
                _raiz.localScale = new Vector3(estado.MiraDerecha ? 1f : -1f, 1f, 1f);

                _cuerpo.localPosition = new Vector3(0f, alto / 2f, 0f);
                _cuerpo.localScale = new Vector3(ancho, alto, GrosorProfundidad);
                _nariz.localPosition = new Vector3(ancho / 2f + TamanoNariz / 2f, alto * 0.8f, 0f);

                _material.color = ColorSegunImpacto(estado);
                MostrarCaja(_cajaGolpe, _parametros.CajaGolpeActiva(estado));
                MostrarCaja(_cajaAgarre, _parametros.CajaAgarreActiva(estado));
            }

            public void DestruirMateriales()
            {
                Object.Destroy(_material);
                Object.Destroy(_materialCajaGolpe);
                Object.Destroy(_materialCajaAgarre);
            }

            private Color ColorSegunImpacto(in EstadoLuchador estado)
            {
                if (!estado.EnHitstop)
                {
                    return _color;
                }

                return EstadosAccion.EstaAtacando(estado.Accion) ? ColorImpactoAtacante : ColorImpactoGolpeado;
            }

            /// <summary>
            /// Pista visual de la pose (agacharse, cargar el salto, amortiguar la caída). Solo
            /// cambia el dibujo; la caja de la simulación no se modifica.
            /// </summary>
            private static float FactorPose(AccionLuchador accion)
            {
                switch (accion)
                {
                    case AccionLuchador.Agachado:
                    case AccionLuchador.AtacandoAgachado:
                    case AccionLuchador.BlockstunAgachado:
                        return 0.55f;
                    case AccionLuchador.PreSalto:
                        return 0.85f;
                    case AccionLuchador.Aterrizaje:
                        return 0.8f;
                    case AccionLuchador.Agarrado:
                        return 0.7f;
                    case AccionLuchador.Dash:
                        return 0.9f;
                    case AccionLuchador.Noqueado:
                    case AccionLuchador.Derribado:
                        return 0.25f;
                    default:
                        return 1f;
                }
            }
        }
    }
}
