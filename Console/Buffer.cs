using Win32;

namespace ConsoleGame
{
    public partial class Buffer<T>
    {
        readonly Renderer Renderer;

        public short Width => Renderer.Width;
        public short Height => Renderer.Height;

        public int Size => Renderer.Size.Width * Renderer.Size.Height;

        T[] buffer;

        public ref T this[int i] => ref buffer[i];
        public ref T this[int x, int y] => ref buffer[(y * Width) + x];

        public ref T this[float x, float y] => ref this[(int)MathF.Round(x), (int)MathF.Round(y)];
        public ref T this[Vector position] => ref this[position.X, position.Y];
        public ref T this[VectorInt position] => ref this[position.X, position.Y];

        public Buffer(Renderer renderer)
        {
            Renderer = renderer;
            buffer = new T[Renderer.Size.Width * Renderer.Size.Height];
        }

        public void Clear() => Array.Clear(buffer);

        public void Resize() => buffer = new T[Renderer.Size.Width * Renderer.Size.Height];

        public void SetRect(RectInt rect, T value)
        {
            int top = rect.Top;
            int left = rect.Left;
            int bottom = rect.Bottom;
            int right = rect.Right;
            for (int y = top; y < bottom; y++)
            {
                Array.Fill(buffer, value, y * Width + left, right - left);
            }
        }

        public static explicit operator T[](Buffer<T> v) => v.buffer;

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
}
