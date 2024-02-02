using System.Runtime.Versioning;
using Win32;

namespace ConsoleGame
{
    public enum ImageRenderMode
    {
        Normal,
        Shaded,
        Subpixels,
    }

    public delegate void SimpleEventHandler();

    [SupportedOSPlatform("windows")]
    public class ConsoleRenderer : Win32.ConsoleRenderer
    {
        bool shouldResize;

        public bool IsBloomEnabled;

        public Buffer<float> DepthBuffer { get; }

        public ref ConsoleChar this[VectorInt screenPosition] => ref ConsoleBuffer[(screenPosition.Y * BufferWidth) + screenPosition.X];

        public event SimpleEventHandler? OnResized;

        public ConsoleRenderer(short width, short height) : base(width, height)
        {
            DepthBuffer = new Buffer<float>(this);
            shouldResize = true;
        }

        public override void Clear()
        {
            base.Clear();
            DepthBuffer.Clear();
        }

        public void DrawImage(TransparentImage image, RectInt rect, ImageRenderMode mode)
        {
            switch (mode)
            {
                case ImageRenderMode.Normal:
                    DrawImageNormal(image, rect);
                    break;
                case ImageRenderMode.Shaded:
                    DrawImageShaded(image, rect);
                    break;
                case ImageRenderMode.Subpixels:
                    DrawImageSubpixel(image, rect);
                    break;
                default:
                    return;
            }
        }

        public void DrawImage(Image image, RectInt rect, ImageRenderMode mode)
        {
            switch (mode)
            {
                case ImageRenderMode.Normal:
                    DrawImageNormal(image, rect);
                    break;
                case ImageRenderMode.Shaded:
                    DrawImageShaded(image, rect);
                    break;
                case ImageRenderMode.Subpixels:
                    DrawImageSubpixel(image, rect);
                    break;
                default:
                    return;
            }
        }

        public void DrawImage(Image? image, RectInt rect, ImageRenderMode mode)
        {
            if (!image.HasValue) return;
            DrawImage(image.Value, rect, mode);
        }

        public void DrawImage(TransparentImage? image, RectInt rect, ImageRenderMode mode)
        {
            if (!image.HasValue) return;
            DrawImage(image.Value, rect, mode);
        }


        void DrawImageSubpixel(Image image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height - 1);

            for (int y_ = 0; y_ < rect.Height * 2; y_++)
            {
                for (int x_ = 0; x_ < rect.Width * 2; x_++)
                {
                    VectorInt pointTL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    VectorInt pointTR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    VectorInt pointBL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);
                    VectorInt pointBR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);


                    if (!IsVisible(pointTL)) continue;

                    byte colorTL = (byte)image.GetPixelWithUV(rect.Size, pointTL);

                    byte fg = colorTL;
                    byte bg = colorTL;
                    char c = ' ';

                    if (IsVisible(pointBR))
                    {
                        byte colorTR = (byte)image.GetPixelWithUV(rect.Size, pointTR);
                        byte colorBL = (byte)image.GetPixelWithUV(rect.Size, pointBL);
                        byte colorBR = (byte)image.GetPixelWithUV(rect.Size, pointBR);

                        if (colorTL != colorBL || colorTL != colorBR || colorTR != colorBL || colorTR != colorBR)
                        {
                            fg = colorTL;
                            bg = colorBL;
                            c = Ascii.Blocks.Top;
                        }
                        else if (colorTL != colorTR || colorTL != colorBR || colorBL != colorBR || colorBL != colorTR)
                        {
                            fg = colorTL;
                            bg = colorTR;
                            c = Ascii.Blocks.Left;
                        }
                    }

                    this[pointTL] = new ConsoleChar(c, fg, bg);
                }
            }
        }
        void DrawImageShaded(Image image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    VectorInt point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector uv = (Vector)point / (Vector)rect.Size;
                    uv *= imageSize;
                    VectorInt imageCoord = Vector.Floor(uv);

                    Color pixel = image[imageCoord.X, imageCoord.Y];
                    this[point] = CharColor.ToCharacterColored(pixel);
                    // BloomBlur[point] = pixel;
                }
            }
        }
        void DrawImageNormal(Image image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    VectorInt point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector uv = (Vector)point / (Vector)rect.Size;
                    uv *= imageSize;
                    VectorInt imageCoord = Vector.Floor(uv);

                    Color pixel = image[imageCoord.X, imageCoord.Y];
                    byte convertedPixel = CharColor.From24bitColor(pixel);
                    this[point] = new ConsoleChar(' ', CharColor.Black, convertedPixel);
                    // BloomBlur[point] = pixel;
                }
            }
        }

        void DrawImageSubpixel(TransparentImage image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height - 1);

            for (int y_ = 0; y_ < rect.Height * 2; y_++)
            {
                for (int x_ = 0; x_ < rect.Width * 2; x_++)
                {
                    VectorInt pointTL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    VectorInt pointTR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    VectorInt pointBL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);
                    VectorInt pointBR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);

                    if (!IsVisible(pointTL)) continue;

                    byte colorTL = (byte)(Color)image.GetPixelWithUV(rect.Size, pointTL);

                    byte fg = colorTL;
                    byte bg = colorTL;
                    char c = ' ';

                    if (IsVisible(pointBR))
                    {
                        byte colorTR = (byte)(Color)image.GetPixelWithUV(rect.Size, pointTR);
                        byte colorBL = (byte)(Color)image.GetPixelWithUV(rect.Size, pointBL);
                        byte colorBR = (byte)(Color)image.GetPixelWithUV(rect.Size, pointBR);

                        if (colorTL != colorBL || colorTL != colorBR || colorTR != colorBL || colorTR != colorBR)
                        {
                            fg = colorTL;
                            bg = colorBL;
                            c = Ascii.Blocks.Top;
                        }
                        else if (colorTL != colorTR || colorTL != colorBR || colorBL != colorBR || colorBL != colorTR)
                        {
                            fg = colorTL;
                            bg = colorTR;
                            c = Ascii.Blocks.Left;
                        }
                    }

                    this[pointTL] = new ConsoleChar(c, fg, bg);
                }
            }
        }
        void DrawImageShaded(TransparentImage image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    VectorInt point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector uv = (Vector)point / (Vector)rect.Size;
                    uv *= imageSize;
                    VectorInt imageCoord = Vector.Floor(uv);

                    TransparentColor pixel = image[imageCoord.X, imageCoord.Y];
                    if (pixel.A <= float.Epsilon) continue;
                    Color alreadyThere = CharColor.FromCharacter(this[point]);
                    Color c = pixel.Blend(alreadyThere);
                    this[point] = CharColor.ToCharacterColored(c);
                    // BloomBlur[point] = c;
                }
            }
        }
        void DrawImageNormal(TransparentImage image, RectInt rect)
        {
            VectorInt imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    VectorInt point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector uv = (Vector)point / (Vector)rect.Size;
                    uv *= imageSize;
                    VectorInt imageCoord = Vector.Floor(uv);

                    TransparentColor pixel = image[imageCoord.X, imageCoord.Y];
                    if (pixel.A <= float.Epsilon) continue;
                    Color alreadyThere = CharColor.FromCharacter(this[point]);
                    Color c = pixel.Blend(alreadyThere);
                    byte convertedPixel = CharColor.From24bitColor(c);
                    this[point] = new ConsoleChar(' ', CharColor.Black, convertedPixel);
                    // BloomBlur[point] = c;
                }
            }
        }

        public bool IsVisible(VectorInt position) => IsVisible(position.X, position.Y);

        public void ShouldResize() => shouldResize = true;

        public bool Resize()
        {
            if (!shouldResize) return false;
            shouldResize = false;

            Console.Clear();

            base.RefreshBufferSize();

            DepthBuffer.Resize();

            OnResized?.Invoke();

            return true;
        }
    }
}
