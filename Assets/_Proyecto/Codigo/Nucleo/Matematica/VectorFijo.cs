using System;

namespace FgcTag.Nucleo.Matematica
{
    /// <summary>Vector 2D de punto fijo: el combate ocurre en un plano aunque se dibuje en 3D.</summary>
    public readonly struct VectorFijo : IEquatable<VectorFijo>
    {
        public static readonly VectorFijo Cero = new VectorFijo(Fijo.Cero, Fijo.Cero);

        public readonly Fijo X;
        public readonly Fijo Y;

        public VectorFijo(Fijo x, Fijo y)
        {
            X = x;
            Y = y;
        }

        public static VectorFijo DesdeEnteros(int x, int y) => new VectorFijo(Fijo.DesdeEntero(x), Fijo.DesdeEntero(y));

        public static VectorFijo operator +(VectorFijo a, VectorFijo b) => new VectorFijo(a.X + b.X, a.Y + b.Y);

        public static VectorFijo operator -(VectorFijo a, VectorFijo b) => new VectorFijo(a.X - b.X, a.Y - b.Y);

        public static VectorFijo operator -(VectorFijo a) => new VectorFijo(-a.X, -a.Y);

        public static VectorFijo operator *(VectorFijo a, Fijo escalar) => new VectorFijo(a.X * escalar, a.Y * escalar);

        public static bool operator ==(VectorFijo a, VectorFijo b) => a.Equals(b);

        public static bool operator !=(VectorFijo a, VectorFijo b) => !a.Equals(b);

        public VectorFijo ConX(Fijo x) => new VectorFijo(x, Y);

        public VectorFijo ConY(Fijo y) => new VectorFijo(X, y);

        /// <summary>Refleja el vector en el eje X; se usa cuando el personaje mira a la izquierda.</summary>
        public VectorFijo ReflejadoHorizontal() => new VectorFijo(-X, Y);

        public bool Equals(VectorFijo otro) => X == otro.X && Y == otro.Y;

        public override bool Equals(object obj) => obj is VectorFijo otro && Equals(otro);

        public override int GetHashCode() => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());

        public override string ToString() => $"({X}, {Y})";
    }
}
