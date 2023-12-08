namespace ConsoleGame
{
    public partial struct Color24
    {
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
            byte R = (byte)((color & 0b_111_000_00) >> 5);
            byte G = (byte)((color & 0b_000_111_00) >> 2);
            byte B = (byte)(color & 0b_000_000_11);
            return new Color24(R, G, B);
        }

        #endregion

        #region 24bit RGB

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
            return new Color24(checked((byte)r), checked((byte)g), checked((byte)b));
        }

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

        #endregion
    }
}
