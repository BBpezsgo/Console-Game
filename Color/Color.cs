using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Color :
        IEquatable<Color>,
        IEqualityOperators<Color, Color, bool>,

        IAdditionOperators<Color, Color, Color>,
        IDivisionOperators<Color, Color, Color>,
        IMultiplyOperators<Color, Color, Color>,
        ISubtractionOperators<Color, Color, Color>,

        IDivisionOperators<Color, float, Color>,
        IMultiplyOperators<Color, float, Color>,

        IParsable<Color>
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

        public static Color Zero => new(0f);
        public static Color One => new(1f);

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
        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public static Color operator +(Color a, Color b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Color operator -(Color a, Color b) => new(a.R - b.R, a.G - b.G, a.B - b.B);
        public static Color operator *(Color a, Color b) => new(a.R * b.R, a.G * b.G, a.B * b.B);
        public static Color operator /(Color a, Color b) => new(a.R / b.R, a.G / b.G, a.B / b.B);

        public static Color operator *(Color a, float b) => new(a.R * b, a.G * b, a.B * b);
        public static Color operator /(Color a, float b) => new(a.R / b, a.G / b, a.B / b);

        public static Color operator *(float a, Color b) => new(a * b.R, a * b.G, a * b.B);

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

        /// <exception cref="FormatException"/>
        public static Color Parse(string s, IFormatProvider? provider)
        {
            Color color = default;
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

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Color result)
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

        public readonly Color NormalizeIntensity()
        {
            float maxChannel = MaxChannel;
            if (maxChannel == 0)
            { return default; }

            float r = Math.Clamp(R / maxChannel, 0, 1);
            float g = Math.Clamp(G / maxChannel, 0, 1);
            float b = Math.Clamp(B / maxChannel, 0, 1);
            return new Color(r, g, b);
        }

        public static Color Random()
        {
            int r = System.Random.Shared.Next();
            int g = System.Random.Shared.Next();
            int b = System.Random.Shared.Next();
            return new Color((float)r / (float)int.MaxValue, (float)g / (float)int.MaxValue, (float)b / (float)int.MaxValue);
        }
    }
}
