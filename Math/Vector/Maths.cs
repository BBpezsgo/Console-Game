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

        public readonly VectorInt Round() => Vector.Round(this);
        public readonly VectorInt Floor() => Vector.Floor(this);
        public readonly VectorInt Ceil() => Vector.Ceil(this);

        public static VectorInt Round(Vector vector) => new((int)MathF.Round(vector.X), (int)MathF.Round(vector.Y));
        public static VectorInt Floor(Vector vector) => new((int)MathF.Floor(vector.X), (int)MathF.Floor(vector.Y));
        public static VectorInt Ceil(Vector vector) => new((int)MathF.Ceiling(vector.X), (int)MathF.Ceiling(vector.Y));

        public static VectorInt Round(float x, float y) => new((int)MathF.Round(x), (int)MathF.Round(y));
        public static VectorInt Floor(float x, float y) => new((int)MathF.Floor(x), (int)MathF.Floor(y));
        public static VectorInt Ceil(float x, float y) => new((int)MathF.Ceiling(x), (int)MathF.Ceiling(y));

        public static Vector RotateByDeg(Vector direction, float deg)
        {
            float v = Vector.ToDeg(direction);
            v += deg;
            return Vector.FromDeg(v);
        }
        public static void RotateByDeg(ref Vector direction, float deg)
        {
            float v = Vector.ToDeg(direction);
            v += deg;
            direction = Vector.FromDeg(v);
        }

        public static Vector RotateByRad(Vector direction, float rad)
        {
            float v = Vector.ToRad(direction);
            v += rad;
            return Vector.FromRad(v);
        }
        public static void RotateByRad(ref Vector direction, float rad)
        {
            float v = Vector.ToRad(direction);
            v += rad;
            direction = Vector.FromRad(v);
        }
    }
}
