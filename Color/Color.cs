using System.Diagnostics;
using System.Globalization;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Color : IEquatable<Color>
    {
        public float R;
        public float G;
        public float B;

        public readonly float Luminance => (0.2126f * R) + (0.7152f * G) + (0.0722f * B);
        public readonly bool Overflow => R > 1f || G > 1f || B > 1f;
        public readonly float Intensity => MathF.Sqrt((R * R) + (G * G) + (B * B));

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
        public Color(float v)
        {
            R = v;
            G = v;
            B = v;
        }

        public void Clamp()
        {
            R = Math.Clamp(R, 0f, 1f);
            G = Math.Clamp(G, 0f, 1f);
            B = Math.Clamp(B, 0f, 1f);
        }
        public readonly Color Clamped => new(Math.Clamp(R, 0f, 1f), Math.Clamp(G, 0f, 1f), Math.Clamp(B, 0f, 1f));

        public override readonly bool Equals(object? obj) =>
            obj is Color color &&
            Equals(color);
        public readonly bool Equals(Color other) =>
            R == other.R &&
            G == other.G &&
            B == other.B;
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !(left == right);

        public static Color operator +(Color a, Color b)
            => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Color operator -(Color a, Color b)
            => new(a.R - b.R, a.G - b.G, a.B - b.B);
        public static Color operator *(Color a, Color b)
            => new(a.R * b.R, a.G * b.G, a.B * b.B);
        public static Color operator /(Color a, Color b)
            => new(a.R / b.R, a.G / b.G, a.B / b.B);

        public static Color operator *(Color a, float b)
            => new(a.R * b, a.G * b, a.B * b);
        public static Color operator /(Color a, float b)
            => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(float a, Color b)
            => new(a * b.R, a * b.G, a * b.B);

        public static explicit operator byte(Color v) => Color.To4bitIRGB(v);
        public static explicit operator Color(byte v) => Color.From4bitIRGB(v);

        public override readonly string ToString() => $"({R:0.00}, {G:0.00}, {B:0.00})";
        readonly string GetDebuggerDisplay() => ToString();

        /// <summary>
        /// return the (squared) Euclidean distance between two colors
        /// </summary>
        public static float Distance(Color a, Color b)
        {
            Color d = a - b;
            return (d.R * d.R) + (d.G * d.G) + (d.B * d.B);
        }

        public static bool TryParse(string text, out Color color)
        {
            color = default;
            text = text.Trim();
            string[] parts = text.Split(' ');

            if (parts.Length != 3)
            { return false; }

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out color.R))
            { return false; }
            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out color.G))
            { return false; }
            if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out color.B))
            { return false; }

            return true;
        }
    }
}
