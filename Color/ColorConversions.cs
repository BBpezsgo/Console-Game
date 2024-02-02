using Win32;

namespace ConsoleGame
{
    public partial struct Color
    {
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
            l = (m + v) / 2.0f;
            if (l <= 0.0f)
            {
                return;
            }
            vm = v - m;
            s = vm;
            if (s > 0.0f)
            {
                s /= (l <= 0.5f) ? (v + m) : (2.0f - v - m);
            }
            else
            {
                return;
            }
            r2 = (v - r) / vm;
            g2 = (v - g) / vm;
            b2 = (v - b) / vm;
            if (r == v)
            {
                h = (g == m ? 5.0f + b2 : 1.0f - g2);
            }
            else if (g == v)
            {
                h = (b == m ? 1.0f + r2 : 3.0f - b2);
            }
            else
            {
                h = (r == m ? 3.0f + g2 : 5.0f - r2);
            }
            h /= 6.0f;
        }

        #endregion

        #region 4bit IRGB

        public static readonly Color[] Irgb4bitColors = new Color[0b_1_0000]
        {
            (Color)(new Color24(0, 0, 0)),          // 0b_0000
            (Color)(new Color24(0, 0, 128)),        // 0b_0001
            (Color)(new Color24(0, 128, 0)),        // 0b_0010
            (Color)(new Color24(0, 128, 128)),      // 0b_0011
            (Color)(new Color24(128, 0, 0)),        // 0b_0100
            (Color)(new Color24(128, 0, 128)),      // 0b_0101
            (Color)(new Color24(128, 128, 0)),      // 0b_0110
            (Color)(new Color24(192, 192, 192)),    // 0b_0111
            (Color)(new Color24(128, 128, 128)),    // 0b_1000
            (Color)(new Color24(0, 0, 255)),        // 0b_1001
            (Color)(new Color24(0, 255, 0)),        // 0b_1010
            (Color)(new Color24(0, 255, 255)),      // 0b_1011
            (Color)(new Color24(255, 0, 0)),        // 0b_1100
            (Color)(new Color24(255, 0, 255)),      // 0b_1101
            (Color)(new Color24(255, 255, 0)),      // 0b_1110
            (Color)(new Color24(255, 255, 255)),    // 0b_1111
        };

        public static Color From4bitIRGB(byte irgb) => Irgb4bitColors[irgb];

        public static Color From4bitIRGB(byte r, byte g, byte b, byte i) => Irgb4bitColors[((i & 1) << 3) | ((r & 1) << 2) | ((g & 1) << 1) | (b & 1)];

        /// <summary>
        /// <para>
        /// Find the closest 4-bit RGBI approximation (by Euclidean distance) to a 24-bit RGB color
        /// </para>
        /// <para>
        /// Source: <see href="https://stackoverflow.com/questions/41644778/convert-24-bit-color-to-4-bit-rgbi"/>
        /// </para>
        /// </summary>
        public static byte To4bitIRGB(Color color)
        {
            /// <summary>
            /// Find the closest RGBx approximation of a 24-bit RGB color, for x = 0 or 1
            /// </summary>
            static (byte R, byte G, byte B) RgbxApprox(Color color, byte x)
            {
                float threshold = (x + 1f) / 3f;
                byte r = color.R > threshold ? (byte)1 : (byte)0;
                byte g = color.G > threshold ? (byte)1 : (byte)0;
                byte b = color.B > threshold ? (byte)1 : (byte)0;
                return (r, g, b);
            }

            // find best RGB0 and RGB1 approximations:
            (byte r0, byte g0, byte b0) = RgbxApprox(color, 0);
            (byte r1, byte g1, byte b1) = RgbxApprox(color, 1);

            // convert them back to 24-bit RGB:
            Color color1 = Color.From4bitIRGB(r0, g0, b0, 0);
            Color color2 = Color.From4bitIRGB(r1, g1, b1, 1);

            // return the color closer to the original:
            float d0 = Color.Distance(color, color1);
            float d1 = Color.Distance(color, color2);

            byte result = 0b_0000;

            if (d0 <= d1)
            {
                result |= 0b_0000;
                if (r0 != 0)
                { result |= 0b_0100; }
                if (g0 != 0)
                { result |= 0b_0010; }
                if (b0 != 0)
                { result |= 0b_0001; }
            }
            else
            {
                result |= 0b_1000;
                if (r1 != 0)
                { result |= 0b_0100; }
                if (g1 != 0)
                { result |= 0b_0010; }
                if (b1 != 0)
                { result |= 0b_0001; }
            }

            return result;
        }

        static readonly (char Character, float Intensity)[] ShadeCharacters = new (char Character, float Intensity)[]
        {
            ( '░', .25f ),
            ( '▒', .50f ),
            ( '▓', .75f ),
        };

        public static Win32.ConsoleChar ToCharacterShaded(Color color)
        {
            float shade = color.MaxChannel;

            if (shade < .125f)
            { return Win32.ConsoleChar.Empty; }

            byte c = Color.To4bitIRGB(color);

            if (shade > .875f)
            { return new Win32.ConsoleChar(' ', (ushort)(c << 4)); }

            return new Win32.ConsoleChar(Ascii.BlockShade[(int)MathF.Round(shade * (Ascii.BlockShade.Length - 1))], c);
        }

        public static Win32.ConsoleChar ToCharacterColored(Color color)
        {
            Win32.ConsoleChar result = Win32.ConsoleChar.Empty;
            float smallestDist = float.PositiveInfinity;
            Color fgC, bgC;
            float dist;
            float shade;
            byte fg, bg;

            for (fg = 0; fg <= CharColor.White; fg++)
            {
                fgC = Irgb4bitColors[fg];

                {
                    dist = Color.Distance(fgC, color);
                    if (smallestDist > dist)
                    {
                        smallestDist = dist;
                        result = new Win32.ConsoleChar(' ', 0, fg);
                    }
                    if (dist <= float.Epsilon) return result;
                }

                for (bg = (byte)(fg + 1); bg <= CharColor.White; bg++)
                {
                    bgC = Irgb4bitColors[bg];

                    for (int i = 0; i < ShadeCharacters.Length; i++)
                    {
                        shade = ShadeCharacters[i].Intensity;
                        dist = Color.Distance((fgC * shade) + (bgC * (1f - shade)), color);
                        if (smallestDist > dist)
                        {
                            smallestDist = dist;
                            result = new Win32.ConsoleChar(ShadeCharacters[i].Character, fg, bg);
                        }
                        if (dist <= float.Epsilon) return result;
                    }
                }
            }

            return result;
        }

        public static Color FromCharacter(Win32.ConsoleChar character)
        {
            float shade = character.Char switch
            {
                '░' => .25f,
                '▒' => .50f,
                '▓' => .75f,
                ' ' => 0f,
                _ => .5f,
            };

            Color bg = Color.From4bitIRGB(character.Background);
            Color fg = Color.From4bitIRGB(character.Foreground);

            return (bg * (1f - shade)) + (fg * shade);
        }

        public static byte To4bitIRGB_BruteForce(Color color)
        {
            byte closest = 0b_0000;
            float closestDistance = float.PositiveInfinity;

            for (byte irgb = 0b_0000; irgb <= 0b_1111; irgb++)
            {
                Color rgb = Color.From4bitIRGB(irgb);
                float distance = Color.Distance(rgb, color);

                if (distance <= 0.05f)
                {
                    return irgb;
                }

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = irgb;
                }
            }

            return closest;
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

        public static ConsoleColor ToConsoleColor(Color color)
        {
            byte irgb = Color.To4bitIRGB(color);
            return ConsoleColors[irgb];
        }

        public static Color FromConsoleColor(ConsoleColor color)
        {
            byte irgb = (byte)Array.IndexOf(ConsoleColors, color);
            return Color.From4bitIRGB(irgb);
        }

        public static ConsoleColor ToConsoleColor(byte colorIRGB)
            => ConsoleColors[colorIRGB];

        #endregion
    }
}
