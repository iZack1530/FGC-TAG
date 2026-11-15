using FgcTag.Nucleo.Combate;

namespace FgcTag.Arranque
{
    /// <summary>
    /// Mide en vivo la ventaja en frames de cada golpe: anota en qué frame recupera el control
    /// el atacante y en cuál el defensor, y resta. Es una herramienta de depuración; no forma
    /// parte de la simulación.
    /// </summary>
    public sealed class MedidorVentaja
    {
        private int _jugadorAtacante;
        private bool _bloqueado;
        private int _frameAtacante;
        private int _frameDefensor;

        /// <summary>Texto del último golpe medido, o vacío si todavía no hay ninguno.</summary>
        public string Resumen { get; private set; } = string.Empty;

        public void Observar(in EstadoPartida anterior, in EstadoPartida actual)
        {
            if (EmpiezaGolpe(anterior, actual))
            {
                return;
            }

            if (_jugadorAtacante == 0)
            {
                return;
            }

            var atacante = _jugadorAtacante == 1 ? actual.Luchador1 : actual.Luchador2;
            var defensor = _jugadorAtacante == 1 ? actual.Luchador2 : actual.Luchador1;

            if (_frameAtacante == 0 && TieneControl(atacante))
            {
                _frameAtacante = actual.Frame;
            }

            if (_frameDefensor == 0 && TieneControl(defensor))
            {
                _frameDefensor = actual.Frame;
            }

            if (_frameAtacante != 0 && _frameDefensor != 0)
            {
                var ventaja = _frameDefensor - _frameAtacante;
                var resultado = _bloqueado ? "bloqueado" : "golpe";
                Resumen = $"Último {resultado} de J{_jugadorAtacante}: ventaja {ventaja:+0;-0;0} frames (J{_jugadorAtacante} libre en el frame {_frameAtacante}, el rival en el {_frameDefensor})";
                _jugadorAtacante = 0;
            }
        }

        /// <summary>Un golpe empieza cuando el hitstop pasa de 0 a más de 0. Los intercambios no se miden.</summary>
        private bool EmpiezaGolpe(in EstadoPartida anterior, in EstadoPartida actual)
        {
            var golpea1 = !anterior.Luchador1.EnHitstop && actual.Luchador1.EnHitstop && EstadosAccion.EstaAtacando(actual.Luchador1.Accion);
            var golpea2 = !anterior.Luchador2.EnHitstop && actual.Luchador2.EnHitstop && EstadosAccion.EstaAtacando(actual.Luchador2.Accion);

            if (golpea1 == golpea2)
            {
                if (golpea1)
                {
                    _jugadorAtacante = 0;
                    Resumen = "Intercambio: ambos golpearon a la vez";
                }

                return golpea1;
            }

            _jugadorAtacante = golpea1 ? 1 : 2;
            _bloqueado = (golpea1 ? actual.Luchador2 : actual.Luchador1).Accion == AccionLuchador.Blockstun;
            _frameAtacante = 0;
            _frameDefensor = 0;

            return true;
        }

        /// <summary>Libre para actuar en el suelo: sin congelamiento y en una acción con control.</summary>
        public static bool TieneControl(in EstadoLuchador luchador) =>
            !luchador.EnHitstop && EstadosAccion.TieneControlEnTierra(luchador.Accion);
    }
}
