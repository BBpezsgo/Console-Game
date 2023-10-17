using System.Diagnostics;
using System.Globalization;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Vector3 : IEquatable<Vector3>, ISerializable<Vector3>
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float v)
        {
            X = v;
            Y = v;
            Z = v;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
        public static bool operator !=(Vector3 left, Vector3 right) => !(left == right);

        public override readonly bool Equals(object? obj) =>
            obj is Vector3 vector &&
            Equals(vector);
        public readonly bool Equals(Vector3 other) =>
            X == other.X &&
            Y == other.Y &&
            Y == other.Z;
        public override readonly int GetHashCode()
            => HashCode.Combine(X, Y, Z);

        public override readonly string ToString()
            => $"({X.ToString("0.00", CultureInfo.InvariantCulture)}, {Y.ToString("0.00", CultureInfo.InvariantCulture)}, {Z.ToString("0.00", CultureInfo.InvariantCulture)})";
        readonly string GetDebuggerDisplay() => ToString();

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(X);
            serializer.Serialize(Y);
            serializer.Serialize(Z);
        }

        public void Deserialize(Deserializer deserializer)
        {
            X = deserializer.DeserializeFloat();
            Y = deserializer.DeserializeFloat();
            Z = deserializer.DeserializeFloat();
        }

        public static Vector3 Zero => new(0f);
        public static Vector3 One => new(1f);
    }
}
