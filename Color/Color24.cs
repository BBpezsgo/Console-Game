using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Color24 :
        IEquatable<Color24>,
        IEqualityOperators<Color24, Color24, bool>,

        IAdditionOperators<Color24, Color24, Color24>,
        IDivisionOperators<Color24, Color24, Color24>,
        IMultiplyOperators<Color24, Color24, Color24>,
        ISubtractionOperators<Color24, Color24, Color24>,

        IDivisionOperators<Color24, int, Color24>,
        IMultiplyOperators<Color24, int, Color24>,

        IDivisionOperators<Color24, float, Color>,
        IMultiplyOperators<Color24, float, Color>,

        IParsable<Color24>
    {
        public byte R;
        public byte G;
        public byte B;

        public static Color24 Zero => new(0);
        public static Color24 One => new(1);

        public static Color24 Min => new(byte.MinValue);
        public static Color24 Max => new(byte.MaxValue);

        public readonly byte MaxChannel => Math.Max(R, Math.Max(G, B));
        public readonly byte MinChannel => Math.Min(R, Math.Min(G, B));

        const float Lum_R = 0.2126f / (float)byte.MaxValue;
        const float Lum_G = 0.7152f / (float)byte.MaxValue;
        const float Lum_B = 0.0722f / (float)byte.MaxValue;

        public readonly float Luminance => (Lum_R * R) + (Lum_G * G) + (Lum_B * B);
        public readonly byte Intensity => (byte)((R + G + B) / 3);

        public readonly float Brightness => (float)(MaxChannel + MinChannel) / (float)(byte.MaxValue * 2);

        /// <summary>
        /// Source: .NET 7 source code
        /// </summary>
        public readonly float Hue
        {
            get
            {
                if (R == G && G == B) return 0f;

                byte max = MaxChannel;
                byte min = MinChannel;

                int delta = max - min;
                float hue;

                if (R == max)
                { hue = (float)(G - B) / (float)delta; }
                else if (G == max)
                { hue = (float)(B - R) / (float)delta + 2f; }
                else
                { hue = (float)(R - G) / (float)delta + 4f; }

                hue *= 60f;
                if (hue < 0f)
                { hue += 360f; }

                return hue;
            }
        }

        /// <summary>
        /// Source: .NET 7 source code
        /// </summary>
        public readonly float Saturation
        {
            get
            {
                if (R == G && G == B) return 0f;

                byte max = MaxChannel;
                byte min = MinChannel;

                int div = max + min;
                if (div > byte.MaxValue)
                { div = byte.MaxValue * 2 - max - min; }

                return (float)(max - min) / (float)div;
            }
        }

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

        public Color24(int v)
        {
            R = checked((byte)v);
            G = checked((byte)v);
            B = checked((byte)v);
        }

        /// <summary>
        /// return the (squared) Euclidean distance between two colors
        /// </summary>
        public static int Distance(Color24 a, Color24 b)
        {
            Color24 d = a - b;
            return (d.R * d.R) + (d.G * d.G) + (d.B * d.B);
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

        public static Color24 operator +(Color24 a, Color24 b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Color24 operator -(Color24 a, Color24 b) => new(a.R - b.R, a.G - b.G, a.B - b.B);
        public static Color24 operator *(Color24 a, Color24 b) => new(a.R * b.R, a.G * b.G, a.B * b.B);
        public static Color24 operator /(Color24 a, Color24 b) => new(a.R / b.R, a.G / b.G, a.B / b.B);

        public static Color24 operator *(Color24 a, int b) => new(a.R * b, a.G * b, a.B * b);
        public static Color24 operator /(Color24 a, int b) => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(int a, Color24 b) => new(a * b.R, a * b.G, a * b.B);

        public static Color operator *(Color24 a, float b) => new(a.R * b, a.G * b, a.B * b);
        public static Color operator /(Color24 a, float b) => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(float a, Color24 b) => new(a * b.R, a * b.G, a * b.B);

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

        /// <exception cref="FormatException"/>
        public static Color24 Parse(string s, IFormatProvider? provider)
        {
            Color24 color = default;
            s = s.Trim();
            string[] parts = s.Split(' ');

            if (parts.Length != 3)
            { throw new FormatException(); }

            if (!byte.TryParse(parts[0], NumberStyles.Integer, provider, out color.R))
            { throw new FormatException(); }
            if (!byte.TryParse(parts[1], NumberStyles.Integer, provider, out color.G))
            { throw new FormatException(); }
            if (!byte.TryParse(parts[2], NumberStyles.Integer, provider, out color.B))
            { throw new FormatException(); }

            return color;
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Color24 result)
        {
            result = default;

            if (s == null) return false;

            s = s.Trim();
            string[] parts = s.Split(' ');

            if (parts.Length != 3)
            { return false; }

            if (!byte.TryParse(parts[0], NumberStyles.Integer, provider, out result.R))
            { return false; }
            if (!byte.TryParse(parts[1], NumberStyles.Integer, provider, out result.G))
            { return false; }
            if (!byte.TryParse(parts[2], NumberStyles.Integer, provider, out result.B))
            { return false; }

            return true;
        }

        unsafe public static Color24 Random()
        {
            byte* bufferPtr = stackalloc byte[3];
            Span<byte> buffer = new(bufferPtr, 3);
            System.Random.Shared.NextBytes(buffer);
            return new Color24(buffer[0], buffer[1], buffer[2]);
        }
    }
}
