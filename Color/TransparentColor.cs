using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct TransparentColor : IEquatable<TransparentColor>
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public readonly float Luminance => (0.2126f * R) + (0.7152f * G) + (0.0722f * B);
        public readonly bool Overflow => R > 1f || G > 1f || B > 1f;

        public TransparentColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public TransparentColor(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
            A = 1f;
        }

        public readonly Color Blend(Color background)
        {
            float alpha = Math.Clamp(A, 0f, 1f);
            return ((Color)this * alpha) + ((1f - alpha) * background);
        }

        public void Clamp()
        {
            R = Math.Clamp(R, 0f, 1f);
            G = Math.Clamp(G, 0f, 1f);
            B = Math.Clamp(B, 0f, 1f);
            A = Math.Clamp(A, 0f, 1f);
        }
        public readonly TransparentColor Clamped => new(Math.Clamp(R, 0f, 1f), Math.Clamp(G, 0f, 1f), Math.Clamp(B, 0f, 1f), Math.Clamp(A, 0f, 1f));

        public override readonly bool Equals(object? obj) =>
            obj is TransparentColor color &&
            Equals(color);
        public readonly bool Equals(TransparentColor other) =>
            R == other.R &&
            G == other.G &&
            B == other.B &&
            A == other.A;
        public readonly bool Equals(Color other) =>
            R == other.R &&
            G == other.G &&
            B == other.B &&
            A == 1f;
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

        public static bool operator ==(TransparentColor left, TransparentColor right) => left.Equals(right);
        public static bool operator !=(TransparentColor left, TransparentColor right) => !(left == right);

        public static bool operator ==(TransparentColor left, Color right) => left.Equals(right);
        public static bool operator !=(TransparentColor left, Color right) => !(left == right);

        public static bool operator ==(Color left, TransparentColor right) => right.Equals(left);
        public static bool operator !=(Color left, TransparentColor right) => !(right == left);

        public static TransparentColor operator +(TransparentColor a, TransparentColor b)
            => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
        public static TransparentColor operator -(TransparentColor a, TransparentColor b)
            => new(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
        public static TransparentColor operator *(TransparentColor a, TransparentColor b)
            => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
        public static TransparentColor operator /(TransparentColor a, TransparentColor b)
            => new(a.R / b.R, a.G / b.G, a.B / b.B, a.A / b.A);

        public static TransparentColor operator *(TransparentColor a, float b)
            => new(a.R * b, a.G * b, a.B * b, a.A * b);
        public static TransparentColor operator /(TransparentColor a, float b)
            => new(a.R / b, a.G / b, a.B / b, a.A / b);

        public static TransparentColor operator *(float a, TransparentColor b)
            => new(a * b.R, a * b.G, a * b.B, a * b.A);

        public static implicit operator TransparentColor(Color v)
            => new(v.R, v.G, v.B, 1f);

        public static explicit operator Color(TransparentColor v)
            => new(v.R, v.G, v.B);

        public override readonly string ToString() => $"({R:0.00}, {G:0.00}, {B:0.00}, {A:0.00})";
        readonly string GetDebuggerDisplay() => ToString();

        /// <summary>
        /// return the (squared) Euclidean distance between two colors
        /// </summary>
        public static float Distance(TransparentColor a, TransparentColor b)
        {
            TransparentColor d = a - b;
            return (d.R * d.R) + (d.G * d.G) + (d.B * d.B) + (d.A * d.A);
        }
    }
}
