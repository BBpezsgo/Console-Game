using System.Diagnostics;
using System.Globalization;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Vector : IEquatable<Vector>, ISerializable<Vector>
    {
        public float X;
        public float Y;

        public readonly float Magnitude => MathF.Sqrt((X * X) + (Y * Y));
        public readonly float SqrMagnitude => (X * X) + (Y * Y);

        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vector(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }
        public Vector(ValueTuple<float, float> v)
        {
            X = v.Item1;
            Y = v.Item2;
        }

        public static bool operator ==(Vector left, Vector right) => left.Equals(right);
        public static bool operator !=(Vector left, Vector right) => !(left == right);

        public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(Vector a, Vector b) => new(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(Vector a, Vector b) => new(a.X / b.X, a.Y / b.Y);

        public static Vector operator *(Vector a, float b) => new(a.X * b, a.Y * b);
        public static Vector operator /(Vector a, float b) => new(a.X / b, a.Y / b);

        public static Vector operator *(float a, Vector b) => new(a * b.X, a * b.Y);

        public void Normalize()
        {
            float l = Magnitude;
            X /= l;
            Y /= l;
        }
        public readonly Vector Normalized
        {
            get
            {
                float l = Magnitude;
                return new Vector(X / l, Y / l);
            }
        }

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

        const float Deg2Byte = (float)byte.MaxValue / 360f;
        const float Byte2Deg = 360f / (float)byte.MaxValue;
        const float Rad2Deg = 180f / MathF.PI;
        const float Deg2Rad = MathF.PI / 180f;

        public static void ClampAngle(ref float deg)
        {
            while (deg < 0)
            { deg += 360f; }
            while (deg >= 360f)
            { deg -= 360f; }
        }

        public static float ClampAngle(float deg)
        {
            while (deg < 0)
            { deg += 360f; }
            while (deg >= 360f)
            { deg -= 360f; }
            return deg;
        }

        public static Vector FromDeg(float deg) => Vector.FromRad((float)(deg * Deg2Rad));
        public static Vector FromRad(float rad) => new(MathF.Cos(rad), MathF.Sin(rad));

        public static float ToDeg(Vector unitVector) => Vector.ToRad(unitVector) * Rad2Deg;
        public static float ToRad(Vector unitVector) => MathF.Atan2(unitVector.Y, unitVector.X);

        public static void SerializeAsDirection(Vector vec, Serializer serializer)
        {
            serializer.Serialize((byte)MathF.Round(ClampAngle(Vector.ToDeg(vec)) * Deg2Byte));
        }

        public static Vector DeserializeAsDirection(Deserializer deserializer)
        {
            return Vector.FromDeg((float)deserializer.DeserializeByte() * Byte2Deg);
        }

        internal static Vector MoveTowards(Vector a, Vector b, float maxDelta)
        {
            Vector diff = b - a;
            if (diff.SqrMagnitude <= float.Epsilon) return Vector.Zero;
            maxDelta = Math.Clamp(maxDelta, 0f, diff.Magnitude);
            diff.Normalize();
            return diff * maxDelta;
        }

        public static Vector Zero => new(0f, 0f);
        public static Vector One => new(1f, 1f);
    }
}
