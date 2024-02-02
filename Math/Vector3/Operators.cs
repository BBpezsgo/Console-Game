using System.Numerics;

namespace ConsoleGame
{
    public partial struct Vector3
    {
        public static Vector3 operator -(Vector3 a) => new(-a.X, -a.Y, -a.Z);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 a, Vector3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vector3 operator /(Vector3 a, Vector3 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public static Vector3 operator *(Vector3 a, float b) => new(a.X * b, a.Y * b, a.Z * b);
        public static Vector3 operator /(Vector3 a, float b) => new(a.X / b, a.Y / b, a.Z / b);

        public static Vector3 operator *(float a, Vector3 b) => new(a * b.X, a * b.Y, a * b.Z);

        public static implicit operator Vector3(ValueTuple<float, float, float> v) => new(v.Item1, v.Item2, v.Item3);

        public static explicit operator Vector2(Vector3 v) => new(v.X, v.Y);
        public static implicit operator Vector3(Vector2 v) => new(v.X, v.Y, 0f);

        public static implicit operator Vector4(Vector3 v) => new(v.X, v.Y, v.Z, v.W);
        public static implicit operator Vector3(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
    }
}
