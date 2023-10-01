namespace ConsoleGame
{
    public readonly struct Random
    {
        static readonly System.Random random = new();

        public static int Integer() => random.Next();
        public static int Integer(int max) => random.Next(max);
        public static int Integer(int min, int max) => random.Next(min, max);

        public static float Float() => (float)random.NextDouble();

        public static Vector Direction()
        {
            float x = (Random.Float() - .5f) * 2f * MathF.PI * 2f;
            float y = (Random.Float() - .5f) * 2f * MathF.PI * 2f;

            x = MathF.Cos(x);
            y = MathF.Sin(y);

            return new Vector(x, y);
        }
    }
}
