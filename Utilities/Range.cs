using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Range : IEquatable<Range>
    {
        public float A;
        public float B;

        public Range(float a, float b)
        {
            A = a;
            B = b;
        }
        public Range(float v)
        {
            A = v;
            B = v;
        }

        public readonly float Get(float v)
        {
            if (A == B) return A;
            v = Math.Clamp(v, 0f, 1f);
            return (A * (1f - v)) + (B * v);
        }
        public readonly float Random()
        {
            if (A == B) return A;
            else return Get(ConsoleGame.Random.Float());
        }

        public readonly RangeInt Round() => new((int)Math.Round(A), (int)Math.Round(A));
        public readonly RangeInt Floor() => new((int)Math.Floor(A), (int)Math.Floor(A));
        public readonly RangeInt Ceil() => new((int)Math.Ceiling(A), (int)Math.Ceiling(A));

        public override readonly bool Equals(object? obj) =>
            obj is Range range &&
            Equals(range);
        public readonly bool Equals(Range other) =>
            A == other.A &&
            B == other.B;
        public override readonly int GetHashCode() => HashCode.Combine(A, B);

        public static bool operator ==(Range left, Range right) => left.Equals(right);
        public static bool operator !=(Range left, Range right) => !(left == right);

        public static implicit operator Range(RangeInt v) => new(v.A, v.B);
        public static implicit operator Range(ValueTuple<int, int> v) => new(v.Item1, v.Item2);
        public static implicit operator Range(ValueTuple<float, float> v) => new(v.Item1, v.Item2);
        public static implicit operator Range(float v) => new(v, v);

        public static implicit operator ValueTuple<float, float>(Range v) => new(v.A, v.B);

        public override readonly string ToString() => $"( {A} -> {B} )";
        readonly string GetDebuggerDisplay() => ToString();
    }
}
