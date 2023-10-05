namespace ConsoleGame
{
    public partial struct Vector
    {
        public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(Vector a, Vector b) => new(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(Vector a, Vector b) => new(a.X / b.X, a.Y / b.Y);

        public static Vector operator +(Vector a, VectorInt b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, VectorInt b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(Vector a, VectorInt b) => new(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(Vector a, VectorInt b) => new(a.X / b.X, a.Y / b.Y);

        public static Vector operator +(VectorInt a, Vector b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(VectorInt a, Vector b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(VectorInt a, Vector b) => new(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(VectorInt a, Vector b) => new(a.X / b.X, a.Y / b.Y);

        public static Vector operator *(Vector a, float b) => new(a.X * b, a.Y * b);
        public static Vector operator /(Vector a, float b) => new(a.X / b, a.Y / b);

        public static Vector operator *(float a, Vector b) => new(a * b.X, a * b.Y);
    }
}
