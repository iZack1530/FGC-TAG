using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Frame data de todos los ataques normales de un luchador: de pie, agachado y en el aire.</summary>
    public readonly struct MovimientosLuchador
    {
        public readonly DatosMovimiento Debil;
        public readonly DatosMovimiento Medio;
        public readonly DatosMovimiento Fuerte;

        /// <summary>Ataque del botón Especial en tierra: lanza al rival al aire.</summary>
        public readonly DatosMovimiento Lanzador;

        public readonly DatosMovimiento DebilAgachado;
        public readonly DatosMovimiento MedioAgachado;
        public readonly DatosMovimiento FuerteAgachado;

        public readonly DatosMovimiento DebilAereo;
        public readonly DatosMovimiento MedioAereo;
        public readonly DatosMovimiento FuerteAereo;

        /// <summary>Ataque que este luchador hace cuando un compañero lo llama como asistente.</summary>
        public readonly DatosMovimiento Asistencia;

        public MovimientosLuchador(
            DatosMovimiento debil,
            DatosMovimiento medio,
            DatosMovimiento fuerte,
            DatosMovimiento lanzador,
            DatosMovimiento debilAgachado,
            DatosMovimiento medioAgachado,
            DatosMovimiento fuerteAgachado,
            DatosMovimiento debilAereo,
            DatosMovimiento medioAereo,
            DatosMovimiento fuerteAereo,
            DatosMovimiento asistencia)
        {
            Debil = debil;
            Medio = medio;
            Fuerte = fuerte;
            Lanzador = lanzador;
            DebilAgachado = debilAgachado;
            MedioAgachado = medioAgachado;
            FuerteAgachado = fuerteAgachado;
            DebilAereo = debilAereo;
            MedioAereo = medioAereo;
            FuerteAereo = fuerteAereo;
            Asistencia = asistencia;
        }

        /// <summary>En el aire solo existen Débil, Medio y Fuerte; el botón Especial no tiene ataque aéreo.</summary>
        public static bool TieneVersionAerea(TipoAtaque ataque) =>
            ataque == TipoAtaque.Debil || ataque == TipoAtaque.Medio || ataque == TipoAtaque.Fuerte;

        /// <summary>El lanzador es el mismo de pie y agachado.</summary>
        public DatosMovimiento Obtener(TipoAtaque ataque, PosturaAtaque postura)
        {
            switch (ataque)
            {
                case TipoAtaque.Debil:
                    return Elegir(postura, Debil, DebilAgachado, DebilAereo);
                case TipoAtaque.Medio:
                    return Elegir(postura, Medio, MedioAgachado, MedioAereo);
                case TipoAtaque.Fuerte:
                    return Elegir(postura, Fuerte, FuerteAgachado, FuerteAereo);
                case TipoAtaque.Especial when postura != PosturaAtaque.Aire:
                    return Lanzador;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ataque), "No hay frame data para ese ataque en esa situación.");
            }
        }

        private static DatosMovimiento Elegir(PosturaAtaque postura, in DatosMovimiento dePie, in DatosMovimiento agachado, in DatosMovimiento aereo)
        {
            switch (postura)
            {
                case PosturaAtaque.Agachado:
                    return agachado;
                case PosturaAtaque.Aire:
                    return aereo;
                default:
                    return dePie;
            }
        }
    }
}
