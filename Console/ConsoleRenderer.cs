using Microsoft.Win32.SafeHandles;
using Win32;

namespace ConsoleGame
{
    public enum ImageRenderMode
    {
        Normal,
        Shaded,
        Subpixels,
    }

    public class ConsoleRenderer : Win32.Utilities.ConsoleRenderer
    {
        bool shouldResize;

        public ref CharInfo this[VectorInt screenPosition] => ref ConsoleBuffer[(screenPosition.Y * bufferWidth) + screenPosition.X];

        public ConsoleRenderer(SafeFileHandle handle, short width, short height) : base(handle, width, height)
        { }

        public void DrawImage(Image? image, VectorInt position, bool fixWidth)
        {
            if (!image.HasValue) return;
            DrawImage(image.Value, position, fixWidth);
        }
        public void DrawImage(TransparentImage? image, VectorInt position, bool fixWidth)
        {
            if (!image.HasValue) return;
            DrawImage(image.Value, position, fixWidth);
        }

        public void DrawImage(Image image, VectorInt position, bool fixWidth)
        {
            int w = image.Width;
            int h = image.Height;

            if (fixWidth) w *= 2;

            for (int y_ = 0; y_ < h; y_++)
            {
                for (int x_ = 0; x_ < w; x_++)
                {
                    VectorInt point = new(x_ + position.X, y_ + position.Y);
                    if (!IsVisible(point)) continue;
                    this[point] = new CharInfo(' ', ByteColor.Black, Color.To4bitIRGB(image[fixWidth ? x_ / 2 : x_, y_]));
                }
            }
        }
        public void DrawImage(TransparentImage image, VectorInt position, bool fixWidth)
        {
            int w = image.Width;
            int h = image.Height;

            if (fixWidth) w *= 2;

            for (int y_ = 0; y_ < h; y_++)
            {
                for (int x_ = 0; x_ < w; x_++)
                {
                    VectorInt point = new(x_ + position.X, y_ + position.Y);
                    if (!IsVisible(point)) continue;
                    TransparentColor color = image[fixWidth ? x_ / 2 : x_, y_];
                    Color alreadyThere = Color.FromCharacter(this[point]);
                    Color newColor = color.Blend(alreadyThere);
                    this[point] = new CharInfo(' ', ByteColor.Black, Color.To4bitIRGB(newColor));
                }
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

                    this[pointTL] = new CharInfo(c, fg, bg);
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
                    this[point] = Color.ToCharacter(pixel);
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
                    byte convertedPixel = Color.To4bitIRGB(pixel);
                    this[point] = new CharInfo(' ', ByteColor.Black, convertedPixel);
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

                    this[pointTL] = new CharInfo(c, fg, bg);
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
                    Color alreadyThere = Color.FromCharacter(this[point]);
                    this[point] = Color.ToCharacter(pixel.Blend(alreadyThere));
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
                    Color alreadyThere = Color.FromCharacter(this[point]);
                    byte convertedPixel = Color.To4bitIRGB(pixel.Blend(alreadyThere));
                    this[point] = new CharInfo(' ', ByteColor.Black, convertedPixel);
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

            bufferWidth = (short)Console.WindowWidth;
            bufferHeight = (short)Console.WindowHeight;

            ConsoleBuffer = new CharInfo[bufferWidth * bufferHeight];
            ConsoleRect = new SmallRect() { Left = 0, Top = 0, Right = bufferWidth, Bottom = bufferHeight };
            return true;
        }
    }
}
