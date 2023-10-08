using Microsoft.Win32.SafeHandles;
using Win32;

namespace ConsoleGame
{
    public class ConsoleRenderer : Win32.Utilities.ConsoleRenderer
    {
        bool shouldResize;

        public ref CharInfo this[VectorInt screenPosition] => ref ConsoleBuffer[(screenPosition.Y * bufferWidth) + screenPosition.X];

        public ConsoleRenderer(SafeFileHandle handle, short width, short height) : base(handle, width, height)
        { }

        public bool IsVisible(VectorInt position) => IsVisible(position.X, position.Y);

        public void ShouldResize() => shouldResize = true;

        public bool Resize()
        {
            if (!shouldResize) return false;
            shouldResize = false;

            Console.Clear();

            bufferWidth = (short)Console.WindowWidth;
            bufferHeight = (short)Console.WindowHeight;

            ConsoleBuffer = new CharInfo[bufferWidth * bufferHeight];
            ConsoleRect = new SmallRect() { Left = 0, Top = 0, Right = bufferWidth, Bottom = bufferHeight };
            return true;
        }
    }
}
