using System;
using FgcTag.Nucleo.Colisiones;
using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Frame data de un agarre: supera la guardia, pero tiene poco alcance y no atrapa a un rival
    /// en el aire, aturdido o recién salido de un aturdimiento. Sirve igual para el agarre normal
    /// y para futuros agarres de comando.
    /// </summary>
    public readonly struct DatosAgarre
    {
        /// <summary>Frames antes de que la caja de agarre esté activa.</summary>
        public readonly int Arranque;

        public readonly int Activos;

        /// <summary>Frames de recuperación cuando el agarre falla.</summary>
        public readonly int Recuperacion;

        /// <summary>Caja de agarre relativa al luchador mirando a la derecha.</summary>
        public readonly CajaColision CajaAgarre;

        /// <summary>Frames que el rival queda sujeto antes de ser lanzado.</summary>
        public readonly int FramesSujecion;

        /// <summary>Frames que tarda quien agarra en recuperar el control tras lanzar al rival.</summary>
        public readonly int RecuperacionTrasLanzar;

        public readonly int Danio;

        /// <summary>Velocidad con la que sale lanzado el rival: X hacia donde mira quien agarra, Y hacia arriba.</summary>
        public readonly VectorFijo VelocidadLanzamiento;

        /// <summary>Aturdimiento aéreo del rival lanzado; si dura hasta el suelo, cae derribado.</summary>
        public readonly int AturdimientoLanzado;

        public DatosAgarre(
            int arranque,
            int activos,
            int recuperacion,
            CajaColision cajaAgarre,
            int framesSujecion,
            int recuperacionTrasLanzar,
            int danio,
            VectorFijo velocidadLanzamiento,
            int aturdimientoLanzado)
        {
            if (arranque < 0 || activos < 1 || recuperacion < 0 || framesSujecion < 1 || recuperacionTrasLanzar < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(activos), "Un agarre tiene al menos 1 frame activo y 1 de sujeción, y ningún tiempo negativo.");
            }

            if (danio < 0 || aturdimientoLanzado < 0 || velocidadLanzamiento.Y <= Fijo.Cero)
            {
                throw new ArgumentOutOfRangeException(nameof(danio), "El daño y el aturdimiento no pueden ser negativos y el lanzamiento debe levantar al rival.");
            }

            Arranque = arranque;
            Activos = activos;
            Recuperacion = recuperacion;
            CajaAgarre = cajaAgarre;
            FramesSujecion = framesSujecion;
            RecuperacionTrasLanzar = recuperacionTrasLanzar;
            Danio = danio;
            VelocidadLanzamiento = velocidadLanzamiento;
            AturdimientoLanzado = aturdimientoLanzado;
        }

        public int DuracionAlFallar => Arranque + Activos + Recuperacion;

        public int DuracionAlAgarrar => FramesSujecion + RecuperacionTrasLanzar;

        public bool EstaActivo(int framesEnAccion) =>
            framesEnAccion >= Arranque && framesEnAccion < Arranque + Activos;
    }
}
