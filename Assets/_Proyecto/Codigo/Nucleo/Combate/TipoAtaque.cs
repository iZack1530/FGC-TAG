namespace FgcTag.Nucleo.Combate
{
    /// <summary>
    /// Ataques normales ordenados por fuerza. El orden numérico define la cancelación: un ataque
    /// que conectó se puede cancelar en otro de valor mayor. Los valores forman parte de la suma
    /// de verificación: se agregan al final y nunca se reordenan.
    /// </summary>
    public enum TipoAtaque : byte
    {
        Ninguno = 0,
        Debil = 1,
        Medio = 2,
        Fuerte = 3,

        /// <summary>Botón Especial: en tierra es el lanzador universal.</summary>
        Especial = 4,
    }

    public static class LecturaAtaques
    {
        /// <summary>
        /// Ataque que el jugador acaba de pulsar en este frame (no cuenta mantener el botón).
        /// Si pulsa varios a la vez, gana el más fuerte.
        /// </summary>
        public static TipoAtaque RecienPulsado(BotonesEntrada botones, BotonesEntrada botonesAnteriores)
        {
            var nuevos = botones & ~botonesAnteriores;

            if (InterpretacionEntradas.Tiene(nuevos, BotonesEntrada.Especial))
            {
                return TipoAtaque.Especial;
            }

            if (InterpretacionEntradas.Tiene(nuevos, BotonesEntrada.Fuerte))
            {
                return TipoAtaque.Fuerte;
            }

            if (InterpretacionEntradas.Tiene(nuevos, BotonesEntrada.Medio))
            {
                return TipoAtaque.Medio;
            }

            return InterpretacionEntradas.Tiene(nuevos, BotonesEntrada.Debil) ? TipoAtaque.Debil : TipoAtaque.Ninguno;
        }
    }
}
