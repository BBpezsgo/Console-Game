namespace ConsoleGame
{
    public partial struct Color
    {
        public static Color Red => new(1f, 0f, 0f);
        public static Color Green => new(0f, 1f, 0f);
        public static Color Blue => new(0f, 0f, 1f);
        public static Color Yellow => new(1f, 1f, 0f);
        public static Color Cyan => new(0f, 1f, 1f);
        public static Color Magenta => new(1f, 0f, 1f);
        public static Color Black => new(0f, 0f, 0f);
        public static Color White => new(1f, 1f, 1f);
        public static Color Gray => new(.5f, .5f, .5f);
    }
}
