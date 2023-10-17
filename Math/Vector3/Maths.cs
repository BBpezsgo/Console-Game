namespace ConsoleGame
{
    public partial struct Vector3
    {
        public readonly float Magnitude => MathF.Sqrt((X * X) + (Y * Y) + (Z * Z));
        public readonly float SqrMagnitude => (X * X) + (Y * Y) + (Z * Z);

        internal static Vector3 MoveTowards(Vector3 a, Vector3 b, float maxDelta)
        {
            Vector3 diff = b - a;
            if (diff.SqrMagnitude <= float.Epsilon) return Vector3.Zero;
            maxDelta = Math.Clamp(maxDelta, 0f, diff.Magnitude);
            diff.Normalize();
            return diff * maxDelta;
        }

        public void Normalize()
        {
            float l = Magnitude;
            if (l == 0f)
            {
                X = 0f;
                Y = 0f;
                Z = 0f;
            }
            else
            {
                X /= l;
                Y /= l;
                Z /= l;
            }
        }
        public readonly Vector3 Normalized
        {
            get
            {
                float l = Magnitude;
                return l == 0f ? Vector3.Zero : new Vector3(X / l, Y / l, Z / l);
            }
        }

        public static Vector3 LinearLerp(Vector3 a, Vector3 b, float t) => (a * (1f - t)) + (b * t);

        public static float Distance(Vector3 a, Vector3 b) => (b - a).Magnitude;

        public static float Dot(Vector3 a, Vector3 b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            Vector3 v;
            v.X = a.Y * b.Z - a.Z * b.Y;
            v.Y = a.Z * b.X - a.X * b.Z;
            v.Z = a.X * b.Y - a.Y * b.X;
            return v;
        }
    }
}
