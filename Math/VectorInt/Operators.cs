﻿namespace ConsoleGame
{
    public partial struct VectorInt
    {
        public static VectorInt operator +(VectorInt a, VectorInt b) => new(a.X + b.X, a.Y + b.Y);
        public static VectorInt operator -(VectorInt a, VectorInt b) => new(a.X - b.X, a.Y - b.Y);
        public static VectorInt operator *(VectorInt a, VectorInt b) => new(a.X * b.X, a.Y * b.Y);
        public static VectorInt operator /(VectorInt a, VectorInt b) => new(a.X / b.X, a.Y / b.Y);

        public static VectorInt operator *(VectorInt a, int b) => new(a.X * b, a.Y * b);
        public static VectorInt operator /(VectorInt a, int b) => new(a.X / b, a.Y / b);

        public static Vector operator *(VectorInt a, float b) => new(a.X * b, a.Y * b);
        public static Vector operator /(VectorInt a, float b) => new(a.X / b, a.Y / b);

        public static VectorInt operator *(int a, VectorInt b) => new(a * b.X, a * b.Y);
        public static Vector operator *(float a, VectorInt b) => new(a * b.X, a * b.Y);

        public static implicit operator VectorInt(Win32.Common.Point v) => new(v.X, v.Y);
        public static implicit operator VectorInt(Win32.Coord v) => new(v.X, v.Y);
    }
}
