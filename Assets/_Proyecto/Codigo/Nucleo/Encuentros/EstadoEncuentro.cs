using FgcTag.Nucleo.Combate;
using FgcTag.Nucleo.Equipos;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Encuentros
{
    /// <summary>
    /// Estado completo de un encuentro en un frame: la ronda en curso y el marcador de rondas.
    /// Copiable por valor para el rollback.
    /// </summary>
    public readonly struct EstadoEncuentro
    {
        /// <summary>El combate por equipos de la ronda actual.</summary>
        public readonly EstadoCombateEquipos Ronda;

        public readonly int Victorias1;
        public readonly int Victorias2;

        /// <summary>Número de la ronda actual, empezando en 1.</summary>
        public readonly int NumeroDeRonda;

        /// <summary>Frames transcurridos desde que terminó la ronda actual; 0 mientras se juega.</summary>
        public readonly int FramesTrasLaRonda;

        /// <summary>Resultado del encuentro: se decide cuando alguien reúne las victorias necesarias.</summary>
        public readonly ResultadoRonda Resultado;

        public EstadoEncuentro(EstadoCombateEquipos ronda, int victorias1, int victorias2, int numeroDeRonda, int framesTrasLaRonda, ResultadoRonda resultado)
        {
            Ronda = ronda;
            Victorias1 = victorias1;
            Victorias2 = victorias2;
            NumeroDeRonda = numeroDeRonda;
            FramesTrasLaRonda = framesTrasLaRonda;
            Resultado = resultado;
        }

        public bool Terminado => Resultado != ResultadoRonda.EnCurso;

        /// <summary>La ronda ya terminó y se espera a que empiece la siguiente.</summary>
        public bool EntreRondas => !Terminado && Ronda.Terminado;

        /// <summary>Encuentro recién empezado: primera ronda y marcador a cero.</summary>
        public static EstadoEncuentro Crear(in ConfiguracionEncuentro configuracion) =>
            new EstadoEncuentro(configuracion.RondaNueva(), 0, 0, 1, 0, ResultadoRonda.EnCurso);

        public EstadoEncuentro ConRonda(EstadoCombateEquipos ronda) =>
            new EstadoEncuentro(ronda, Victorias1, Victorias2, NumeroDeRonda, FramesTrasLaRonda, Resultado);

        public SumaVerificacion CalcularSumaVerificacion() =>
            Ronda.CalcularSumaVerificacion()
                .Agregar(Victorias1)
                .Agregar(Victorias2)
                .Agregar(NumeroDeRonda)
                .Agregar(FramesTrasLaRonda)
                .Agregar((int)Resultado);
    }
}
