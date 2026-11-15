using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Estado completo de un combate por equipos en un frame: el duelo en pantalla más lo que
    /// cada equipo guarda fuera de ella. Copiable por valor para el rollback.
    /// </summary>
    public readonly struct EstadoCombateEquipos
    {
        /// <summary>El duelo entre los dos luchadores en pantalla. Su resultado solo indica si ese duelo está detenido.</summary>
        public readonly EstadoPartida Duelo;

        public readonly EstadoEquipo Equipo1;
        public readonly EstadoEquipo Equipo2;

        /// <summary>Resultado del combate: se decide cuando un equipo se queda sin luchadores o se agota el tiempo.</summary>
        public readonly ResultadoRonda Resultado;

        /// <summary>Frames transcurridos desde un K.O. que espera relevo; 0 si no hay ninguno pendiente.</summary>
        public readonly int FramesDesdeNocaut;

        public EstadoCombateEquipos(EstadoPartida duelo, EstadoEquipo equipo1, EstadoEquipo equipo2, ResultadoRonda resultado, int framesDesdeNocaut)
        {
            Duelo = duelo;
            Equipo1 = equipo1;
            Equipo2 = equipo2;
            Resultado = resultado;
            FramesDesdeNocaut = framesDesdeNocaut;
        }

        public bool Terminado => Resultado != ResultadoRonda.EnCurso;

        /// <summary>Un K.O. detuvo el duelo y se espera a que entre el siguiente luchador.</summary>
        public bool EsperandoRelevo => !Terminado && Duelo.RondaTerminada;

        /// <summary>Estado del frame 0: el primer luchador de cada equipo en pantalla y todos con la vida llena.</summary>
        public static EstadoCombateEquipos Crear(VectorFijo posicionJugador1, VectorFijo posicionJugador2, in ConfiguracionEquipos configuracion) =>
            new EstadoCombateEquipos(
                EstadoPartida.Crear(posicionJugador1, posicionJugador2, configuracion.DueloEntre(0, 0)),
                EstadoEquipo.Completo(configuracion.Equipo1),
                EstadoEquipo.Completo(configuracion.Equipo2),
                ResultadoRonda.EnCurso,
                0);

        public SumaVerificacion CalcularSumaVerificacion() =>
            Equipo2.AgregarA(Equipo1.AgregarA(Duelo.CalcularSumaVerificacion()))
                .Agregar((int)Resultado)
                .Agregar(FramesDesdeNocaut);
    }
}
