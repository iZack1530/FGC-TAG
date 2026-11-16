using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Compañero llamado como asistente: aparece delante del luchador activo, ejecuta su ataque de
    /// asistencia, espera unos frames y se marcha. Mientras está en pantalla el rival puede
    /// golpearlo: si recibe un golpe pierde su ataque, queda aturdido y se retira al recuperarse.
    /// </summary>
    public readonly struct EstadoAsistente
    {
        private const int SinMiembro = -1;
        private const int SinGolpe = -1;

        public static readonly EstadoAsistente Ninguno = new EstadoAsistente(SinMiembro, VectorFijo.Cero, true, 0, 0, false, 0, SinGolpe, 0);

        /// <summary>Índice del miembro del equipo que asiste, o -1 si no hay asistente en pantalla.</summary>
        public readonly int Miembro;

        public readonly VectorFijo Posicion;
        public readonly bool MiraDerecha;

        /// <summary>Frames transcurridos desde que fue llamado, sin contar los congelados por hitstop.</summary>
        public readonly int Frames;

        public readonly int FramesHitstop;

        /// <summary>Su ataque ya golpeó al rival.</summary>
        public readonly bool GolpeConectado;

        /// <summary>Frames de aturdimiento que le quedan tras recibir un golpe.</summary>
        public readonly int FramesAturdimiento;

        /// <summary>
        /// Frame del ataque del rival en el que ese ataque lo golpeó, o -1. Evita que el mismo
        /// ataque lo golpee en cada uno de sus frames activos.
        /// </summary>
        public readonly int FrameDelGolpeRecibido;

        /// <summary>Golpes recibidos desde que entró: escalan el daño de los siguientes como en un combo.</summary>
        public readonly int GolpesRecibidos;

        public EstadoAsistente(
            int miembro,
            VectorFijo posicion,
            bool miraDerecha,
            int frames,
            int framesHitstop,
            bool golpeConectado,
            int framesAturdimiento,
            int frameDelGolpeRecibido,
            int golpesRecibidos)
        {
            Miembro = miembro;
            Posicion = posicion;
            MiraDerecha = miraDerecha;
            Frames = frames;
            FramesHitstop = framesHitstop;
            GolpeConectado = golpeConectado;
            FramesAturdimiento = framesAturdimiento;
            FrameDelGolpeRecibido = frameDelGolpeRecibido;
            GolpesRecibidos = golpesRecibidos;
        }

        public static EstadoAsistente Llamado(int miembro, VectorFijo posicion, bool miraDerecha) =>
            new EstadoAsistente(miembro, posicion, miraDerecha, 0, 0, false, 0, SinGolpe, 0);

        public bool EnPantalla => Miembro != SinMiembro;

        /// <summary>Recibió algún golpe: ya no ataca y se retira cuando se le pasa el aturdimiento.</summary>
        public bool FueGolpeado => GolpesRecibidos > 0;

        /// <summary>El ataque del rival que va por ese frame ya lo golpeó.</summary>
        public bool YaLoGolpeoElAtaqueEnElFrame(int frameDelAtaque) =>
            FrameDelGolpeRecibido != SinGolpe && frameDelAtaque >= FrameDelGolpeRecibido;

        public EstadoAsistente UnFrameDespues() =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames + 1, FramesHitstop, GolpeConectado, FramesAturdimiento, FrameDelGolpeRecibido, GolpesRecibidos);

        public EstadoAsistente ConHitstop(int frames) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames, frames, GolpeConectado, FramesAturdimiento, FrameDelGolpeRecibido, GolpesRecibidos);

        public EstadoAsistente TrasConectar(int framesHitstop) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames + 1, framesHitstop, true, FramesAturdimiento, FrameDelGolpeRecibido, GolpesRecibidos);

        public EstadoAsistente ConAturdimiento(int frames) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames, FramesHitstop, GolpeConectado, frames, FrameDelGolpeRecibido, GolpesRecibidos);

        /// <summary>Recibe un golpe: queda aturdido y recuerda en qué frame del ataque rival ocurrió.</summary>
        public EstadoAsistente TrasRecibirGolpe(int framesAturdimiento, int frameDelAtaqueRival) =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames, 0, GolpeConectado, framesAturdimiento, frameDelAtaqueRival, GolpesRecibidos + 1);

        /// <summary>El rival empezó otro ataque o dejó de atacar: el siguiente ataque puede volver a golpearlo.</summary>
        public EstadoAsistente SinGolpeRecordado() =>
            new EstadoAsistente(Miembro, Posicion, MiraDerecha, Frames, FramesHitstop, GolpeConectado, FramesAturdimiento, SinGolpe, GolpesRecibidos);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Miembro)
                .Agregar(Posicion)
                .Agregar(MiraDerecha)
                .Agregar(Frames)
                .Agregar(FramesHitstop)
                .Agregar(GolpeConectado)
                .Agregar(FramesAturdimiento)
                .Agregar(FrameDelGolpeRecibido)
                .Agregar(GolpesRecibidos);
    }
}
