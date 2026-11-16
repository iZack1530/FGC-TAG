using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Equipos
{
    /// <summary>
    /// Luchador que abandona la pantalla tras un cambio: salta hacia el lado al que daba la
    /// espalda y desaparece. No participa en el combate; se guarda para dibujar su salida.
    /// </summary>
    public readonly struct EstadoSaliente
    {
        private const int SinMiembro = -1;

        public static readonly EstadoSaliente Ninguno = new EstadoSaliente(SinMiembro, VectorFijo.Cero, true, 0);

        /// <summary>Índice del miembro que sale, o -1 si nadie está saliendo.</summary>
        public readonly int Miembro;

        /// <summary>Punto del suelo desde el que saltó.</summary>
        public readonly VectorFijo Origen;

        /// <summary>Hacia dónde miraba al salir; salta en sentido contrario.</summary>
        public readonly bool MiraDerecha;

        /// <summary>Frames transcurridos desde que empezó a salir.</summary>
        public readonly int Frames;

        public EstadoSaliente(int miembro, VectorFijo origen, bool miraDerecha, int frames)
        {
            Miembro = miembro;
            Origen = origen;
            MiraDerecha = miraDerecha;
            Frames = frames;
        }

        public static EstadoSaliente Desde(int miembro, VectorFijo origen, bool miraDerecha) =>
            new EstadoSaliente(miembro, origen, miraDerecha, 0);

        public bool EnPantalla => Miembro != SinMiembro;

        /// <summary>Avanza un frame; al cumplir la duración de la salida, desaparece.</summary>
        public EstadoSaliente UnFrameDespues(int framesSalida) =>
            !EnPantalla || Frames + 1 >= framesSalida ? Ninguno : new EstadoSaliente(Miembro, Origen, MiraDerecha, Frames + 1);

        /// <summary>Posición actual: se aleja hacia atrás y hacia arriba a velocidad constante.</summary>
        public VectorFijo Posicion(VectorFijo velocidadSalida) =>
            new VectorFijo(
                Origen.X - velocidadSalida.X * Frames * (MiraDerecha ? 1 : -1),
                Origen.Y + velocidadSalida.Y * Frames);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Miembro)
                .Agregar(Origen)
                .Agregar(MiraDerecha)
                .Agregar(Frames);
    }
}
