using Microsoft.Win32.SafeHandles;
using Win32;

namespace ConsoleGame
{
    public partial class ConsoleRenderer
    {
        readonly SafeFileHandle Handle;

        public short Width => width;
        public short Height => height;

        public int Size => width * height;

        short width;
        short height;

        CharInfo[] ConsoleBuffer;
        SmallRect ConsoleRect;

        bool shouldResize = false;

        public ref CharInfo this[int i] => ref ConsoleBuffer[i];
        public ref CharInfo this[int x, int y] => ref ConsoleBuffer[(y * width) + x];

        public ref CharInfo this[float x, float y] => ref this[(int)MathF.Round(x), (int)MathF.Round(y)];
        public ref CharInfo this[Vector position] => ref this[position.X, position.Y];

        public ConsoleRenderer(SafeFileHandle handle, short width, short height)
        {
            Handle = handle;
            this.width = width;
            this.height = height;
            ConsoleBuffer = new CharInfo[this.width * this.height];
            for (int i = 0; i < ConsoleBuffer.Length; i++)
            {
                ConsoleBuffer[i].Char = ' ';
            }
            ConsoleRect = new SmallRect() { Left = 0, Top = 0, Right = this.width, Bottom = this.height };
        }

        public void Render()
        {
            if (Handle.IsInvalid)
            {
                System.Diagnostics.Debug.Fail("Console handle is invalid");
                return;
            }

            if (Handle.IsClosed)
            { return; }

            if (Kernel32.WriteConsoleOutputW(
                Handle,
                ConsoleBuffer,
                new Coord(width, height),
                new Coord(0, 0),
                ref ConsoleRect) == 0)
            { throw WindowsException.Get(); }
        }

        public void Clear() => Array.Clear(ConsoleBuffer);

        public void ShouldResize() => shouldResize = true;

        public void Resize()
        {
            if (!shouldResize) return;
            shouldResize = false;

            Console.Clear();

            width = (short)Console.WindowWidth;
            height = (short)Console.WindowHeight;
            
            ConsoleBuffer = new CharInfo[width * height];
            ConsoleRect = new SmallRect() { Left = 0, Top = 0, Right = width, Bottom = height };
        }
    }
}
