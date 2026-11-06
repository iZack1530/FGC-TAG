using System;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Reglas fijas de una partida: luchadores en pantalla y paredes del escenario.</summary>
    public readonly struct ConfiguracionCombate
    {
        public readonly ParametrosLuchador Jugador1;
        public readonly ParametrosLuchador Jugador2;
        public readonly Fijo LimiteIzquierdo;
        public readonly Fijo LimiteDerecho;

        public ConfiguracionCombate(ParametrosLuchador jugador1, ParametrosLuchador jugador2, Fijo limiteIzquierdo, Fijo limiteDerecho)
        {
            var anchoMinimo = (jugador1.MedioAnchoEmpuje + jugador2.MedioAnchoEmpuje) * 2;

            if (limiteDerecho - limiteIzquierdo < anchoMinimo)
            {
                throw new ArgumentException("El escenario debe caber a ambos luchadores lado a lado.", nameof(limiteDerecho));
            }

            Jugador1 = jugador1;
            Jugador2 = jugador2;
            LimiteIzquierdo = limiteIzquierdo;
            LimiteDerecho = limiteDerecho;
        }
    }
}
