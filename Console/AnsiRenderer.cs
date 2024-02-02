using System.Text;
using Win32;

namespace ConsoleGame
{
    [Flags]
    public enum AnsiGraphicsModes : uint
    {
        #region Foreground Colors
        ForegroundRed = 30 + 0b_0001,
        ForegroundGreen = 30 + 0b_0010,
        ForegroundBlue = 30 + 0b_0100,

        ForegroundYellow = 30 + 0b_0011,
        ForegroundMagenta = 30 + 0b_0101,
        ForegroundCyan = 30 + 0b_0110,

        ForegroundBlack = 30 + 0b_0000,
        ForegroundWhite = 30 + 0b_0111,
        ForegroundDefault = 39,
        #endregion

        #region Background Colors
        BackgroundRed = 40 + 0b_0001,
        BackgroundGreen = 40 + 0b_0010,
        BackgroundBlue = 40 + 0b_0100,

        BackgroundYellow = 40 + 0b_0011,
        BackgroundMagenta = 40 + 0b_0101,
        BackgroundCyan = 40 + 0b_0110,

        BackgroundBlack = 40 + 0b_0000,
        BackgroundWhite = 40 + 0b_0111,
        BackgroundDefault = 49,
        #endregion

        #region Foreground Bright Colors
        ForegroundBrightRed = 90 + 0b_0001,
        ForegroundBrightGreen = 90 + 0b_0010,
        ForegroundBrightBlue = 90 + 0b_0100,

        ForegroundBrightYellow = 90 + 0b_0011,
        ForegroundBrightMagenta = 90 + 0b_0101,
        ForegroundBrightCyan = 90 + 0b_0110,

        ForegroundBrightBlack = 90 + 0b_0000,
        ForegroundBrightWhite = 90 + 0b_0111,
        #endregion

        #region Background Bright Colors
        BackgroundBrightRed = 100 + 0b_0001,
        BackgroundBrightGreen = 100 + 0b_0010,
        BackgroundBrightBlue = 100 + 0b_0100,

        BackgroundBrightYellow = 100 + 0b_0011,
        BackgroundBrightMagenta = 100 + 0b_0101,
        BackgroundBrightCyan = 100 + 0b_0110,

        BackgroundBrightBlack = 100 + 0b_0000,
        BackgroundBrightWhite = 100 + 0b_0111,
        #endregion
    }

    public enum AnsiColorType
    {
        Extended,
        TrueColor,
    }

    public class AnsiRenderer : IRenderer<Color>
    {
        Color[] buffer;
        int width;
        int height;
        bool shouldResize;

        public AnsiColorType ColorType;
        public bool IsBloomEnabled;

        public Buffer<float> DepthBuffer { get; }
        public Span<Color> Buffer => buffer;

        public short Width => (short)width;
        public short Height => (short)height;

        public ref Color this[int i] => ref buffer[i];

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
                    byte bruh = Color24.ToAnsi256(color);

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
                        Ansi.SetBackgroundColor(builder, (System.Drawing.Color)color);
                        prevColor = color;
                    }

                    builder.Append(' ');
                }
            }
            Console.Out.Write(builder);
            Console.SetCursorPosition(0, 0);
        }

        public void Clear()
        {
            Array.Clear(buffer);
            DepthBuffer.Clear();
        }

        public bool IsVisible(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        public void Render()
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

        public bool Resize()
        {
            if (!shouldResize) return false;
            shouldResize = false;

            Console.Clear();

            width = Console.WindowWidth;
            height = Console.WindowHeight;

            buffer = new Color[width * height];

            DepthBuffer.Resize();

            OnResized?.Invoke();

            return true;
        }
    }
}
