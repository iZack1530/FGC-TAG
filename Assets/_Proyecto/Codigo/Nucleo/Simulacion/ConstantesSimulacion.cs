namespace FgcTag.Nucleo.Simulacion
{
    /// <summary>Valores fijos de la simulación. Cambiar cualquiera invalida todas las repeticiones grabadas.</summary>
    public static class ConstantesSimulacion
    {
        /// <summary>Frecuencia de la lógica; toda la frame data se expresa en estos frames.</summary>
        public const int FramesPorSegundoLogica = 60;

        /// <summary>Personajes por equipo en el formato 3v3. Es un parámetro para no cerrar la puerta a un 2v2.</summary>
        public const int PersonajesPorEquipo = 3;

        public const int JugadoresPorPartida = 2;
    }
}
