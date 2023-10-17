using System.Runtime.ExceptionServices;
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

        public void DrawLines(VectorInt[] points, Color color, bool connectEnd = false)
            => DrawLines(points, (byte)color, ' ', connectEnd);
        public void DrawLines(VectorInt[] points, ushort attributes, char c, bool connectEnd = false)
        {
            for (int i = 1; i < points.Length; i++)
            { DrawLine(points[i - 1], points[i], attributes, c); }

            if (connectEnd && points.Length > 2)
            { DrawLine(points[0], points[^1], attributes, c); }
        }
        public void DrawLine(VectorInt a, VectorInt b, Color color)
            => DrawLine(a, b, (byte)color, ' ');
        public void DrawLine(VectorInt a, VectorInt b, ushort attributes, char c)
        {
            int dist = (int)MathF.Sqrt((a - b).SqrMagnitude);

            for (int i = 0; i < dist; i++)
            {
                float v = (float)i / (float)dist;
                Vector p = (a * v) + (b * (1f - v));
                VectorInt p2 = p.Round();
                if (!IsVisible(p2)) continue;
                this[p2].Attributes = attributes;
                this[p2].Char = c;
            }
            return;

            int minX = Math.Min(a.X, b.X);
            int minY = Math.Min(a.Y, b.Y);
            int maxX = Math.Max(a.X, b.X);
            int maxY = Math.Max(a.Y, b.Y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!IsVisible(x, y)) continue;
                    this[x, y].Attributes = attributes;
                    this[x, y].Char = c;
                }
            }
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = b;
            b = a;
            a = temp;
        }

        public void FillTriangle(VectorInt a, VectorInt b, VectorInt c, ushort attributes, char character)
            => FillTriangle(a.X, a.Y, b.X, b.Y, c.X, c.Y, new CharInfo(character, attributes));
        public void FillTriangle(VectorInt a, VectorInt b, VectorInt c, CharInfo c1)
            => FillTriangle(a.X, a.Y, b.X, b.Y, c.X, c.Y, c1);
        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, CharInfo c)
        {
            int width = this.Width;
            int height = this.Height;

            // sort the points vertically
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            if (y0 > y1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            double dx_far = Convert.ToDouble(x2 - x0) / (y2 - y0 + 1);
            double dx_upper = Convert.ToDouble(x1 - x0) / (y1 - y0 + 1);
            double dx_low = Convert.ToDouble(x2 - x1) / (y2 - y1 + 1);
            double xf = x0;
            double xt = x0 + dx_upper; // if y0 == y1, special case
            for (int y = y0; y <= (y2 > height - 1 ? height - 1 : y2); y++)
            {
                if (y >= 0)
                {
                    for (int x = (xf > 0 ? Convert.ToInt32(xf) : 0);
                         x <= (xt < width ? xt : width - 1); x++)
                    {
                        if (!this.IsVisible(x, y)) continue;
                        this[Convert.ToInt32(x + y * width)] = c;
                    }
                    for (int x = (xf < width ? Convert.ToInt32(xf) : width - 1);
                         x >= (xt > 0 ? xt : 0); x--)
                    {
                        if (!this.IsVisible(x, y)) continue;
                        this[Convert.ToInt32(x + y * width)] = c;
                    }
                }
                xf += dx_far;
                if (y < y1)
                { xt += dx_upper; }
                else
                { xt += dx_low; }
            }
        }

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
                    this[point] = Color.ToCharacterColored(pixel);
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
                    this[point] = Color.ToCharacterColored(pixel.Blend(alreadyThere));
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
