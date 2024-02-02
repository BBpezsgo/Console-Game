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
            this BufferedRenderer<Color> renderer,
            int radius) => ColorUtils.Bloom(renderer.Buffer.ToArray(), renderer.Width, renderer.Height, radius);

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            VectorInt a,
            VectorInt b,
            VectorInt c,
            T color)
            => renderer.FillTriangle(
                a.X, a.Y,
                b.X, b.Y,
                c.X, c.Y,
                color);

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            Buffer<float>? depth,
            VectorInt a, float depthA,
            VectorInt b, float depthB,
            VectorInt c, float depthC,
            T color)
            => renderer.FillTriangle<T>(
                depth,
                a.X, a.Y, depthA,
                b.X, b.Y, depthB,
                c.X, c.Y, depthC,
                color);

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            Buffer<float>? depth,
            VectorInt a, Vector3 texA,
            VectorInt b, Vector3 texB,
            VectorInt c, Vector3 texC,
            Image image, Func<Color, T> converter)
            => renderer.FillTriangle<T>(
                depth,
                a.X, a.Y, texA.X, texA.Y, texA.Z,
                b.X, b.Y, texB.X, texB.Y, texB.Z,
                c.X, c.Y, texC.X, texC.Y, texC.Z,
                image, converter);

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            Buffer<float>? depth,
            int x1, int y1, float u1, float v1, float w1,
            int x2, int y2, float u2, float v2, float w2,
            int x3, int y3, float u3, float v3, float w3,
            Image image, Func<Color, T> converter)
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
                            Color c = image.NormalizedSample(texU / texW, texV / texW);
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
                            Color c = image.NormalizedSample(texU / texW, texV / texW);
                            renderer[j, i] = converter.Invoke(c);
                            // BloomBlur[j, i] = c;
                            if (depth is not null) depth[j, i] = texW;
                        }

                        t += tStep;
                    }
                }
            }
        }

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            T color)
        {
            int width = renderer.Width;
            int height = renderer.Height;

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
                        if (!renderer.IsVisible(x, y)) continue;
                        renderer[Convert.ToInt32(x + y * width)] = color;
                    }
                    for (int x = (xf < width ? Convert.ToInt32(xf) : width - 1);
                         x >= (xt > 0 ? xt : 0); x--)
                    {
                        if (!renderer.IsVisible(x, y)) continue;
                        renderer[Convert.ToInt32(x + y * width)] = color;
                    }
                }
                xf += dx_far;
                if (y < y1)
                { xt += dx_upper; }
                else
                { xt += dx_low; }
            }
        }

        public static void FillTriangle<T>(
            this Renderer<T> renderer,
            Buffer<float>? depth,
            int x1, int y1, float w1,
            int x2, int y2, float w2,
            int x3, int y3, float w3,
            T color)
        {
            // sort the points vertically
            if (y2 < y1)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
                Swap(ref w1, ref w2);
            }

            if (y3 < y1)
            {
                Swap(ref x1, ref x3);
                Swap(ref y1, ref y3);
                Swap(ref w1, ref w3);
            }

            if (y2 > y3)
            {
                Swap(ref x2, ref x3);
                Swap(ref y2, ref y3);
                Swap(ref w2, ref w3);
            }

            int dy1 = y2 - y1;
            int dx1 = x2 - x1;
            float dw1 = w2 - w1;

            int dy2 = y3 - y1;
            int dx2 = x3 - x1;
            float dw2 = w3 - w1;

            float w;

            float daxStep = 0f;
            float dbxStep = 0f;
            float dw1Step = 0f;
            float dw2Step = 0f;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

            if (dy1 != 0)
            {
                for (int i = y1; i <= y2; i++)
                {
                    int ax = (int)(x1 + ((i - y1) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float sw = w1 + (i - y1) * dw1Step;

                    float ew = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref sw, ref ew);
                    }

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        w = (1f - t) * sw + t * ew;

                        if (renderer.IsVisible(j, i) && (depth is null || w > depth[j, i]))
                        {
                            renderer[j, i] = color;
                            if (depth is not null) depth[j, i] = w;
                        }

                        t += tStep;
                    }
                }
            }

            dy1 = y3 - y2;
            dx1 = x3 - x2;
            dw1 = w3 - w2;

            if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

            if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

            if (dy1 != 0)
            {
                for (int i = y2; i <= y3; i++)
                {
                    int ax = (int)(x2 + ((i - y2) * daxStep));
                    int bx = (int)(x1 + ((i - y1) * dbxStep));

                    float sw = w2 + (i - y2) * dw1Step;

                    float ew = w1 + (i - y1) * dw2Step;

                    if (ax > bx)
                    {
                        Swap(ref ax, ref bx);
                        Swap(ref sw, ref ew);
                    }

                    float tStep = 1f / (float)(bx - ax);
                    float t = 0f;

                    for (int j = ax; j < bx; j++)
                    {
                        w = (1f - t) * sw + t * ew;

                        if (renderer.IsVisible(j, i) && (depth is null || w > depth[j, i]))
                        {
                            renderer[j, i] = color;
                            if (depth is not null) depth[j, i] = w;
                        }

                        t += tStep;
                    }
                }
            }
        }

        public static void DrawLines<T>(
            this Renderer<T> renderer,
            VectorInt[] points,
            T c,
            bool connectEnd = false)
        {
            for (int i = 1; i < points.Length; i++)
            { DrawLine(renderer, points[i - 1], points[i], c); }

            if (connectEnd && points.Length > 2)
            { DrawLine(renderer, points[0], points[^1], c); }
        }

        /*
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
        */

        /// <summary>
        /// Source: <see href="https://stackoverflow.com/a/32252934">StackOverflow</see>
        /// </summary>
        public static void DrawLine<T>(
            this Renderer<T> renderer,
            VectorInt a,
            VectorInt b,
            T c)
            => DrawLine(renderer, a.X, a.Y, b.X, b.Y, c);
        /// <summary>
        /// Source: <see href="https://stackoverflow.com/a/32252934">StackOverflow</see>
        /// </summary>
        public static void DrawLine<T>(
            this Renderer<T> renderer,
            int x1, int y1,
            int x2, int y2,
            T c)
        {
            int Dx = x2 - x1;
            int Dy = y2 - y1;

            int Sx = Math.Sign(Dx);
            int Sy = Math.Sign(Dy);

            Dx = Math.Abs(Dx);
            Dy = Math.Abs(Dy);
            int D = Math.Max(Dx, Dy);

            double R = D / 2;

            int X = x1;
            int Y = y1;
            if (Dx > Dy)
            {
                for (int I = 0; I < D; I++)
                {
                    renderer[X, Y] = c;
                    X += Sx; R += Dy;
                    if (R >= Dx)
                    {
                        Y += Sy;
                        R -= Dx;
                    }
                }
            }
            else
            {
                for (int I = 0; I < D; I++)
                {
                    renderer[X, Y] = c;
                    Y += Sy;
                    R += Dx;
                    if (R >= Dy)
                    {
                        X += Sx;
                        R -= Dy;
                    }
                }
            }
        }

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            Image? image,
            VectorInt position,
            bool fixWidth,
            Func<Color, T> converter)
        {
            if (!image.HasValue) return;
            DrawImage(renderer, image.Value, position, fixWidth, converter);
        }
        public static void DrawImage<T>(
            this Renderer<T> renderer,
            TransparentImage? image,
            VectorInt position,
            bool fixWidth,
            Func<T, TransparentColor, T> blender)
        {
            if (!image.HasValue) return;
            DrawImage(renderer, image.Value, position, fixWidth, blender);
        }

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            Image image,
            VectorInt position,
            bool fixWidth,
            Func<Color, T> converter)
        {
            int w = image.Width;
            int h = image.Height;

            if (fixWidth) w *= 2;

            for (int y_ = 0; y_ < h; y_++)
            {
                for (int x_ = 0; x_ < w; x_++)
                {
                    VectorInt point = new(x_ + position.X, y_ + position.Y);
                    if (!renderer.IsVisible(point.X, point.Y)) continue;
                    Color c = image[fixWidth ? x_ / 2 : x_, y_];
                    renderer[point.X, point.Y] = converter.Invoke(c); // new ConsoleChar(' ', CharColor.Black, Color.To4bitIRGB(c));
                }
            }
        }

        public static void DrawImage<T>(
            this Renderer<T> renderer,
            TransparentImage image,
            VectorInt position,
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
                    VectorInt point = new(x_ + position.X, y_ + position.Y);
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
