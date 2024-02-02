using System.Text;
using Win32;
using Win32.Gdi32;

namespace ConsoleGame
{
    public enum AnsiColorType
    {
        Extended,
        TrueColor,
    }

    public class AnsiRenderer : BufferedRenderer<Color>
    {
        Color[] buffer;
        int width;
        int height;
        bool shouldResize;

        public AnsiColorType ColorType;
        public bool IsBloomEnabled;

        public Buffer<float> DepthBuffer { get; }
        public override Span<Color> Buffer => buffer;

        public override short Width => (short)width;
        public override short Height => (short)height;

        public override ref Color this[int i] => ref buffer[i];

        public event SimpleEventHandler? OnResized;

        public AnsiRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.buffer = new Color[width * height];
            this.DepthBuffer = new Buffer<float>(this);
            this.shouldResize = true;
            this.ColorType = AnsiColorType.TrueColor;
            this.IsBloomEnabled = true;
        }

        public static void RenderExtended(Color[] buffer, int width, int height)
        {
            StringBuilder builder = new(width * height);
            byte prevColor = default;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Color24 color = (Color24)buffer[i];
                    byte bruh = Ansi.ToAnsi256(color.R, color.G, color.B);

                    if ((x == 0 && y == 0) || prevColor != bruh)
                    {
                        Ansi.SetBackgroundColor(builder, bruh);
                        prevColor = bruh;
                    }

                    builder.Append(' ');
                }
            }
            Console.Out.Write(builder);
            Console.SetCursorPosition(0, 0);
        }

        public static void RenderTrueColor(Color[] buffer, int width, int height)
        {
            StringBuilder builder = new(width * height);
            Color24 prevColor = default;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Color24 color = (Color24)buffer[i];

                    if ((x == 0 && y == 0) || prevColor != color)
                    {
                        Ansi.SetBackgroundColor(builder, color.R, color.G, color.B);
                        prevColor = color;
                    }

                    builder.Append(' ');
                }
            }
            Console.Out.Write(builder);
            Console.SetCursorPosition(0, 0);
        }

        public override void Clear()
        {
            Array.Clear(buffer);
            DepthBuffer.Clear();
        }

        public override void Render()
        {
            if (IsBloomEnabled)
            { ColorUtils.Bloom(buffer, width, height, 5); }

            switch (ColorType)
            {
                case AnsiColorType.Extended:
                    RenderExtended(buffer, width, height);
                    break;
                case AnsiColorType.TrueColor:
                    RenderTrueColor(buffer, width, height);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void ShouldResize() => shouldResize = true;

        public override void RefreshBufferSize()
        {
            if (!shouldResize) return;
            shouldResize = false;

            Console.Clear();

            width = Console.WindowWidth;
            height = Console.WindowHeight;

            buffer = new Color[width * height];

            DepthBuffer.Resize();

            OnResized?.Invoke();
        }
    }
}
