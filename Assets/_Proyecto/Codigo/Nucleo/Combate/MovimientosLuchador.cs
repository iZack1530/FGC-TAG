using System;

namespace FgcTag.Nucleo.Combate
{
    /// <summary>Frame data de todos los ataques normales de un luchador, en tierra y en el aire.</summary>
    public readonly struct MovimientosLuchador
    {
        public readonly DatosMovimiento Debil;
        public readonly DatosMovimiento Medio;
        public readonly DatosMovimiento Fuerte;

        /// <summary>Ataque del botón Especial en tierra: lanza al rival al aire.</summary>
        public readonly DatosMovimiento Lanzador;

        public readonly DatosMovimiento DebilAereo;
        public readonly DatosMovimiento MedioAereo;
        public readonly DatosMovimiento FuerteAereo;

        public MovimientosLuchador(
            DatosMovimiento debil,
            DatosMovimiento medio,
            DatosMovimiento fuerte,
            DatosMovimiento lanzador,
            DatosMovimiento debilAereo,
            DatosMovimiento medioAereo,
            DatosMovimiento fuerteAereo)
        {
            Debil = debil;
            Medio = medio;
            Fuerte = fuerte;
            Lanzador = lanzador;
            DebilAereo = debilAereo;
            MedioAereo = medioAereo;
            FuerteAereo = fuerteAereo;
        }

        /// <summary>En el aire solo existen Débil, Medio y Fuerte; el botón Especial no tiene ataque aéreo.</summary>
        public static bool TieneVersionAerea(TipoAtaque ataque) =>
            ataque == TipoAtaque.Debil || ataque == TipoAtaque.Medio || ataque == TipoAtaque.Fuerte;

        public DatosMovimiento Obtener(TipoAtaque ataque, bool enAire)
        {
            switch (ataque)
            {
                case TipoAtaque.Debil:
                    return enAire ? DebilAereo : Debil;
                case TipoAtaque.Medio:
                    return enAire ? MedioAereo : Medio;
                case TipoAtaque.Fuerte:
                    return enAire ? FuerteAereo : Fuerte;
                case TipoAtaque.Especial when !enAire:
                    return Lanzador;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ataque), "No hay frame data para ese ataque en esa situación.");
            }
        }
    }
}
