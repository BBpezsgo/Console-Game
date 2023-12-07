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
            float v;
            float r, g, b;

            r = l; // default to gray
            g = l;

            b = l;
            v = (l <= 0.5f) ? (l * (1.0f + sl)) : (l + sl - (l * sl));

            if (v > 0)
            {
                float m;
                float sv;
                int sextant;
                float fract, vsf, mid1, mid2;

                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0f;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }

            return new Color(r, g, b);
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

        #region 24bit RGB

        public static Color From24bitRGB(int r, int g, int b) => new(
            (float)r / (float)byte.MaxValue,
            (float)g / (float)byte.MaxValue,
            (float)b / (float)byte.MaxValue);

        public static (byte R, byte G, byte B) To24bitRGB(Color color)
        {
            Color clamped = color.Clamped;
            byte r = (byte)MathF.Round(clamped.R * byte.MaxValue);
            byte g = (byte)MathF.Round(clamped.G * byte.MaxValue);
            byte b = (byte)MathF.Round(clamped.B * byte.MaxValue);
            return (r, g, b);
        }

        #endregion

        #region 8bit RGB

        /// <summary>
        /// Source: <see href="https://stackoverflow.com/questions/41420215/single-byte-to-rgb-and-rgb-to-single-byte"/>
        /// </summary>
        public static byte To8bitRGB(Color color)
        {
            (byte r, byte g, byte b) = Color.To24bitRGB(color);
            return (byte)(((r / 32) << 5) + ((g / 32) << 2) + (b / 64));
        }

        /// <summary>
        /// Source: <see href="https://stackoverflow.com/questions/41420215/single-byte-to-rgb-and-rgb-to-single-byte"/>
        /// </summary>
        public static Color From8bitRGB(byte color)
        {
            byte R = (byte)((color & 0b_111_000_00) >> 5);
            byte G = (byte)((color & 0b_000_111_00) >> 2);
            byte B = (byte)(color & 0b_000_000_11);
            return Color.From24bitRGB(R, G, B);
        }

        #endregion

        #region 4bit IRGB

        static readonly Color[] Irgb4bitColors = new Color[0b_1_0000]
        {
            Color.From24bitRGB(0, 0, 0), // 0b_0000
            Color.From24bitRGB(0, 0, 128), // 0b_0001
            Color.From24bitRGB(0, 128, 0), // 0b_0010
            Color.From24bitRGB(0, 128, 128), // 0b_0011
            Color.From24bitRGB(128, 0, 0), // 0b_0100
            Color.From24bitRGB(128, 0, 128), // 0b_0101
            Color.From24bitRGB(128, 128, 0), // 0b_0110
            Color.From24bitRGB(192, 192, 192), // 0b_0111
            Color.From24bitRGB(128, 128, 128), // 0b_1000
            Color.From24bitRGB(0, 0, 255), // 0b_1001
            Color.From24bitRGB(0, 255, 0), // 0b_1010
            Color.From24bitRGB(0, 255, 255), // 0b_1011
            Color.From24bitRGB(255, 0, 0), // 0b_1100
            Color.From24bitRGB(255, 0, 255), // 0b_1101
            Color.From24bitRGB(255, 255, 0), // 0b_1110
            Color.From24bitRGB(255, 255, 255), // 0b_1111
        };

        public static Color From4bitIRGB(byte irgb)
        {
            return Irgb4bitColors[irgb];
            /*
            if (irgb == 0b_0111)
            { return Color.FromRGB(192, 192, 192); }

            if (irgb == 0b_1000)
            { return Color.FromRGB(128, 128, 128); }

            int intensity = (irgb & 0b_1000) >> 3;
            int r = (irgb & 0b_0100) >> 2;
            int g = (irgb & 0b_0010) >> 1;
            int b = (irgb & 0b_0001) >> 0;

            if (intensity == 0)
            { return new Color(r, g, b) * Ratio1; }
            else
            { return new Color(r, g, b); }
            */
        }

        public static Color From4bitIRGB(byte r, byte g, byte b, byte i)
            => Color.From4bitIRGB((byte)((i << 3) | (r << 2) | (g << 1) | (b)));

        public static byte To4bitIRGB(Color color)
        {
            return RgbiApprox(color);
            /*
            byte result = 0b_0000;

            // if (color.Luminance > .3f)
            if (color.R > Ratio1 && color.G > Ratio1 || color.B > Ratio1)
            { result |= 0b_1000; }

            if (color.R > Ratio2)
            { result |= 0b_0100; }

            if (color.G > Ratio2)
            { result |= 0b_0010; }

            if (color.B > Ratio2)
            { result |= 0b_0001; }

            return result;
            */
        }

        static readonly (char Character, float Intensity)[] ShadeCharacters = new (char Character, float Intensity)[]
        {
            ( '░', .25f ),
            ( '▒', .50f ),
            ( '▓', .75f ),
        };

        public static Win32.ConsoleChar ToCharacterShaded(Color color)
        {
            Win32.ConsoleChar result = new(' ');

            float shade = color.Intensity;

            if (shade <= float.Epsilon)
            { return result; }

            byte c = Color.To4bitIRGB(color);
            
            if (shade >= 1f)
            { return new Win32.ConsoleChar(' ', (ushort)(c << 4)); }

            return new Win32.ConsoleChar(Ascii.BlockShade[(int)MathF.Round(shade * (Ascii.BlockShade.Length - 1))], c);
        }

        public static Win32.ConsoleChar ToCharacterColored(Color color)
        {
            Win32.ConsoleChar result = new(' ');
            float smallestDist = float.PositiveInfinity;

            for (byte c1 = 0; c1 <= ByteColor.White; c1++)
            {
                Color c1a = Color.From4bitIRGB(c1);
                
                {
                    float dist = Color.Distance(c1a, color);
                    if (smallestDist > dist)
                    {
                        smallestDist = dist;
                        result = new Win32.ConsoleChar(' ', 0, c1);
                    }
                    if (dist <= float.Epsilon) return result;
                }

                for (byte c2 = (byte)(c1 + 1); c2 <= ByteColor.White; c2++)
                {
                    Color c2a = Color.From4bitIRGB(c2);
                    for (int i = 0; i < ShadeCharacters.Length; i++)
                    {
                        float shade = ShadeCharacters[i].Intensity;
                        Color shadedColor = (c1a * shade) + (c2a * (1f - shade));
                        float dist = Color.Distance(shadedColor, color);
                        if (smallestDist > dist)
                        {
                            smallestDist = dist;
                            result = new Win32.ConsoleChar(ShadeCharacters[i].Character, c1, c2);
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

        // const float Ratio1 = 128f / byte.MaxValue;
        // const float Ratio2 = 64f / byte.MaxValue;

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

        /// <summary>
        /// <para>
        /// Find the closest RGBx approximation of a 24-bit RGB color, for x = 0 or 1
        /// </para>
        /// <para>
        /// Source: <see href="https://stackoverflow.com/questions/41644778/convert-24-bit-color-to-4-bit-rgbi"/>
        /// </para>
        /// </summary>
        static (byte R, byte G, byte B) RgbxApprox(Color color, byte x)
        {
            float threshold = (x + 1f) / 3f;
            byte r = (byte)(color.R > threshold ? 1 : 0);
            byte g = (byte)(color.G > threshold ? 1 : 0);
            byte b = (byte)(color.B > threshold ? 1 : 0);
            return (r, g, b);
        }

        /// <summary>
        /// <para>
        /// Find the closest 4-bit RGBI approximation (by Euclidean distance) to a 24-bit RGB color
        /// </para>
        /// <para>
        /// Source: <see href="https://stackoverflow.com/questions/41644778/convert-24-bit-color-to-4-bit-rgbi"/>
        /// </para>
        /// </summary>
        static byte RgbiApprox(Color color)
        {
            // find best RGB0 and RGB1 approximations:
            (byte r0, byte g0, byte b0) = Color.RgbxApprox(color, 0);
            (byte r1, byte g1, byte b1) = Color.RgbxApprox(color, 1);

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
