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

        readonly Buffer<float> DepthBuffer;
        public ref CharInfo this[VectorInt screenPosition] => ref ConsoleBuffer[(screenPosition.Y * bufferWidth) + screenPosition.X];

        public ConsoleRenderer(SafeFileHandle handle, short width, short height) : base(handle, width, height)
        {
            DepthBuffer = new Buffer<float>(this);
            shouldResize = true;
        }

        public override void ClearBuffer()
        {
            base.ClearBuffer();
            DepthBuffer.Clear();
        }

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

        public void FillTriangle(
            VectorInt a,
            Vector3 texA,
            VectorInt b,
            Vector3 texB,
            VectorInt c,
            Vector3 texC,
            CharInfo @char
            )
            => this.FillTriangle(
                a.X, a.Y,
                texA.X, texA.Y, texA.Z,
                b.X, b.Y,
                texB.X, texB.Y, texB.Z,
                c.X, c.Y,
                texC.X, texC.Y, texC.Z,
                @char
                );
        public void FillTriangle(
            int x1, int y1,
            float u1, float v1, float w1,
            int x2, int y2,
            float u2, float v2, float w2,
            int x3, int y3,
            float u3, float v3, float w3,
            CharInfo @char
            )
        {
            // sort the points vertically
            if (y2 < y1)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
                Swap(ref u1, ref u2);
                Swap(ref v1, ref v2);
                Swap(ref w1, ref w2);
            }

            if (y3 < y1)
            {
                Swap(ref x1, ref x3);
                Swap(ref y1, ref y3);
                Swap(ref u1, ref u3);
                Swap(ref v1, ref v3);
                Swap(ref w1, ref w3);
            }

            if (y2 > y3)
            {
                Swap(ref x2, ref x3);
                Swap(ref y2, ref y3);
                Swap(ref u2, ref u3);
                Swap(ref v2, ref v3);
                Swap(ref w2, ref w3);
            }

            int dy1 = y2 - y1;
            int dx1 = x2 - x1;
            float dv1 = v2 - v1;
            float du1 = u2 - u1;
            float dw1 = w2 - w1;

            int dy2 = y3 - y1;
            int dx2 = x3 - x1;
            float dv2 = v3 - v1;
            float du2 = u3 - u1;
            float dw2 = w3 - w1;

            float texW;

            float daxStep = 0f;
            float dbxStep = 0f;
            float du1Step = 0f;
            float dv1Step = 0f;
            float du2Step = 0f;
            float dv2Step = 0f;
            float dw1Step = 0f;
            float dw2Step = 0f;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
            if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy2 != 0) du2Step = du2 / MathF.Abs(dy2);
            if (dy2 != 0) dv2Step = dv2 / MathF.Abs(dy2);
            if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

            if (dy1 != 0)
            {
                for (int i = y1; i <= y2; i++)
                {
                    int ax = (int)(x1 + ((i - y1) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float texSu = u1 + (i - y1) * du1Step;
                    float texSv = v1 + (i - y1) * dv1Step;
                    float texSw = w1 + (i - y1) * dw1Step;

                    float texEu = u1 + (i - y1) * du2Step;
                    float texEv = v1 + (i - y1) * dv2Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSu, ref texEu);
                        Swap(ref texSv, ref texEv);
                        Swap(ref texSw, ref texEw);
                    }

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texW = (1f - t) * texSw + t * texEw;

                        if (this.IsVisible(j, i) && (DepthBuffer == null || texW > DepthBuffer[j, i]))
                        {
                            this[j, i] = @char;
                            if (DepthBuffer != null) DepthBuffer[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
            }

            dy1 = y3 - y2;
            dx1 = x3 - x2;
            dv1 = v3 - v2;
            du1 = u3 - u2;
            dw1 = w3 - w2;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            du1Step = 0f;
            dv1Step = 0f;
            if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
            if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy1 != 0)
            {
                for (int i = y2; i <= y3; i++)
                {
                    int ax = (int)(x2 + ((i - y2) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float texSu = u2 + (i - y2) * du1Step;
                    float texSv = v2 + (i - y2) * dv1Step;
                    float texSw = w2 + (i - y2) * dw1Step;

                    float texEu = u1 + (i - y1) * du2Step;
                    float texEv = v1 + (i - y1) * dv2Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSu, ref texEu);
                        Swap(ref texSv, ref texEv);
                        Swap(ref texSw, ref texEw);
                    }

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texW = (1f - t) * texSw + t * texEw;

                        if (this.IsVisible(j, i) && (DepthBuffer == null || texW > DepthBuffer[j, i]))
                        {
                            this[j, i] = @char;
                            if (DepthBuffer != null) DepthBuffer[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
            }
        }

        public void FillTriangle(
            VectorInt a,
            Vector3 texA,
            VectorInt b,
            Vector3 texB,
            VectorInt c,
            Vector3 texC,
            Image image
            )
            => this.FillTriangle(
                a.X, a.Y,
                texA.X, texA.Y, texA.Z,
                b.X, b.Y,
                texB.X, texB.Y, texB.Z,
                c.X, c.Y,
                texC.X, texC.Y, texC.Z,
                image
                );
        public void FillTriangle(
            int x1, int y1,
            float u1, float v1, float w1,
            int x2, int y2,
            float u2, float v2, float w2,
            int x3, int y3,
            float u3, float v3, float w3,
            Image image
            )
        {
            // sort the points vertically
            if (y2 < y1)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
                Swap(ref u1, ref u2);
                Swap(ref v1, ref v2);
                Swap(ref w1, ref w2);
            }

            if (y3 < y1)
            {
                Swap(ref x1, ref x3);
                Swap(ref y1, ref y3);
                Swap(ref u1, ref u3);
                Swap(ref v1, ref v3);
                Swap(ref w1, ref w3);
            }

            if (y2 > y3)
            {
                Swap(ref x2, ref x3);
                Swap(ref y2, ref y3);
                Swap(ref u2, ref u3);
                Swap(ref v2, ref v3);
                Swap(ref w2, ref w3);
            }

            int dy1 = y2 - y1;
            int dx1 = x2 - x1;
            float dv1 = v2 - v1;
            float du1 = u2 - u1;
            float dw1 = w2 - w1;

            int dy2 = y3 - y1;
            int dx2 = x3 - x1;
            float dv2 = v3 - v1;
            float du2 = u3 - u1;
            float dw2 = w3 - w1;

            float texU, texV, texW;

            float daxStep = 0f;
            float dbxStep = 0f;
            float du1Step = 0f;
            float dv1Step = 0f;
            float du2Step = 0f;
            float dv2Step = 0f;
            float dw1Step = 0f;
            float dw2Step = 0f;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
            if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy2 != 0) du2Step = du2 / MathF.Abs(dy2);
            if (dy2 != 0) dv2Step = dv2 / MathF.Abs(dy2);
            if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

            if (dy1 != 0)
            {
                for (int i = y1; i <= y2; i++)
                {
                    int ax = (int)(x1 + ((i - y1) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float texSu = u1 + (i - y1) * du1Step;
                    float texSv = v1 + (i - y1) * dv1Step;
                    float texSw = w1 + (i - y1) * dw1Step;

                    float texEu = u1 + (i - y1) * du2Step;
                    float texEv = v1 + (i - y1) * dv2Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSu, ref texEu);
                        Swap(ref texSv, ref texEv);
                        Swap(ref texSw, ref texEw);
                    }

                    texU = texSu;
                    texV = texSv;
                    texW = texSw;

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texU = (1f - t) * texSu + t * texEu;
                        texV = (1f - t) * texSv + t * texEv;
                        texW = (1f - t) * texSw + t * texEw;

                        if (this.IsVisible(j, i) && (DepthBuffer == null || texW > DepthBuffer[j, i]))
                        {
                            this[j, i] = Color.ToCharacterShaded(image.NormalizedSample(texU / texW, texV / texW));
                            if (DepthBuffer != null) DepthBuffer[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
            }

            dy1 = y3 - y2;
            dx1 = x3 - x2;
            dv1 = v3 - v2;
            du1 = u3 - u2;
            dw1 = w3 - w2;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            du1Step = 0f;
            dv1Step = 0f;
            if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
            if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy1 != 0)
            {
                for (int i = y2; i <= y3; i++)
                {
                    int ax = (int)(x2 + ((i - y2) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float texSu = u2 + (i - y2) * du1Step;
                    float texSv = v2 + (i - y2) * dv1Step;
                    float texSw = w2 + (i - y2) * dw1Step;

                    float texEu = u1 + (i - y1) * du2Step;
                    float texEv = v1 + (i - y1) * dv2Step;
                    float texEw = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref texSu, ref texEu);
                        Swap(ref texSv, ref texEv);
                        Swap(ref texSw, ref texEw);
                    }

                    texU = texSu;
                    texV = texSv;
                    texW = texSw;

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        texU = (1f - t) * texSu + t * texEu;
                        texV = (1f - t) * texSv + t * texEv;
                        texW = (1f - t) * texSw + t * texEw;

                        if (this.IsVisible(j, i) && (DepthBuffer == null || texW > DepthBuffer[j, i]))
                        {
                            this[j, i] = Color.ToCharacterShaded(image.NormalizedSample(texU / texW, texV / texW));
                            if (DepthBuffer != null) DepthBuffer[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
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

            DepthBuffer?.Resize();

            ConsoleBuffer = new CharInfo[bufferWidth * bufferHeight];
            ConsoleRect = new SmallRect() { Left = 0, Top = 0, Right = bufferWidth, Bottom = bufferHeight };
            return true;
        }
    }
}
