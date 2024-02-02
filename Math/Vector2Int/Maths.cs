namespace ConsoleGame
{
    public partial struct Vector2Int
    {
        public readonly int SqrMagnitude => (X * X) + (Y * Y);
    }
}
