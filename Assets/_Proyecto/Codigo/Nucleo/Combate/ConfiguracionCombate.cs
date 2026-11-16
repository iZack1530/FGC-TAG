using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Reglas fijas de una partida: luchadores en pantalla, escenario, daño y duración de la ronda.</summary>
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

        public readonly ReglasDanio Danio;

        public readonly ReglasJuggle Juggle;

        public readonly ReglasVidaRecuperable VidaRecuperable;

        /// <summary>Duración del reloj de la ronda, en frames de 60 Hz.</summary>
        public readonly int FramesPorRonda;

        public readonly ReglasBarra Barra;

        public readonly ReglasAgarre Agarres;

        public ConfiguracionCombate(
            ParametrosLuchador jugador1,
            ParametrosLuchador jugador2,
            Fijo limiteIzquierdo,
            Fijo limiteDerecho,
            Fijo friccionRetroceso,
            int framesBuferEntradas,
            ReglasDanio danio,
            ReglasJuggle juggle,
            ReglasVidaRecuperable vidaRecuperable,
            ReglasBarra barra,
            ReglasAgarre agarres,
            int framesPorRonda)
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

            if (framesPorRonda < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesPorRonda), "La ronda dura al menos 1 frame.");
            }

            Jugador1 = jugador1;
            Jugador2 = jugador2;
            LimiteIzquierdo = limiteIzquierdo;
            LimiteDerecho = limiteDerecho;
            FriccionRetroceso = friccionRetroceso;
            FramesBuferEntradas = framesBuferEntradas;
            Danio = danio;
            Juggle = juggle;
            VidaRecuperable = vidaRecuperable;
            FramesPorRonda = framesPorRonda;
            Barra = barra;
            Agarres = agarres;
        }

        /// <summary>Las mismas reglas con otros dos luchadores en pantalla.</summary>
        public ConfiguracionCombate ConLuchadores(in ParametrosLuchador jugador1, in ParametrosLuchador jugador2) =>
            new ConfiguracionCombate(
                jugador1,
                jugador2,
                LimiteIzquierdo,
                LimiteDerecho,
                FriccionRetroceso,
                FramesBuferEntradas,
                Danio,
                Juggle,
                VidaRecuperable,
                Barra,
                Agarres,
                FramesPorRonda);
    }
}
