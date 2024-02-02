using System.Globalization;
using System.Numerics;

namespace ConsoleGame
{
    public static partial class Vector
    {
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
