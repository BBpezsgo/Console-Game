namespace ConsoleGame
{
    public readonly struct Tags
    {
        public static readonly int Player      = 0b_0000_0000_0000_0001;
        public static readonly int Enemy       = 0b_0000_0000_0000_0010;
        public static readonly int Projectile  = 0b_0000_0000_0000_0100;
        public static readonly int Effect      = 0b_0000_0000_0000_1000;
    }
}
