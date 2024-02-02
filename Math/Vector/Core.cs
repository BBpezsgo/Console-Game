using System.Globalization;
using System.Numerics;

namespace ConsoleGame
{
    public static partial class Vector
    {
        public static Vector4 To4(this Vector2 v) => new(v.X, v.Y, 0f, 1f);
        public static Vector4 To4(this Vector3 v) => new(v.X, v.Y, v.Z, 1f);

        public static Vector3 To3(this Vector4 v) => new(v.X, v.Y, v.Z);
        public static Vector3 To3(this Vector2 v) => new(v.X, v.Y, 0f);

        public static Vector2 To2(this Vector4 v) => new(v.X, v.Y);
        public static Vector2 To2(this Vector3 v) => new(v.X, v.Y);

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

        public static bool TryParse(string text, out Vector2 vector2)
        {
            vector2 = default;
            text = text.Trim();
            string[] parts = text.Split(' ');

            if (parts.Length != 2)
            { return false; }

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector2.X))
            { return false; }
            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector2.Y))
            { return false; }

            return true;
        }
    }
}
