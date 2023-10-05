using System.Diagnostics;
using System.Globalization;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct VectorInt : IEquatable<VectorInt>, ISerializable<VectorInt>
    {
        public int X;
        public int Y;

        public VectorInt(int x, int y)
        {
            X = x;
            Y = y;
        }
        public VectorInt(ValueTuple<int, int> v)
        {
            X = v.Item1;
            Y = v.Item2;
        }

        public static bool operator ==(VectorInt left, VectorInt right) => left.Equals(right);
        public static bool operator !=(VectorInt left, VectorInt right) => !(left == right);

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

        public static implicit operator Vector(VectorInt v) => new(v.X, v.Y);

        public override readonly bool Equals(object? obj) =>
            obj is VectorInt vector &&
            Equals(vector);
        public readonly bool Equals(VectorInt other) =>
            X == other.X &&
            Y == other.Y;
        public override readonly int GetHashCode()
            => HashCode.Combine(X, Y);

        public override readonly string ToString()
            => $"({X.ToString("0.00", CultureInfo.InvariantCulture)}, {Y.ToString("0.00", CultureInfo.InvariantCulture)})";
        readonly string GetDebuggerDisplay() => ToString();

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(X);
            serializer.Serialize(Y);
        }

        public void Deserialize(Deserializer deserializer)
        {
            X = deserializer.DeserializeInt32();
            Y = deserializer.DeserializeInt32();
        }

        public static VectorInt Zero => new(0, 0);
        public static VectorInt One => new(1, 1);
    }
}
