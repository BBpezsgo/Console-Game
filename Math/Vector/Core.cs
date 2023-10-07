using System.Diagnostics;
using System.Globalization;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Vector : IEquatable<Vector>, ISerializable<Vector>
    {
        public float X;
        public float Y;

        public Vector(float v)
        {
            X = v;
            Y = v;
        }

        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector left, Vector right) => left.Equals(right);
        public static bool operator !=(Vector left, Vector right) => !(left == right);

        public override readonly bool Equals(object? obj) =>
            obj is Vector vector &&
            Equals(vector);
        public readonly bool Equals(Vector other) =>
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
            X = deserializer.DeserializeFloat();
            Y = deserializer.DeserializeFloat();
        }

        public static void SerializeAsDirection(Serializer serializer, Vector vec)
        {
            serializer.Serialize((byte)MathF.Round(ClampAngle(Vector.ToDeg(vec)) * Deg2Byte));
        }

        public static Vector DeserializeAsDirection(Deserializer deserializer)
        {
            return Vector.FromDeg((float)deserializer.DeserializeByte() * Byte2Deg);
        }

        public static Vector Zero => new(0f);
        public static Vector One => new(1f);
    }
}
