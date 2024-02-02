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

        public static implicit operator Win32.Gdi32.GdiColor(Color24 color) => new(color.R, color.G, color.B);
        public static implicit operator Color24(Win32.Gdi32.GdiColor color) => new(color.R, color.G, color.B);

        #endregion
    }
}
