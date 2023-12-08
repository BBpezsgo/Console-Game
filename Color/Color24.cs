using System.Diagnostics;
using System.Globalization;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Color24 : IEquatable<Color24>
    {
        public byte R;
        public byte G;
        public byte B;

        public Color24(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color24(int r, int g, int b)
        {
            R = checked((byte)r);
            G = checked((byte)g);
            B = checked((byte)b);
        }

        public Color24(byte v)
        {
            R = v;
            G = v;
            B = v;
        }

        public override readonly bool Equals(object? obj) =>
            obj is Color24 color &&
            Equals(color);
        public readonly bool Equals(Color24 other) =>
            R == other.R &&
            G == other.G &&
            B == other.B;
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

        public static bool operator ==(Color24 left, Color24 right) => left.Equals(right);
        public static bool operator !=(Color24 left, Color24 right) => !left.Equals(right);

        public static Color24 operator +(Color24 a, Color24 b)
            => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Color24 operator -(Color24 a, Color24 b)
            => new(a.R - b.R, a.G - b.G, a.B - b.B);
        public static Color24 operator *(Color24 a, Color24 b)
            => new(a.R * b.R, a.G * b.G, a.B * b.B);
        public static Color24 operator /(Color24 a, Color24 b)
            => new(a.R / b.R, a.G / b.G, a.B / b.B);

        public static Color operator *(Color24 a, int b)
            => new(a.R * b, a.G * b, a.B * b);
        public static Color operator /(Color24 a, int b)
            => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(int a, Color24 b)
            => new(a * b.R, a * b.G, a * b.B);

        public static Color operator *(Color24 a, float b)
            => new(a.R * b, a.G * b, a.B * b);
        public static Color operator /(Color24 a, float b)
            => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(float a, Color24 b)
            => new(a * b.R, a * b.G, a * b.B);

        public override readonly string ToString() => $"({R}, {G}, {B})";
        readonly string GetDebuggerDisplay() => ToString();

        public static bool TryParse(string text, out Color24 color)
        {
            color = default;
            text = text.Trim();
            string[] parts = text.Split(' ');

            if (parts.Length != 3)
            { return false; }

            if (!byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out color.R))
            { return false; }
            if (!byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out color.G))
            { return false; }
            if (!byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out color.B))
            { return false; }

            return true;
        }
    }
}
