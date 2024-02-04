using System.Numerics;
using Win32;

namespace ConsoleGame
{
    public static class RendererExtensions
    {
        static void Swap<T>(ref T a, ref T b)
        {
            T temp = b;
            b = a;
            a = temp;
        }

        public static void ApplyBloom(
            this BufferedRenderer<ColorF> renderer,
            int radius) => ColorUtils.Bloom(renderer.Buffer, renderer.Width, renderer.Height, radius);

        public static void FillTriangle<TPixel>(
            this Renderer<TPixel> renderer,
            Buffer<float>? depth,
            Vector2Int a, Vector3 texA,
            Vector2Int b, Vector3 texB,
            Vector2Int c, Vector3 texC,
            Image image, Func<ColorF, TPixel> converter)
            => renderer.FillTriangle<TPixel>(
                depth,
                a.X, a.Y, texA.X, texA.Y, texA.Z,
                b.X, b.Y, texB.X, texB.Y, texB.Z,
                c.X, c.Y, texC.X, texC.Y, texC.Z,
                image, converter);

        public static void FillTriangle<TPixel>(
            this Renderer<TPixel> renderer,
            Buffer<float>? depth,
            int x1, int y1, float u1, float v1, float w1,
            int x2, int y2, float u2, float v2, float w2,
            int x3, int y3, float u3, float v3, float w3,
            Image image, Func<ColorF, TPixel> converter)
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

                        if (renderer.IsVisible(j, i) && (depth is null || texW > depth[j, i]))
                        {
                            ColorF c = image.NormalizedSample(texU / texW, texV / texW);
                            renderer[j, i] = converter.Invoke(c);
                            // BloomBlur[j, i] = c;
                            if (depth is not null) depth[j, i] = texW;
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

                        if (renderer.IsVisible(j, i) && (depth is null || texW > depth[j, i]))
                        {
                            ColorF c = image.NormalizedSample(texU / texW, texV / texW);
                            renderer[j, i] = converter.Invoke(c);
                            // BloomBlur[j, i] = c;
                            if (depth is not null) depth[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
            }
        }

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            Image? image,
            Vector2Int position,
            bool fixWidth,
            Func<ColorF, T> converter)
        {
            if (!image.HasValue) return;
            DrawImage(renderer, image.Value, position, fixWidth, converter);
        }
        public static void DrawImage<T>(
            this Renderer<T> renderer,
            TransparentImage? image,
            Vector2Int position,
            bool fixWidth,
            Func<T, TransparentColor, T> blender)
        {
            if (!image.HasValue) return;
            DrawImage(renderer, image.Value, position, fixWidth, blender);
        }

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            Image image,
            Vector2Int position,
            bool fixWidth,
            Func<ColorF, T> converter)
        {
            int w = image.Width;
            int h = image.Height;

            if (fixWidth) w *= 2;

            for (int y_ = 0; y_ < h; y_++)
            {
                for (int x_ = 0; x_ < w; x_++)
                {
                    Vector2Int point = new(x_ + position.X, y_ + position.Y);
                    if (!renderer.IsVisible(point.X, point.Y)) continue;
                    ColorF c = image[fixWidth ? x_ / 2 : x_, y_];
                    renderer[point.X, point.Y] = converter.Invoke(c); // new ConsoleChar(' ', CharColor.Black, Color.To4bitIRGB(c));
                }
            }
        }

        public static void DrawImage(this Renderer<ColorF> renderer, Image image, Vector2Int position)
            => renderer.Put(position.X, position.Y, image.Data, image.Width, image.Height);

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            TransparentImage image,
            Vector2Int position,
            bool fixWidth,
            Func<T, TransparentColor, T> blender)
        {
            int w = image.Width;
            int h = image.Height;

            if (fixWidth) w *= 2;

            for (int y_ = 0; y_ < h; y_++)
            {
                for (int x_ = 0; x_ < w; x_++)
                {
                    Vector2Int point = new(x_ + position.X, y_ + position.Y);
                    if (!renderer.IsVisible(point.X, point.Y)) continue;
                    TransparentColor color = image[fixWidth ? x_ / 2 : x_, y_];
                    ref T alreadyThere = ref renderer[point.X, point.Y];
                    T newColor = blender.Invoke(alreadyThere, color);
                    renderer[point.X, point.Y] = newColor; // new ConsoleChar(' ', CharColor.Black, Color.To4bitIRGB(newColor));
                }
            }
        }
    }
}
