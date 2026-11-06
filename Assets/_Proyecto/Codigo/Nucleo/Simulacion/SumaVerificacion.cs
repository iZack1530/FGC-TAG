using FgcTag.Nucleo.Matematica;

namespace FgcTag.Nucleo.Simulacion
{
    /// <summary>
    /// Suma de verificación FNV-1a de 64 bits para comparar estados de partida entre frames,
    /// jugadores y repeticiones. No se usa <c>GetHashCode</c> ni <c>System.HashCode</c> porque
    /// pueden variar entre ejecuciones; esta suma es estable para siempre.
    /// </summary>
    public readonly struct SumaVerificacion
    {
        private const ulong BaseFnv = 14695981039346656037UL;
        private const ulong PrimoFnv = 1099511628211UL;

        public static readonly SumaVerificacion Inicial = new SumaVerificacion(BaseFnv);

        public readonly ulong Valor;

        private SumaVerificacion(ulong valor) => Valor = valor;

        public SumaVerificacion Agregar(long dato)
        {
            var acumulado = Valor;
            var bits = unchecked((ulong)dato);

            for (var desplazamiento = 0; desplazamiento < 64; desplazamiento += 8)
            {
                acumulado ^= (bits >> desplazamiento) & 0xFFUL;
                acumulado = unchecked(acumulado * PrimoFnv);
            }

            return new SumaVerificacion(acumulado);
        }

        public SumaVerificacion Agregar(int dato) => Agregar((long)dato);

        public SumaVerificacion Agregar(bool dato) => Agregar(dato ? 1L : 0L);

        public SumaVerificacion Agregar(Fijo dato) => Agregar(dato.Crudo);

        public SumaVerificacion Agregar(VectorFijo dato) => Agregar(dato.X).Agregar(dato.Y);

        public override string ToString() => Valor.ToString("X16");
    }
}
