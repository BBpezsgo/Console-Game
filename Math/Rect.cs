using System.Diagnostics;
using System.Globalization;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Rect : IEquatable<Rect>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float Top
        {
            readonly get => Y;
            set
            {
                float diff = Y - value;
                Y = value;
                Height += diff;
            }
        }
        public float Left
        {
            readonly get => X;
            set
            {
                float diff = X - value;
                X = value;
                Width += diff;
            }
        }
        public float Bottom
        {
            readonly get => Y + Height;
            set => Height = value - Y;
        }
        public float Right
        {
            readonly get => X + Width;
            set => Width = value - X;
        }

        public Vector Position
        {
            readonly get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector Size
        {
            readonly get => new(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect(Vector position, Vector size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public readonly bool Contains(Vector point) =>
            point.X >= X &&
            point.Y >= Y &&
            point.X <= Right &&
            point.Y <= Bottom;
        public readonly bool Contains(float x, float y) =>
            x >= X &&
            y >= Y &&
            x <= Right &&
            y <= Bottom;

        public override readonly bool Equals(object? obj) =>
            obj is Rect rect &&
            Equals(rect);
        public readonly bool Equals(Rect other) =>
            X == other.X &&
            Y == other.Y &&
            Width == other.Width &&
            Height == other.Height;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(Rect left, Rect right) => left.Equals(right);
        public static bool operator !=(Rect left, Rect right) => !(left == right);

        public override readonly string ToString() => $"({X.ToString("0.00", CultureInfo.InvariantCulture)}, {Y.ToString("0.00", CultureInfo.InvariantCulture)}, {Width.ToString("0.00", CultureInfo.InvariantCulture)}, {Height.ToString("0.00", CultureInfo.InvariantCulture)})";
        readonly string GetDebuggerDisplay() => ToString();

        public static Rect Intersection(Rect a, Rect b)
        {
            float left = Math.Max(a.Left, b.Left);
            float right = Math.Min(a.Right, b.Right);
            float top = Math.Max(a.Top, b.Top);
            float bottom = Math.Min(a.Bottom, b.Bottom);

            return new Rect(left, top, right - left, bottom - top);
        }
    }
}
