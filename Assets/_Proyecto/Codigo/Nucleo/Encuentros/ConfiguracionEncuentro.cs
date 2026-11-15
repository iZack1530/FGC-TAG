using System;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Encuentros
{
    /// <summary>Reglas fijas de un encuentro: el combate por equipos que se repite ronda tras ronda y cuántas rondas hay que ganar.</summary>
    public readonly struct ConfiguracionEncuentro
    {
        public readonly ConfiguracionEquipos Equipos;

        /// <summary>Rondas que hay que ganar para llevarse el encuentro: 2 en un encuentro al mejor de 3.</summary>
        public readonly int VictoriasNecesarias;

        /// <summary>Frames entre el final de una ronda y el comienzo de la siguiente.</summary>
        public readonly int FramesEntreRondas;

        public readonly VectorFijo PosicionInicial1;
        public readonly VectorFijo PosicionInicial2;

        public ConfiguracionEncuentro(
            ConfiguracionEquipos equipos,
            int victoriasNecesarias,
            int framesEntreRondas,
            VectorFijo posicionInicial1,
            VectorFijo posicionInicial2)
        {
            if (victoriasNecesarias < 1 || framesEntreRondas < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(victoriasNecesarias), "Hace falta al menos 1 victoria y 1 frame de pausa entre rondas.");
            }

            Equipos = equipos;
            VictoriasNecesarias = victoriasNecesarias;
            FramesEntreRondas = framesEntreRondas;
            PosicionInicial1 = posicionInicial1;
            PosicionInicial2 = posicionInicial2;
        }

        /// <summary>Ronda recién empezada: los dos equipos completos, en sus posiciones iniciales y con el reloj lleno.</summary>
        public EstadoCombateEquipos RondaNueva() =>
            EstadoCombateEquipos.Crear(PosicionInicial1, PosicionInicial2, Equipos);
    }
}
