using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Todo lo que cambia de un luchador entre frames. Copiable por valor para el rollback.</summary>
    public readonly struct EstadoLuchador
    {
        /// <summary>Punto entre los pies del luchador, en unidades de escenario. Y = 0 es el suelo.</summary>
        public readonly VectorFijo Posicion;

        /// <summary>Unidades por frame; solo se usa en el aire, en el suelo la caminata es directa.</summary>
        public readonly VectorFijo Velocidad;

        public readonly bool MiraDerecha;
        public readonly AccionLuchador Accion;

        /// <summary>Frames transcurridos desde que empezó la acción actual; la presentación elige la pose con este valor.</summary>
        public readonly int FramesEnAccion;

        public EstadoLuchador(VectorFijo posicion, VectorFijo velocidad, bool miraDerecha, AccionLuchador accion, int framesEnAccion)
        {
            Posicion = posicion;
            Velocidad = velocidad;
            MiraDerecha = miraDerecha;
            Accion = accion;
            FramesEnAccion = framesEnAccion;
        }

        /// <summary>Luchador quieto en el suelo, sin velocidad.</summary>
        public static EstadoLuchador EnReposo(VectorFijo posicion, bool miraDerecha) =>
            new EstadoLuchador(posicion, VectorFijo.Cero, miraDerecha, AccionLuchador.Quieto, 0);

        /// <summary>1 si mira a la derecha, -1 si mira a la izquierda.</summary>
        public int SentidoAdelante => MiraDerecha ? 1 : -1;

        public EstadoLuchador ConPosicion(VectorFijo posicion) =>
            new EstadoLuchador(posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion);

        public EstadoLuchador ConX(Fijo x) => ConPosicion(Posicion.ConX(x));

        public EstadoLuchador ConVelocidad(VectorFijo velocidad) =>
            new EstadoLuchador(Posicion, velocidad, MiraDerecha, Accion, FramesEnAccion);

        public EstadoLuchador ConOrientacion(bool miraDerecha) =>
            new EstadoLuchador(Posicion, Velocidad, miraDerecha, Accion, FramesEnAccion);

        /// <summary>Cambia de acción reiniciando el contador, o lo avanza si la acción se mantiene.</summary>
        public EstadoLuchador ConAccion(AccionLuchador accion) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, accion, accion == Accion ? FramesEnAccion + 1 : 0);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Posicion).Agregar(Velocidad).Agregar(MiraDerecha).Agregar((int)Accion).Agregar(FramesEnAccion);
    }
}
