using System;
using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Frame data de un ataque, en frames de 60 Hz. El movimiento dura
    /// Arranque + Activos + Recuperacion frames y su primer frame activo es el Arranque + 1.
    /// </summary>
    public readonly struct DatosMovimiento
    {
        /// <summary>Frames antes del primer frame activo.</summary>
        public readonly int Arranque;

        /// <summary>Frames en los que la caja de golpe puede conectar.</summary>
        public readonly int Activos;

        /// <summary>Frames tras el último activo hasta recuperar el control.</summary>
        public readonly int Recuperacion;

        /// <summary>Frames que el rival queda sin control al recibir el golpe, contados tras el hitstop.</summary>
        public readonly int Hitstun;

        /// <summary>Frames que el rival queda sin control al bloquear, contados tras el hitstop.</summary>
        public readonly int Blockstun;

        /// <summary>Frames que ambos luchadores se congelan al conectar, golpee o sea bloqueado.</summary>
        public readonly int Hitstop;

        /// <summary>Vida que quita el golpe como primer golpe de un combo; bloqueado no quita nada.</summary>
        public readonly int Danio;

        /// <summary>Velocidad inicial (unidades por frame) con la que retrocede el rival golpeado.</summary>
        public readonly Fijo RetrocesoAlGolpear;

        /// <summary>Velocidad inicial (unidades por frame) con la que retrocede el rival que bloquea.</summary>
        public readonly Fijo RetrocesoAlBloquear;

        /// <summary>Caja de golpe relativa a los pies del atacante mirando a la derecha.</summary>
        public readonly CajaColision CajaGolpe;

        public DatosMovimiento(
            int arranque,
            int activos,
            int recuperacion,
            int hitstun,
            int blockstun,
            int hitstop,
            int danio,
            Fijo retrocesoAlGolpear,
            Fijo retrocesoAlBloquear,
            CajaColision cajaGolpe)
        {
            if (arranque < 0 || activos < 1 || recuperacion < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(activos), "Un movimiento necesita al menos 1 frame activo y duraciones no negativas.");
            }

            if (hitstun < 1 || blockstun < 1 || hitstop < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hitstun), "Hitstun y blockstun duran al menos 1 frame y el hitstop no puede ser negativo.");
            }

            if (danio < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(danio), "El daño no puede ser negativo.");
            }

            if (retrocesoAlGolpear < Fijo.Cero || retrocesoAlBloquear < Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(retrocesoAlGolpear), "El retroceso no puede ser negativo.");
            }

            Arranque = arranque;
            Activos = activos;
            Recuperacion = recuperacion;
            Hitstun = hitstun;
            Blockstun = blockstun;
            Hitstop = hitstop;
            Danio = danio;
            RetrocesoAlGolpear = retrocesoAlGolpear;
            RetrocesoAlBloquear = retrocesoAlBloquear;
            CajaGolpe = cajaGolpe;
        }

        public int DuracionTotal => Arranque + Activos + Recuperacion;

        /// <summary>
        /// Frames que el atacante se adelanta (positivo) o se retrasa (negativo) respecto al rival
        /// cuando el golpe conecta en su primer frame activo.
        /// </summary>
        public int VentajaAlGolpear => Hitstun - FramesRestantesTrasPrimerActivo;

        /// <summary>Igual que <see cref="VentajaAlGolpear"/>, pero cuando el rival bloquea.</summary>
        public int VentajaAlBloquear => Blockstun - FramesRestantesTrasPrimerActivo;

        /// <param name="framesEnAccion">Frames transcurridos desde que empezó el ataque (0 en su primer frame).</param>
        public bool EstaActivo(int framesEnAccion) =>
            framesEnAccion >= Arranque && framesEnAccion < Arranque + Activos;

        private int FramesRestantesTrasPrimerActivo => Activos - 1 + Recuperacion;
    }
}
