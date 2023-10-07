using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct RangeInt : IEquatable<RangeInt>
    {
        public int A;
        public int B;

        public RangeInt(int a, int b)
        {
            A = a;
            B = b;
        }
        public RangeInt(int v)
        {
            A = v;
            B = v;
        }

        public readonly float GetFloat(float v)
        {
            if (A == B) return A;
            v = Math.Clamp(v, 0, 1);
            return (A * (1f - v)) + (B * v);
        }
        public readonly int Get(float v)
        {
            if (A == B) return A;
            v = Math.Clamp(v, 0, 1);
            return (int)MathF.Round((A * (1f - v)) + (B * v));
        }
        public readonly int Random()
        {
            if (A == B) return A;
            else return Get(ConsoleGame.Random.Float());
        }

        public override readonly bool Equals(object? obj) =>
            obj is RangeInt range &&
            Equals(range);
        public readonly bool Equals(RangeInt other) =>
            A == other.A &&
            B == other.B;
        public override readonly int GetHashCode() => HashCode.Combine(A, B);

        public static bool operator ==(RangeInt left, RangeInt right) => left.Equals(right);
        public static bool operator !=(RangeInt left, RangeInt right) => !(left == right);

        public static implicit operator RangeInt(ValueTuple<int, int> v) => new(v.Item1, v.Item2);
        public static implicit operator RangeInt(int v) => new(v, v);

        public static implicit operator ValueTuple<int, int>(RangeInt v) => new(v.A, v.B);

        public override readonly string ToString() => $"( {A} -> {B} )";
        readonly string GetDebuggerDisplay() => ToString();
    }
}
