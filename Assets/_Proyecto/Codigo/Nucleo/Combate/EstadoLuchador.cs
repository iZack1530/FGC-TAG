using FgcTag.Nucleo.Matematica;
using FgcTag.Nucleo.Simulacion;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Todo lo que cambia de un luchador entre frames. Copiable por valor para el rollback.</summary>
    public readonly struct EstadoLuchador
    {
        /// <summary>Punto entre los pies del luchador, en unidades de escenario. Y = 0 es el suelo.</summary>
        public readonly VectorFijo Posicion;

        /// <summary>Unidades por frame; se usa en el aire y para el retroceso al ser golpeado.</summary>
        public readonly VectorFijo Velocidad;

        public readonly bool MiraDerecha;
        public readonly AccionLuchador Accion;

        /// <summary>Frames transcurridos desde que empezó la acción actual; la presentación elige la pose con este valor.</summary>
        public readonly int FramesEnAccion;

        /// <summary>Frames de congelamiento restantes; mientras sea mayor que cero el luchador no avanza.</summary>
        public readonly int FramesHitstop;

        /// <summary>Frames de hitstun o blockstun restantes.</summary>
        public readonly int FramesAturdimiento;

        /// <summary>El ataque en curso ya conectó; evita que golpee varias veces durante sus frames activos.</summary>
        public readonly bool GolpeConectado;

        /// <summary>Botones del frame anterior, para distinguir pulsar de mantener.</summary>
        public readonly BotonesEntrada BotonesAnteriores;

        public EstadoLuchador(
            VectorFijo posicion,
            VectorFijo velocidad,
            bool miraDerecha,
            AccionLuchador accion,
            int framesEnAccion,
            int framesHitstop,
            int framesAturdimiento,
            bool golpeConectado,
            BotonesEntrada botonesAnteriores)
        {
            Posicion = posicion;
            Velocidad = velocidad;
            MiraDerecha = miraDerecha;
            Accion = accion;
            FramesEnAccion = framesEnAccion;
            FramesHitstop = framesHitstop;
            FramesAturdimiento = framesAturdimiento;
            GolpeConectado = golpeConectado;
            BotonesAnteriores = botonesAnteriores;
        }

        /// <summary>Luchador quieto en el suelo, sin velocidad.</summary>
        public static EstadoLuchador EnReposo(VectorFijo posicion, bool miraDerecha) =>
            new EstadoLuchador(posicion, VectorFijo.Cero, miraDerecha, AccionLuchador.Quieto, 0, 0, 0, false, BotonesEntrada.Ninguno);

        /// <summary>1 si mira a la derecha, -1 si mira a la izquierda.</summary>
        public int SentidoAdelante => MiraDerecha ? 1 : -1;

        public bool EnHitstop => FramesHitstop > 0;

        public EstadoLuchador ConPosicion(VectorFijo posicion) =>
            new EstadoLuchador(posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        public EstadoLuchador ConX(Fijo x) => ConPosicion(Posicion.ConX(x));

        public EstadoLuchador ConVelocidad(VectorFijo velocidad) =>
            new EstadoLuchador(Posicion, velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        public EstadoLuchador ConOrientacion(bool miraDerecha) =>
            new EstadoLuchador(Posicion, Velocidad, miraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        /// <summary>Cambia de acción reiniciando el contador, o lo avanza si la acción se mantiene.</summary>
        public EstadoLuchador ConAccion(AccionLuchador accion) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, accion, accion == Accion ? FramesEnAccion + 1 : 0, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        /// <summary>Empieza una acción desde su primer frame aunque sea la misma que la actual.</summary>
        public EstadoLuchador ConAccionNueva(AccionLuchador accion) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, accion, 0, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        public EstadoLuchador ConHitstop(int frames) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, frames, FramesAturdimiento, GolpeConectado, BotonesAnteriores);

        public EstadoLuchador ConAturdimiento(int frames) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, frames, GolpeConectado, BotonesAnteriores);

        public EstadoLuchador ConGolpeConectado(bool golpeConectado) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, golpeConectado, BotonesAnteriores);

        public EstadoLuchador ConBotonesAnteriores(BotonesEntrada botones) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, botones);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Posicion)
                .Agregar(Velocidad)
                .Agregar(MiraDerecha)
                .Agregar((int)Accion)
                .Agregar(FramesEnAccion)
                .Agregar(FramesHitstop)
                .Agregar(FramesAturdimiento)
                .Agregar(GolpeConectado)
                .Agregar((int)BotonesAnteriores);
    }
}
