using System.Net.Http.Headers;

namespace ConsoleGame
{
    public class AnsiRenderer : IRenderer<Color>
    {
        const char ESC = '\x1B';
        const char CSI = '[';

        Color[] buffer;
        short width;
        short height;

        public ref Color this[int i] => ref buffer[i];

        public Buffer<float> DepthBuffer { get; }

        public short Width => width;

        public short Height => height;

        public VectorInt Rect => new(width, height);

        public AnsiRenderer()
        {
            width = (short)Console.WindowWidth;
            height = (short)Console.WindowHeight;
            buffer = new Color[width * height];
            DepthBuffer = new Buffer<float>(this);
        }

        public void SetImmediately(int x, int y, Color color)
        {
            TextWriter o = Console.Out;
            Console.SetCursorPosition(x, y);
            (byte r, byte g, byte b) = Color.To24bitRGB(color);
            o.Write($"{ESC}{CSI}48;2;{r};{g};{b}m ");
            Console.SetCursorPosition(0, 0);
        }

        public static void Render(Color[] buffer, int width, int height)
        {
            TextWriter o = Console.Out;
            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(0, y);
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    (byte r, byte g, byte b) = Color.To24bitRGB(buffer[i]);
                    o.Write($"{ESC}{CSI}48;2;{r};{g};{b}m ");
                }
            }
            Console.SetCursorPosition(0, 0);
        }

        public static void Render(Color[] buffer)
        {
            TextWriter o = Console.Out;
            for (int i = 0; i < buffer.Length; i++)
            { o.Write($"{ESC}{CSI}48;2;{buffer[i]};{buffer[i]};{buffer[i]}m "); }
            Console.SetCursorPosition(0, 0);
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer);
            DepthBuffer.Clear();
        }

        public bool IsVisible(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
        public bool IsVisible(VectorInt p) => IsVisible(p.X, p.Y);

        public void Render()
        {
            Render(buffer, width, height);
        }
    }
}
