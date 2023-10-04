namespace ConsoleGame
{
    public static class Extensions
    {
        public static void Set<T>(this ref T self, T value) where T : struct
        {
            self = value;
        }
    }
}
