namespace ConsoleGame
{
    public partial struct ColorF
    {
        public static ColorF Red => new(1f, 0f, 0f);
        public static ColorF Green => new(0f, 1f, 0f);
        public static ColorF Blue => new(0f, 0f, 1f);
        public static ColorF Yellow => new(1f, 1f, 0f);
        public static ColorF Cyan => new(0f, 1f, 1f);
        public static ColorF Magenta => new(1f, 0f, 1f);
        public static ColorF Black => new(0f, 0f, 0f);
        public static ColorF White => new(1f, 1f, 1f);
        public static ColorF Gray => new(.5f, .5f, .5f);
    }
}
