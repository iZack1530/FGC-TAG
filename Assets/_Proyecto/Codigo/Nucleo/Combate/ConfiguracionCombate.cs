using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Reglas fijas de una partida: luchadores en pantalla, paredes del escenario y fricción.</summary>
    public readonly struct ConfiguracionCombate
    {
        public readonly ParametrosLuchador Jugador1;
        public readonly ParametrosLuchador Jugador2;
        public readonly Fijo LimiteIzquierdo;
        public readonly Fijo LimiteDerecho;

        /// <summary>Velocidad de retroceso que un luchador aturdido pierde cada frame hasta detenerse.</summary>
        public readonly Fijo FriccionRetroceso;

        /// <summary>
        /// Frames durante los que un ataque pulsado se recuerda (contando el de la pulsación) para
        /// salir en cuanto el luchador pueda actuar. El hitstop no consume estos frames.
        /// </summary>
        public readonly int FramesBuferEntradas;

        public ConfiguracionCombate(
            ParametrosLuchador jugador1,
            ParametrosLuchador jugador2,
            Fijo limiteIzquierdo,
            Fijo limiteDerecho,
            Fijo friccionRetroceso,
            int framesBuferEntradas)
        {
            var anchoMinimo = (jugador1.MedioAnchoMaximo + jugador2.MedioAnchoMaximo) * 2;

            if (limiteDerecho - limiteIzquierdo < anchoMinimo)
            {
                throw new ArgumentException("El escenario debe caber a ambos luchadores lado a lado.", nameof(limiteDerecho));
            }

            if (friccionRetroceso <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(friccionRetroceso), "La fricción del retroceso debe ser positiva.");
            }

            if (framesBuferEntradas < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesBuferEntradas), "El búfer de entradas dura al menos 1 frame.");
            }

            Jugador1 = jugador1;
            Jugador2 = jugador2;
            LimiteIzquierdo = limiteIzquierdo;
            LimiteDerecho = limiteDerecho;
            FriccionRetroceso = friccionRetroceso;
            FramesBuferEntradas = framesBuferEntradas;
        }
    }
}
