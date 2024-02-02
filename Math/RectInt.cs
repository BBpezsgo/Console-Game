using System.Diagnostics;
using System.Numerics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct RectInt : IEquatable<RectInt>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int Top
        {
            readonly get => Y;
            set
            {
                int diff = Y - value;
                Y = value;
                Height += diff;
            }
        }
        public int Left
        {
            readonly get => X;
            set
            {
                int diff = X - value;
                X = value;
                Width += diff;
            }
        }
        public int Bottom
        {
            readonly get => Y + Height;
            set => Height = value - Y;
        }
        public int Right
        {
            readonly get => X + Width;
            set => Width = value - X;
        }

        public Vector2Int Position
        {
            readonly get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2Int Size
        {
            readonly get => new(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public RectInt(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectInt(Vector2Int position, Vector2Int size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public readonly bool Contains(Vector2 point) =>
            point.X >= X &&
            point.Y >= Y &&
            point.X <= Right &&
            point.Y <= Bottom;
        public readonly bool Contains(float x, float y) =>
            x >= X &&
            y >= Y &&
            x <= Right &&
            y <= Bottom;

        public readonly bool Contains(Vector2Int point) =>
            point.X >= X &&
            point.Y >= Y &&
            point.X <= Right &&
            point.Y <= Bottom;
        public readonly bool Contains(int x, int y) =>
            x >= X &&
            y >= Y &&
            x <= Right &&
            y <= Bottom;

        public override readonly bool Equals(object? obj) =>
            obj is RectInt rect &&
            Equals(rect);
        public readonly bool Equals(RectInt other) =>
            X == other.X &&
            Y == other.Y &&
            Width == other.Width &&
            Height == other.Height;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(RectInt left, RectInt right) => left.Equals(right);
        public static bool operator !=(RectInt left, RectInt right) => !(left == right);

        public static implicit operator Rect(RectInt v) => new(v.X, v.Y, v.Width, v.Height);

        public override readonly string ToString() => $"({X} {Y} {Width} {Height})";
        readonly string GetDebuggerDisplay() => ToString();

        public RectInt Expand(int v)
        {
            X -= v;
            Y -= v;
            Width += v * 2;
            Height += v * 2;

            return this;
        }

        public RectInt Expand(Vector2Int v)
        {
            X -= v.X;
            Y -= v.Y;
            Width += v.X * 2;
            Height += v.Y * 2;

            return this;
        }

        public RectInt Expand(int top, int right, int bottom, int left)
        {
            X -= left;
            Y -= top;
            Width += left + right;
            Height += top + bottom;

            return this;
        }

        public static RectInt Intersection(Rect a, RectInt b)
            => RectInt.Intersection(b, a);
        public static RectInt Intersection(RectInt a, Rect b)
        {
            int left = Math.Max(a.Left, (int)MathF.Floor(b.Left));
            int right = Math.Min(a.Right, (int)MathF.Ceiling(b.Right));
            int top = Math.Max(a.Top, (int)MathF.Floor(b.Top));
            int bottom = Math.Min(a.Bottom, (int)MathF.Ceiling(b.Bottom));

            return new RectInt(left, top, right - left, bottom - top);
        }
        public static RectInt Intersection(RectInt a, RectInt b)
        {
            int left = Math.Max(a.Left, b.Left);
            int right = Math.Min(a.Right, b.Right);
            int top = Math.Max(a.Top, b.Top);
            int bottom = Math.Min(a.Bottom, b.Bottom);

            return new RectInt(left, top, right - left, bottom - top);
        }
    }
}
