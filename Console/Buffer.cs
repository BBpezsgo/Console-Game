using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame;

public class Buffer<T>
{
    readonly IRenderer Renderer;

    public int Width => Renderer.Width;
    public int Height => Renderer.Height;

    public int Size => Renderer.Width * Renderer.Height;

    T[] buffer;

    public ref T this[int i] => ref buffer[i];
    public ref T this[int x, int y] => ref buffer[(y * Width) + x];

    public ref T this[float x, float y] => ref this[(int)MathF.Round(x), (int)MathF.Round(y)];
    public ref T this[Vector2 position] => ref this[position.X, position.Y];
    public ref T this[Vector2Int position] => ref this[position.X, position.Y];

    public Buffer(IRenderer renderer)
    {
        Renderer = renderer;
        buffer = new T[Renderer.Width * Renderer.Height];
    }

    public void Clear() => Array.Clear(buffer);

    public void Resize() => buffer = new T[Renderer.Width * Renderer.Height];

    public void SetRect(RectInt rect, T value)
    {
        int top = rect.Top;
        int left = rect.Left;
        int bottom = rect.Bottom;
        int right = rect.Right;
        for (int y = top; y < bottom; y++)
        {
            Array.Fill(buffer, value, (y * Width) + left, right - left);
        }
    }

    [return: NotNullIfNotNull(nameof(v))]
    public static explicit operator T[]?(Buffer<T>? v) => v?.buffer;

    public static explicit operator Span<T>(Buffer<T>? v) => v?.buffer ?? Span<T>.Empty;

    public void Copy(ConsoleRenderer destination, Func<T, ConsoleChar> converter)
    {
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                destination[x, y] = converter.Invoke(this[x, y]);
            }
        }
    }
}
