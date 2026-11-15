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

        /// <summary>Duración del reloj de la ronda, en frames de 60 Hz.</summary>
        public readonly int FramesPorRonda;

        /// <summary>Frames que tarda en poder actuar un luchador que acaba de entrar a la pantalla.</summary>
        public readonly int FramesEntrada;

        public ConfiguracionCombate(
            ParametrosLuchador jugador1,
            ParametrosLuchador jugador2,
            Fijo limiteIzquierdo,
            Fijo limiteDerecho,
            Fijo friccionRetroceso,
            int framesBuferEntradas,
            ReglasDanio danio,
            ReglasJuggle juggle,
            int framesPorRonda,
            int framesEntrada)
        {
            if (framesEntrada < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(framesEntrada), "La entrada dura al menos 1 frame.");
            }

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
            FramesPorRonda = framesPorRonda;
            FramesEntrada = framesEntrada;
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
                FramesPorRonda,
                FramesEntrada);
    }
}
