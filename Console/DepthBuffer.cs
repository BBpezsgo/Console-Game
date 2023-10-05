namespace ConsoleGame
{
    public partial class DepthBuffer
    {
        readonly ConsoleRenderer Renderer;

        public short Width => Renderer.Width;
        public short Height => Renderer.Height;

        public int Size => Renderer.Size;

        byte[] Buffer;

        public ref byte this[int i] => ref Buffer[i];
        public ref byte this[int x, int y] => ref Buffer[(y * Width) + x];

        public ref byte this[float x, float y] => ref this[(int)MathF.Round(x), (int)MathF.Round(y)];
        public ref byte this[Vector position] => ref this[position.X, position.Y];
        public ref byte this[VectorInt position] => ref this[position.X, position.Y];

        public DepthBuffer(ConsoleRenderer renderer)
        {
            Renderer = renderer;
            Buffer = new byte[renderer.Size];
        }

        public void Clear() => Array.Clear(Buffer);
        public void Resize() => Buffer = new byte[Renderer.Size];
    }
}
