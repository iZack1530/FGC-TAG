using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Estado de los controles de un jugador en un frame. Las direcciones son absolutas
    /// (izquierda/derecha de la pantalla); el simulador las traduce a adelante/atrás.
    /// </summary>
    [Flags]
    public enum BotonesEntrada : ushort
    {
        Ninguno = 0,
        Arriba = 1 << 0,
        Abajo = 1 << 1,
        Izquierda = 1 << 2,
        Derecha = 1 << 3,
        Debil = 1 << 4,
        Medio = 1 << 5,
        Fuerte = 1 << 6,
        Especial = 1 << 7,
        Asistencia1 = 1 << 8,
        Asistencia2 = 1 << 9,
    }

    public static class InterpretacionEntradas
    {
        public static bool Tiene(BotonesEntrada botones, BotonesEntrada boton) => (botones & boton) == boton;

        /// <summary>
        /// Dirección horizontal absoluta: -1 izquierda, 1 derecha, 0 neutral.
        /// Izquierda y derecha a la vez cuentan como neutral (resolución SOCD estándar en la FGC).
        /// </summary>
        public static int DireccionHorizontal(BotonesEntrada botones)
        {
            var izquierda = Tiene(botones, BotonesEntrada.Izquierda);
            var derecha = Tiene(botones, BotonesEntrada.Derecha);

            if (izquierda == derecha)
            {
                return 0;
            }

            return derecha ? 1 : -1;
        }

        /// <summary>Arriba y abajo a la vez cuentan como neutral, igual que en el eje horizontal.</summary>
        public static int DireccionVertical(BotonesEntrada botones)
        {
            var arriba = Tiene(botones, BotonesEntrada.Arriba);
            var abajo = Tiene(botones, BotonesEntrada.Abajo);

            if (arriba == abajo)
            {
                return 0;
            }

            return arriba ? 1 : -1;
        }
    }
}
