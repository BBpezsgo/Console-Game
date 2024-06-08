using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame;

public class Buffer<T> : IRenderer<T>
{
    readonly IRenderer _renderer;
    T[] _buffer;

    public int Width => _renderer.Width;
    public int Height => _renderer.Height;

    public ref T this[int i] => ref _buffer[i];
    public ref T this[int x, int y] => ref _buffer[(y * Width) + x];

    public ref T this[float x, float y] => ref this[(int)MathF.Round(x), (int)MathF.Round(y)];
    public ref T this[Vector2 position] => ref this[position.X, position.Y];
    public ref T this[Vector2Int position] => ref this[position.X, position.Y];

    public Buffer(IRenderer renderer)
    {
        this._renderer = renderer;
        _buffer = new T[this._renderer.Width * this._renderer.Height];
    }

    public void RefreshBufferSize() => _buffer = new T[_renderer.Width * _renderer.Height];

    [return: NotNullIfNotNull(nameof(v))]
    public static explicit operator T[]?(Buffer<T>? v) => v?._buffer;

    public static explicit operator Span<T>(Buffer<T>? v) => v?._buffer ?? Span<T>.Empty;

    public void Set(int i, T pixel) => _buffer[i] = pixel;
    public void Render() => _renderer.Render();

    public void Clear() => Array.Clear(_buffer);
}
