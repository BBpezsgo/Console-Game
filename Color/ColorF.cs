using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct ColorF :
        IEquatable<ColorF>,
        IEqualityOperators<ColorF, ColorF, bool>,

        IAdditionOperators<ColorF, ColorF, ColorF>,
        IDivisionOperators<ColorF, ColorF, ColorF>,
        IMultiplyOperators<ColorF, ColorF, ColorF>,
        ISubtractionOperators<ColorF, ColorF, ColorF>,

        IDivisionOperators<ColorF, float, ColorF>,
        IMultiplyOperators<ColorF, float, ColorF>,

        IParsable<ColorF>
    {
        public float R;
        public float G;
        public float B;

        public readonly float Luminance => (0.2126f * R) + (0.7152f * G) + (0.0722f * B);
        public readonly float Intensity => (R + G + B) / 3f;
        /// <summary>
        /// Source: .NET 7 source code
        /// </summary>
        public readonly float Saturation
        {
            get
            {
                if (R == G && G == B) return 0f;

                float min = MinChannel;
                float max = MaxChannel;

                float div = max + min;
                if (div > 1f)
                { div = 2f - max - min; }

                return (max - min) / div;

                float l = (min + max) * .5f;

                if (l is <= float.Epsilon or >= 1f)
                { return 0f; }

                float s = max - min;

                if (s <= 0f)
                { return s; }

                if (l <= 0.5f)
                { return s / (max + min); }
                else
                { return s / (2f - max - min); }
            }
        }
        public readonly float Lightness => (MinChannel + MaxChannel) * .5f;
        public readonly float Chroma => MaxChannel - MinChannel;
        public readonly float MaxChannel => Math.Max(R, Math.Max(G, B));
        public readonly float MinChannel => Math.Min(R, Math.Min(G, B));
        /// <summary>
        /// Source: .NET 7 source code
        /// </summary>
        public readonly float Hue
        {
            get
            {
                if (R == G && G == B) return 0f;

                float min = MinChannel;
                float max = MaxChannel;

                float delta = max - min;
                float hue;

                if (R == max)
                { hue = (G - B) / delta; }
                else if (G == max)
                { hue = (B - R) / delta + 2f; }
                else
                { hue = (R - G) / delta + 4f; }

                hue *= 60f;
                if (hue < 0f)
                { hue += 360f; }

                return hue;
            }
        }

        public static ColorF Zero => new(0f);
        public static ColorF One => new(1f);

        public ColorF(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
        public ColorF(float v)
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
        public readonly ColorF Clamped => new(Math.Clamp(R, 0f, 1f), Math.Clamp(G, 0f, 1f), Math.Clamp(B, 0f, 1f));

        public override readonly bool Equals(object? obj) =>
            obj is ColorF color &&
            Equals(color);
        public readonly bool Equals(ColorF other) =>
            R == other.R &&
            G == other.G &&
            B == other.B;
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

        public static bool operator ==(ColorF left, ColorF right) => left.Equals(right);
        public static bool operator !=(ColorF left, ColorF right) => !left.Equals(right);

        public static ColorF operator +(ColorF a, ColorF b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static ColorF operator -(ColorF a, ColorF b) => new(a.R - b.R, a.G - b.G, a.B - b.B);
        public static ColorF operator *(ColorF a, ColorF b) => new(a.R * b.R, a.G * b.G, a.B * b.B);
        public static ColorF operator /(ColorF a, ColorF b) => new(a.R / b.R, a.G / b.G, a.B / b.B);

        public static ColorF operator *(ColorF a, float b) => new(a.R * b, a.G * b, a.B * b);
        public static ColorF operator /(ColorF a, float b) => new(a.R / b, a.G / b, a.B / b);

        public static ColorF operator *(float a, ColorF b) => new(a * b.R, a * b.G, a * b.B);

        public override readonly string ToString() => $"({R:0.00}, {G:0.00}, {B:0.00})";
        readonly string GetDebuggerDisplay() => ToString();

        /// <summary>
        /// return the (squared) Euclidean distance between two colors
        /// </summary>
        public static float Distance(ColorF a, ColorF b)
        {
            ColorF d = a - b;
            return (d.R * d.R) + (d.G * d.G) + (d.B * d.B);
        }

        /// <exception cref="FormatException"/>
        public static ColorF Parse(string s, IFormatProvider? provider)
        {
            ColorF color = default;
            s = s.Trim();
            string[] parts = s.Split(' ');

            if (parts.Length != 3)
            { throw new FormatException(); }

            if (!float.TryParse(parts[0], NumberStyles.Float, provider, out color.R))
            { throw new FormatException(); }
            if (!float.TryParse(parts[1], NumberStyles.Float, provider, out color.G))
            { throw new FormatException(); }
            if (!float.TryParse(parts[2], NumberStyles.Float, provider, out color.B))
            { throw new FormatException(); }

            return color;
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ColorF result)
        {
            result = default;

            if (s == null) return false;

            s = s.Trim();
            string[] parts = s.Split(' ');

            if (parts.Length != 3)
            { return false; }

            if (!float.TryParse(parts[0], NumberStyles.Float, provider, out result.R))
            { return false; }
            if (!float.TryParse(parts[1], NumberStyles.Float, provider, out result.G))
            { return false; }
            if (!float.TryParse(parts[2], NumberStyles.Float, provider, out result.B))
            { return false; }

            return true;
        }

        public readonly ColorF NormalizeIntensity()
        {
            float maxChannel = MaxChannel;
            if (maxChannel == 0)
            { return default; }

            float r = Math.Clamp(R / maxChannel, 0, 1);
            float g = Math.Clamp(G / maxChannel, 0, 1);
            float b = Math.Clamp(B / maxChannel, 0, 1);
            return new ColorF(r, g, b);
        }

        public static ColorF Random()
        {
            int r = System.Random.Shared.Next();
            int g = System.Random.Shared.Next();
            int b = System.Random.Shared.Next();
            return new ColorF((float)r / (float)int.MaxValue, (float)g / (float)int.MaxValue, (float)b / (float)int.MaxValue);
        }
    }
}
