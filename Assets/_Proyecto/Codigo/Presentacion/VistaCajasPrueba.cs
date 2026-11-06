using FgcTag.Nucleo.Combate;
using UnityEngine;

namespace FgcTag.Presentacion
{
    /// <summary>
    /// Representación provisional de la fase 1: cada luchador es una caja del tamaño de su caja
    /// de empuje, con una «nariz» que indica hacia dónde mira. Solo lee el estado; nunca lo modifica.
    /// </summary>
    public sealed class VistaCajasPrueba
    {
        private const float GrosorProfundidad = 0.6f;
        private const float TamanoNariz = 0.18f;
        private const float FactorAlturaAgachado = 0.55f;

        private static readonly Color ColorJugador1 = new Color(0.25f, 0.55f, 0.95f);
        private static readonly Color ColorJugador2 = new Color(0.95f, 0.35f, 0.3f);
        private static readonly Color ColorPiso = new Color(0.35f, 0.35f, 0.38f);

        private readonly GameObject _raiz;
        private readonly Material _materialPiso;
        private readonly LuchadorVisual _luchador1;
        private readonly LuchadorVisual _luchador2;

        public VistaCajasPrueba(Transform padre, in ConfiguracionCombate configuracion)
        {
            _raiz = new GameObject("VistaCajasPrueba");
            _raiz.transform.SetParent(padre, false);

            _materialPiso = CrearPiso(_raiz.transform, configuracion);
            _luchador1 = new LuchadorVisual("Jugador1", _raiz.transform, configuracion.Jugador1, ColorJugador1);
            _luchador2 = new LuchadorVisual("Jugador2", _raiz.transform, configuracion.Jugador2, ColorJugador2);
        }

        public void Mostrar(in EstadoPartida estado)
        {
            _luchador1.Mostrar(estado.Luchador1);
            _luchador2.Mostrar(estado.Luchador2);
        }

        public void Destruir()
        {
            _luchador1.DestruirMaterial();
            _luchador2.DestruirMaterial();
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

        private sealed class LuchadorVisual
        {
            private readonly Transform _raiz;
            private readonly Transform _cuerpo;
            private readonly Material _material;
            private readonly float _ancho;
            private readonly float _alto;

            public LuchadorVisual(string nombre, Transform padre, in ParametrosLuchador parametros, Color color)
            {
                _ancho = parametros.MedioAnchoEmpuje.AFlotante() * 2f;
                _alto = parametros.MedioAltoEmpuje.AFlotante() * 2f;

                _raiz = new GameObject(nombre).transform;
                _raiz.SetParent(padre, false);

                var cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cuerpo.name = "Cuerpo";
                _cuerpo = cuerpo.transform;
                _cuerpo.SetParent(_raiz, false);

                var renderer = cuerpo.GetComponent<Renderer>();
                _material = renderer.material;
                _material.color = color;

                var nariz = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nariz.name = "Nariz";
                nariz.transform.SetParent(_raiz, false);
                nariz.transform.localPosition = new Vector3(_ancho / 2f + TamanoNariz / 2f, _alto * 0.8f, 0f);
                nariz.transform.localScale = Vector3.one * TamanoNariz;
                nariz.GetComponent<Renderer>().sharedMaterial = _material;

                AjustarCuerpo(_alto);
            }

            public void Mostrar(in EstadoLuchador estado)
            {
                _raiz.localPosition = new Vector3(estado.Posicion.X.AFlotante(), estado.Posicion.Y.AFlotante(), 0f);
                _raiz.localScale = new Vector3(estado.MiraDerecha ? 1f : -1f, 1f, 1f);
                AjustarCuerpo(estado.Accion == AccionLuchador.Agachado ? _alto * FactorAlturaAgachado : _alto);
            }

            public void DestruirMaterial() => Object.Destroy(_material);

            private void AjustarCuerpo(float altura)
            {
                _cuerpo.localPosition = new Vector3(0f, altura / 2f, 0f);
                _cuerpo.localScale = new Vector3(_ancho, altura, GrosorProfundidad);
            }
        }
    }
}
