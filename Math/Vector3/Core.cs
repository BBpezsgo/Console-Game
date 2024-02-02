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
        public float W;

        public Vector3(float v)
        {
            X = v;
            Y = v;
            Z = v;
            W = 1f;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 1f;
        }

        Vector3(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
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

        public static bool TryParse(string text, out Vector3 vector3)
        {
            vector3 = default;
            text = text.Trim();
            string[] parts = text.Split(' ');

            if (parts.Length != 3)
            { return false; }

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.X))
            { return false; }
            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.Y))
            { return false; }
            if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector3.Z))
            { return false; }

            return true;
        }
    }
}
