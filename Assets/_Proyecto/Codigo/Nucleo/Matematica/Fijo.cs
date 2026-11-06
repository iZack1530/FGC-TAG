using System;
using System.Globalization;

namespace FgcTag.Nucleo.Matematica
{
    /// <summary>
    /// Número de punto fijo Q47.16 almacenado en un <see cref="long"/>.
    /// Sustituye a <c>float</c> dentro de la simulación para que el resultado sea idéntico,
    /// bit a bit, en cualquier procesador: requisito del rollback y de las repeticiones.
    /// </summary>
    /// <remarks>
    /// La multiplicación es segura mientras el producto de los valores crudos no supere 2^63,
    /// es decir, con factores de hasta ±46 000 unidades aproximadamente. Las posiciones y
    /// velocidades del combate están muy por debajo de ese límite.
    /// </remarks>
    public readonly struct Fijo : IEquatable<Fijo>, IComparable<Fijo>
    {
        public const int BitsFraccion = 16;
        public const long UnoCrudo = 1L << BitsFraccion;

        public static readonly Fijo Cero = new Fijo(0);
        public static readonly Fijo Uno = new Fijo(UnoCrudo);
        public static readonly Fijo MenosUno = new Fijo(-UnoCrudo);
        public static readonly Fijo Medio = new Fijo(UnoCrudo / 2);

        public readonly long Crudo;

        private Fijo(long crudo) => Crudo = crudo;

        public static Fijo DesdeCrudo(long crudo) => new Fijo(crudo);

        public static Fijo DesdeEntero(int valor) => new Fijo((long)valor << BitsFraccion);

        /// <summary>Crea un valor exacto a partir de una fracción, sin pasar por <c>float</c>.</summary>
        public static Fijo DesdeFraccion(int numerador, int denominador)
        {
            if (denominador == 0)
            {
                throw new DivideByZeroException("El denominador de una fracción de punto fijo no puede ser cero.");
            }

            return new Fijo(((long)numerador << BitsFraccion) / denominador);
        }

        public static Fijo operator +(Fijo a, Fijo b) => new Fijo(a.Crudo + b.Crudo);

        public static Fijo operator -(Fijo a, Fijo b) => new Fijo(a.Crudo - b.Crudo);

        public static Fijo operator -(Fijo a) => new Fijo(-a.Crudo);

        public static Fijo operator *(Fijo a, Fijo b) => new Fijo((a.Crudo * b.Crudo) >> BitsFraccion);

        public static Fijo operator *(Fijo a, int b) => new Fijo(a.Crudo * b);

        public static Fijo operator /(Fijo a, Fijo b)
        {
            if (b.Crudo == 0)
            {
                throw new DivideByZeroException("División de punto fijo entre cero.");
            }

            return new Fijo((a.Crudo << BitsFraccion) / b.Crudo);
        }

        public static Fijo operator /(Fijo a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("División de punto fijo entre cero.");
            }

            return new Fijo(a.Crudo / b);
        }

        public static bool operator ==(Fijo a, Fijo b) => a.Crudo == b.Crudo;

        public static bool operator !=(Fijo a, Fijo b) => a.Crudo != b.Crudo;

        public static bool operator <(Fijo a, Fijo b) => a.Crudo < b.Crudo;

        public static bool operator >(Fijo a, Fijo b) => a.Crudo > b.Crudo;

        public static bool operator <=(Fijo a, Fijo b) => a.Crudo <= b.Crudo;

        public static bool operator >=(Fijo a, Fijo b) => a.Crudo >= b.Crudo;

        public static Fijo Abs(Fijo valor) => valor.Crudo < 0 ? -valor : valor;

        public static Fijo Min(Fijo a, Fijo b) => a.Crudo <= b.Crudo ? a : b;

        public static Fijo Max(Fijo a, Fijo b) => a.Crudo >= b.Crudo ? a : b;

        public static Fijo Limitar(Fijo valor, Fijo minimo, Fijo maximo)
        {
            if (minimo > maximo)
            {
                throw new ArgumentException("El mínimo no puede ser mayor que el máximo.", nameof(minimo));
            }

            return Max(minimo, Min(valor, maximo));
        }

        public static int Signo(Fijo valor) => valor.Crudo > 0 ? 1 : valor.Crudo < 0 ? -1 : 0;

        /// <summary>Mayor entero menor o igual que el valor (redondeo hacia menos infinito).</summary>
        public int Piso() => (int)(Crudo >> BitsFraccion);

        /// <summary>Entero más cercano; los empates se redondean hacia arriba.</summary>
        public int Redondear() => (int)((Crudo + UnoCrudo / 2) >> BitsFraccion);

        /// <summary>
        /// Conversión de salida para la capa de presentación. Nunca debe volver a entrar en la
        /// simulación, porque <c>float</c> no es determinista entre plataformas.
        /// </summary>
        public float AFlotante() => (float)((double)Crudo / UnoCrudo);

        public bool Equals(Fijo otro) => Crudo == otro.Crudo;

        public override bool Equals(object obj) => obj is Fijo otro && Equals(otro);

        public override int GetHashCode() => Crudo.GetHashCode();

        public int CompareTo(Fijo otro) => Crudo.CompareTo(otro.Crudo);

        public override string ToString() =>
            ((double)Crudo / UnoCrudo).ToString("0.#####", CultureInfo.InvariantCulture);
    }
}
