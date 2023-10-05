namespace ConsoleGame
{
    public partial struct Vector
    {
        public readonly float Magnitude => MathF.Sqrt((X * X) + (Y * Y));
        public readonly float SqrMagnitude => (X * X) + (Y * Y);

        internal static Vector MoveTowards(Vector a, Vector b, float maxDelta)
        {
            Vector diff = b - a;
            if (diff.SqrMagnitude <= float.Epsilon) return Vector.Zero;
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
            }
            else
            {
                X /= l;
                Y /= l;
            }
        }
        public readonly Vector Normalized
        {
            get
            {
                float l = Magnitude;
                return l == 0f ? Vector.Zero : new Vector(X / l, Y / l);
            }
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

        public static Vector LinearLerp(Vector a, Vector b, float t) => (a * (1f - t)) + (b * t);

        public static float Distance(Vector a, Vector b) => (b - a).Magnitude;
    }
}
