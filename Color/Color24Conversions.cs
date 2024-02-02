namespace ConsoleGame
{
    public partial struct Color24
    {
        #region Ansi256

        /// <summary>
        /// Source: <see href=""="https://stackoverflow.com/a/27165165"/>
        /// </summary>
        public static Color24 FromAnsi256(int number)
        {
            int index_R = (number - 16) / 36;
            int r = (index_R > 0) ? (55 + (index_R * 40)) : 0;
            int index_G = (number - 16) % 36 / 6;
            int g = (index_G > 0) ? (55 + (index_G * 40)) : 0;
            int index_B = (number - 16) % 6;
            int b = (index_B > 0) ? (55 + (index_B * 40)) : 0;
            return new Color24(r, g, b);
        }

        /// <summary>
        /// Source: <see href=""="https://stackoverflow.com/a/27165165"/>
        /// </summary>
        public static Color24 FromAnsi256Grayscale(int number)
        {
            int v = ((number - 232) * 10) + 8;
            return new Color24(v, v, v);
        }

        /// <summary>
        /// Source: <see href=""="https://stackoverflow.com/a/26665998"/>
        /// </summary>
        public static byte ToAnsi256Grayscale(byte color)
        {
            if (color < 8)
            { return 16; }

            if (color > 248)
            { return 231; }

            return (byte)(MathF.Round(((float)color - 8f) / 247f * 24f) + 232f);
        }

        /// <summary>
        /// Source: <see href=""="https://stackoverflow.com/a/26665998"/>
        /// </summary>
        public static byte ToAnsi256(Color24 color)
        {
            if (color.R == color.G && color.G == color.B)
            { return ToAnsi256Grayscale(color.R); }

            return (byte)(16
                + (36 * MathF.Round(color.R / (float)255 * 5))
                + (6 * MathF.Round(color.G / (float)255 * 5))
                + MathF.Round(color.B / (float)255 * 5));
        }

        #endregion

        #region 8bit RGB

        /// <summary>
        /// Source: <see href="https://stackoverflow.com/questions/41420215/single-byte-to-rgb-and-rgb-to-single-byte"/>
        /// </summary>
        public static explicit operator byte(Color24 color)
            => (byte)(((color.R / 32) << 5) + ((color.G / 32) << 2) + (color.B / 64));

        /// <summary>
        /// Source: <see href="https://stackoverflow.com/questions/41420215/single-byte-to-rgb-and-rgb-to-single-byte"/>
        /// </summary>
        public static explicit operator Color24(byte color)
        {
            byte r = (byte)((color & 0b_111_000_00) >> 5);
            byte g = (byte)((color & 0b_000_111_00) >> 2);
            byte b = (byte)(color & 0b_000_000_11);
            return new Color24(r, g, b);
        }

        #endregion

        #region 24bit RGB & Floating Color

        public static implicit operator Color(Color24 v) => new(
            (float)v.R / (float)byte.MaxValue,
            (float)v.G / (float)byte.MaxValue,
            (float)v.B / (float)byte.MaxValue);
        public static explicit operator Color24(Color v) => new(
            (byte)MathF.Round(Math.Clamp(v.R, 0f, 1f) * byte.MaxValue),
            (byte)MathF.Round(Math.Clamp(v.G, 0f, 1f) * byte.MaxValue),
            (byte)MathF.Round(Math.Clamp(v.B, 0f, 1f) * byte.MaxValue));

        public static explicit operator int(Color24 color)
            => (color.R << 16) | (color.G << 8) | color.B;
        public static explicit operator Color24(int color)
        {
            int r = (color & 0xff0000) >> 16;
            int g = (color & 0x00ff00) >> 8;
            int b = color & 0x0000ff;
            return new Color24((byte)r, (byte)g, (byte)b);
        }

        public static implicit operator System.Drawing.Color(Color24 color)
            => System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        public static implicit operator Color24(System.Drawing.Color color)
            => new(color.R, color.G, color.B);

        #endregion

        #region 4bit IRGB

        public static readonly Color24[] Irgb4bitColors = new Color24[0b_1_0000]
        {
            new(0, 0, 0), // 0b_0000
            new(0, 0, 128), // 0b_0001
            new(0, 128, 0), // 0b_0010
            new(0, 128, 128), // 0b_0011
            new(128, 0, 0), // 0b_0100
            new(128, 0, 128), // 0b_0101
            new(128, 128, 0), // 0b_0110
            new(192, 192, 192), // 0b_0111
            new(128, 128, 128), // 0b_1000
            new(0, 0, 255), // 0b_1001
            new(0, 255, 0), // 0b_1010
            new(0, 255, 255), // 0b_1011
            new(255, 0, 0), // 0b_1100
            new(255, 0, 255), // 0b_1101
            new(255, 255, 0), // 0b_1110
            new(255, 255, 255), // 0b_1111
        };

        public static Color24 From4bitIRGB(byte irgb) => Irgb4bitColors[irgb];

        public static Color24 From4bitIRGB(byte r, byte g, byte b, byte i)
            => Color24.From4bitIRGB((byte)((i << 3) | (r << 2) | (g << 1) | (b)));

        /// <summary>
        /// <para>
        /// Find the closest 4-bit RGBI approximation (by Euclidean distance) to a 24-bit RGB color
        /// </para>
        /// <para>
        /// Source: <see href="https://stackoverflow.com/questions/41644778/convert-24-bit-color-to-4-bit-rgbi"/>
        /// </para>
        /// </summary>
        public static byte To4bitIRGB(Color24 color)
        {
            /// <summary>
            /// Find the closest RGBx approximation of a 24-bit RGB color, for x = 0 or 1
            /// </summary>
            static (byte R, byte G, byte B) RgbxApprox(Color24 color, byte x)
            {
                int threshold = ((x + 1) * byte.MaxValue) / 3;
                byte r = color.R > threshold ? (byte)1 : (byte)0;
                byte g = color.G > threshold ? (byte)1 : (byte)0;
                byte b = color.B > threshold ? (byte)1 : (byte)0;
                return (r, g, b);
            }

            // find best RGB0 and RGB1 approximations:
            (byte r0, byte g0, byte b0) = RgbxApprox(color, 0);
            (byte r1, byte g1, byte b1) = RgbxApprox(color, 1);

            // convert them back to 24-bit RGB:
            Color24 color1 = Color24.From4bitIRGB(r0, g0, b0, 0);
            Color24 color2 = Color24.From4bitIRGB(r1, g1, b1, 1);

            // return the color closer to the original:
            int d0 = Color24.Distance(color, color1);
            int d1 = Color24.Distance(color, color2);

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
    }
}
