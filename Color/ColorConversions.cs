using Win32;

namespace ConsoleGame
{
    public partial struct Color
    {
        public static explicit operator byte(Color v) => CharColor.From24bitColor(v);
        public static explicit operator Color(byte v) => CharColor.To24bitColor(v);

        public static implicit operator System.Drawing.Color(Color v) => (System.Drawing.Color)(Color24)v;
        public static implicit operator Color(System.Drawing.Color v) => new(v.R / 255f, v.G / 255f, v.B / 255f);

        public static implicit operator Win32.Gdi32.GdiColor(Color v) => (Win32.Gdi32.GdiColor)(Color24)v;
        public static implicit operator Color(Win32.Gdi32.GdiColor v) => new(v.R / 255f, v.G / 255f, v.B / 255f);

        #region HSL

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static Color FromHSL(float h, float sl, float l)
        {
            float v = (l <= 0.5f) ? (l * (1f + sl)) : (l + sl - (l * sl));

            if (v <= 0f)
            { return new Color(l, l, l); }

            float m;
            float sv;
            int sextant;
            float fract, vsf, mid1, mid2;

            m = l + l - v;
            sv = (v - m) / v;
            h *= 6f;
            sextant = (int)h;
            fract = h - sextant;
            vsf = v * sv * fract;
            mid1 = m + vsf;
            mid2 = v - vsf;
            return sextant switch
            {
                0 => new Color(v, mid1, m),
                1 => new Color(mid2, v, m),
                2 => new Color(m, v, mid1),
                3 => new Color(m, mid2, v),
                4 => new Color(mid1, m, v),
                5 => new Color(v, m, mid2),
                _ => default,
            };
        }

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static (float H, float S, float L) ToHsl(Color color)
        {
            ToHsl(color, out float h, out float s, out float l);
            return (h, s, l);
        }

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static void ToHsl(Color color, out float h, out float s, out float l)
        {
            float r = color.R;
            float g = color.G;
            float b = color.B;
            float v;
            float m;
            float vm;
            float r2, g2, b2;

            h = 0; // default to black
            s = 0;
            v = Math.Max(r, g);
            v = Math.Max(v, b);
            m = Math.Min(r, g);
            m = Math.Min(m, b);
            l = (m + v) / 2f;

            if (l <= 0f)
            { return; }

            vm = v - m;
            s = vm;

            if (s > 0f)
            { s /= (l <= 0.5f) ? (v + m) : (2f - v - m); }
            else
            { return; }

            r2 = (v - r) / vm;
            g2 = (v - g) / vm;
            b2 = (v - b) / vm;

            if (r == v)
            { h = g == m ? 5f + b2 : 1f - g2; }
            else if (g == v)
            { h = b == m ? 1f + r2 : 3f - b2; }
            else
            { h = r == m ? 3f + g2 : 5f - r2; }

            h /= 6f;
        }

        #endregion

        #region ConsoleColor

        static readonly ConsoleColor[] ConsoleColors = new ConsoleColor[0b_1_0000]
        {
            ConsoleColor.Black,         // 0b_0000
            ConsoleColor.DarkBlue,      // 0b_0001
            ConsoleColor.DarkGreen,     // 0b_0010
            ConsoleColor.DarkCyan,      // 0b_0011
            ConsoleColor.DarkRed,       // 0b_0100
            ConsoleColor.DarkMagenta,   // 0b_0101
            ConsoleColor.DarkYellow,    // 0b_0110
            ConsoleColor.Gray,          // 0b_0111
            ConsoleColor.DarkGray,      // 0b_1000
            ConsoleColor.Blue,          // 0b_1001
            ConsoleColor.Green,         // 0b_1010
            ConsoleColor.Cyan,          // 0b_1011
            ConsoleColor.Red,           // 0b_1100
            ConsoleColor.Magenta,       // 0b_1101
            ConsoleColor.Yellow,        // 0b_1110
            ConsoleColor.White,         // 0b_1111
        };

        public static ConsoleColor ToConsoleColor(Color color) => ConsoleColors[CharColor.From24bitColor(color)];

        public static Color FromConsoleColor(ConsoleColor color) => CharColor.To24bitColor((byte)Array.IndexOf(ConsoleColors, color));

        public static ConsoleColor ToConsoleColor(byte colorIRGB) => ConsoleColors[colorIRGB];

        #endregion
    }
}
