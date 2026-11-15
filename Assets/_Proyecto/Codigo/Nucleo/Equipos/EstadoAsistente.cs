using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Compañero que entra un momento a la pantalla para hacer un único ataque y marcharse. No
    /// recibe golpes ni sustituye al luchador activo.
    /// </summary>
    public readonly struct EstadoAsistente
    {
        private const int SinMiembro = -1;

        public static readonly EstadoAsistente Ninguno = new EstadoAsistente(SinMiembro, VectorFijo.Cero, true, 0, 0, false);

        /// <summary>Posición en el equipo, de 0 a 2, del compañero que asiste; -1 si no hay asistente en pantalla.</summary>
        public readonly int Miembro;

        public readonly VectorFijo Posicion;
        public readonly bool MiraDerecha;

        /// <summary>Frames desde que fue llamado: primero llega y después ejecuta su ataque.</summary>
        public readonly int Frames;

        public readonly int FramesHitstop;
        public readonly bool GolpeConectado;

        public EstadoAsistente(int miembro, VectorFijo posicion, bool miraDerecha, int frames, int framesHitstop, bool golpeConectado)
        {
            Miembro = miembro;
            Posicion = posicion;
            MiraDerecha = miraDerecha;
            Frames = frames;
            FramesHitstop = framesHitstop;
            GolpeConectado = golpeConectado;
        }

        public static EstadoAsistente Llamado(int miembro, VectorFijo posicion, bool miraDerecha) =>
            new EstadoAsistente(miembro, posicion, miraDerecha, 0, 0, false);

        public bool EnPantalla => Miembro != SinMiembro;

        public EstadoAsistente UnFrameDespues() =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames + 1, FramesHitstop, GolpeConectado);

        public EstadoAsistente ConHitstop(int frames) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames, frames, GolpeConectado);

        public EstadoAsistente TrasConectar(int framesHitstop) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames + 1, framesHitstop, true);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Miembro)
                .Agregar(Posicion)
                .Agregar(MiraDerecha)
                .Agregar(Frames)
                .Agregar(FramesHitstop)
                .Agregar(GolpeConectado);
    }
}
