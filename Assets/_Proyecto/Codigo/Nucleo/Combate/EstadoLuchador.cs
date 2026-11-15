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

        /// <summary>El ataque en curso ya conectó: no vuelve a golpear y se puede cancelar en uno más fuerte.</summary>
        public readonly bool GolpeConectado;

        /// <summary>Botones del frame anterior, para distinguir pulsar de mantener.</summary>
        public readonly BotonesEntrada BotonesAnteriores;

        /// <summary>Ataque que se está ejecutando; Ninguno fuera de la acción Atacando.</summary>
        public readonly TipoAtaque Ataque;

        /// <summary>Ataque pulsado hace poco que saldrá en cuanto el luchador pueda actuar.</summary>
        public readonly TipoAtaque AtaqueEnBufer;

        /// <summary>Frames que le quedan al ataque guardado en el búfer antes de descartarse.</summary>
        public readonly int FramesBufer;

        /// <summary>Golpes seguidos recibidos sin haber recuperado el control.</summary>
        public readonly int GolpesEnCombo;

        /// <summary>Vida restante; al llegar a 0 el luchador queda noqueado.</summary>
        public readonly int Vida;

        public EstadoLuchador(
            VectorFijo posicion,
            VectorFijo velocidad,
            bool miraDerecha,
            AccionLuchador accion,
            int framesEnAccion,
            int framesHitstop,
            int framesAturdimiento,
            bool golpeConectado,
            BotonesEntrada botonesAnteriores,
            TipoAtaque ataque,
            TipoAtaque ataqueEnBufer,
            int framesBufer,
            int golpesEnCombo,
            int vida)
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
            Ataque = ataque;
            AtaqueEnBufer = ataqueEnBufer;
            FramesBufer = framesBufer;
            GolpesEnCombo = golpesEnCombo;
            Vida = vida;
        }

        /// <summary>Luchador quieto en el suelo, sin velocidad y con la vida indicada.</summary>
        public static EstadoLuchador EnReposo(VectorFijo posicion, bool miraDerecha, int vida) =>
            new EstadoLuchador(
                posicion,
                VectorFijo.Cero,
                miraDerecha,
                AccionLuchador.Quieto,
                0,
                0,
                0,
                false,
                BotonesEntrada.Ninguno,
                TipoAtaque.Ninguno,
                TipoAtaque.Ninguno,
                0,
                0,
                vida);

        /// <summary>1 si mira a la derecha, -1 si mira a la izquierda.</summary>
        public int SentidoAdelante => MiraDerecha ? 1 : -1;

        public bool EnHitstop => FramesHitstop > 0;

        public bool SinVida => Vida <= 0;

        public bool TieneAtaqueEnBufer => AtaqueEnBufer != TipoAtaque.Ninguno && FramesBufer > 0;

        public EstadoLuchador ConPosicion(VectorFijo posicion) =>
            new EstadoLuchador(posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConX(Fijo x) => ConPosicion(Posicion.ConX(x));

        public EstadoLuchador ConVelocidad(VectorFijo velocidad) =>
            new EstadoLuchador(Posicion, velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConOrientacion(bool miraDerecha) =>
            new EstadoLuchador(Posicion, Velocidad, miraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        /// <summary>Cambia de acción reiniciando el contador, o lo avanza si la acción se mantiene.</summary>
        public EstadoLuchador ConAccion(AccionLuchador accion) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, accion, accion == Accion ? FramesEnAccion + 1 : 0, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        /// <summary>Empieza una acción desde su primer frame aunque sea la misma que la actual.</summary>
        public EstadoLuchador ConAccionNueva(AccionLuchador accion) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, accion, 0, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConHitstop(int frames) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, frames, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConAturdimiento(int frames) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, frames, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConGolpeConectado(bool golpeConectado) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, golpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConBotonesAnteriores(BotonesEntrada botones) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, botones, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConAtaque(TipoAtaque ataque) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, Vida);

        public EstadoLuchador ConBufer(TipoAtaque ataque, int frames) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, ataque, frames, GolpesEnCombo, Vida);

        public EstadoLuchador ConGolpesEnCombo(int golpes) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, golpes, Vida);

        public EstadoLuchador ConVida(int vida) =>
            new EstadoLuchador(Posicion, Velocidad, MiraDerecha, Accion, FramesEnAccion, FramesHitstop, FramesAturdimiento, GolpeConectado, BotonesAnteriores, Ataque, AtaqueEnBufer, FramesBufer, GolpesEnCombo, vida);

        public SumaVerificacion AgregarA(SumaVerificacion suma) =>
            suma.Agregar(Posicion)
                .Agregar(Velocidad)
                .Agregar(MiraDerecha)
                .Agregar((int)Accion)
                .Agregar(FramesEnAccion)
                .Agregar(FramesHitstop)
                .Agregar(FramesAturdimiento)
                .Agregar(GolpeConectado)
                .Agregar((int)BotonesAnteriores)
                .Agregar((int)Ataque)
                .Agregar((int)AtaqueEnBufer)
                .Agregar(FramesBufer)
                .Agregar(GolpesEnCombo)
                .Agregar(Vida);
    }
}
