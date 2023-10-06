namespace ConsoleGame
{
    public partial class ConsoleRenderer
    {
        public static ushort MakeAttributes(int background, int foreground)
        {
            background &= 0b_1111;
            foreground &= 0b_1111;
            return unchecked((ushort)((background << 4) | foreground));
        }
    }
}
