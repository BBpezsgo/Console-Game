using Win32;

namespace ConsoleGame
{
    public partial struct ColorF
    {
        public static explicit operator Win32.Gdi32.GdiColor(ColorF v) => new(v.R, v.G, v.B);
        public static implicit operator ColorF(Win32.Gdi32.GdiColor v) => new(v.R / 255f, v.G / 255f, v.B / 255f);

        public static implicit operator ValueTuple<float, float, float>(ColorF v) => new(v.R, v.G, v.B);
        public static implicit operator ColorF(ValueTuple<float, float, float> v) => new(v.Item1, v.Item2, v.Item3);

        #region HSL

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static ColorF FromHSL(float h, float sl, float l)
        {
            float v = (l <= 0.5f) ? (l * (1f + sl)) : (l + sl - (l * sl));

            if (v <= 0f)
            { return new ColorF(l, l, l); }

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
                0 => new ColorF(v, mid1, m),
                1 => new ColorF(mid2, v, m),
                2 => new ColorF(m, v, mid1),
                3 => new ColorF(m, mid2, v),
                4 => new ColorF(mid1, m, v),
                5 => new ColorF(v, m, mid2),
                _ => default,
            };
        }

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static (float H, float S, float L) ToHsl(ColorF color)
        {
            ToHsl(color, out float h, out float s, out float l);
            return (h, s, l);
        }

        /// <summary>
        /// Source: <see href="https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm"/>
        /// </summary>
        public static void ToHsl(ColorF color, out float h, out float s, out float l)
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
    }
}
