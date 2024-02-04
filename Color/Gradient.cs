using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Gradient : IEquatable<Gradient>
    {
        public ColorF A;
        public ColorF B;

        public Gradient(ColorF a, ColorF b)
        {
            A = a;
            B = b;
        }

        public readonly ColorF Get(float v)
        {
            v = Math.Clamp(v, 0f, 1f);
            return (A * (1f - v)) + (B * v);
        }

        public override readonly bool Equals(object? obj) =>
            obj is Gradient gradient &&
            Equals(gradient);
        public readonly bool Equals(Gradient other) =>
            A.Equals(other.A) &&
            B.Equals(other.B);
        public override readonly int GetHashCode() => HashCode.Combine(A, B);

        public static bool operator ==(Gradient left, Gradient right) => left.Equals(right);
        public static bool operator !=(Gradient left, Gradient right) => !(left == right);

        public override readonly string ToString() => $"({A} -> {B})";
        private readonly string GetDebuggerDisplay() => ToString();
    }
}
