﻿using System.Numerics;

namespace ConsoleGame
{
    public static class ColorUtils
    {
        public static void Threshold(Span<Color> buffer, Color threshold)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = buffer[i] - threshold;
                buffer[i].R = Math.Max(0, buffer[i].R);
                buffer[i].G = Math.Max(0, buffer[i].G);
                buffer[i].B = Math.Max(0, buffer[i].B);
            }
        }

        public static void Threshold<T>(Span<T> buffer, T threshold)
            where T : INumber<T>
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = buffer[i] - threshold;
                buffer[i] = T.Max(T.Zero, buffer[i]);
            }
        }

        public static void Blur(Span<Color24> pix, int w, int h, int radius)
        {
            if (radius < 1) return;

            int wm = w - 1;
            int hm = h - 1;
            int wh = w * h;

            int div = radius + radius + 1;

            int[] r = new int[wh];
            int[] g = new int[wh];
            int[] b = new int[wh];

            int rsum;
            int gsum;
            int bsum;

            int x; int y;

            int i;

            int yp;
            int yi;
            int yw;

            Color24 p;
            Color24 p1;
            Color24 p2;

            int[] vmin = new int[Math.Max(w, h)];
            int[] vmax = new int[Math.Max(w, h)];

            int[] dv = new int[256 * div];
            for (i = 0; i < 256 * div; i++)
            { dv[i] = i / div; }

            yw = yi = 0;

            for (y = 0; y < h; y++)
            {
                rsum = gsum = bsum = 0;
                for (i = -radius; i <= radius; i++)
                {
                    p = pix[yi + Math.Min(wm, Math.Max(i, 0))];
                    rsum += p.R;
                    gsum += p.G;
                    bsum += p.B;
                }
                for (x = 0; x < w; x++)
                {

                    r[yi] = dv[rsum];
                    g[yi] = dv[gsum];
                    b[yi] = dv[bsum];

                    if (y == 0)
                    {
                        vmin[x] = Math.Min(x + radius + 1, wm);
                        vmax[x] = Math.Max(x - radius, 0);
                    }
                    p1 = pix[yw + vmin[x]];
                    p2 = pix[yw + vmax[x]];

                    rsum += ((p1.R) - (p2.R));
                    gsum += ((p1.G) - (p2.G));
                    bsum += p1.B - p2.B;
                    yi++;
                }
                yw += w;
            }

            for (x = 0; x < w; x++)
            {
                rsum = gsum = bsum = 0;
                yp = -radius * w;
                for (i = -radius; i <= radius; i++)
                {
                    yi = Math.Max(0, yp) + x;
                    rsum += r[yi];
                    gsum += g[yi];
                    bsum += b[yi];
                    yp += w;
                }
                yi = x;
                for (y = 0; y < h; y++)
                {
                    pix[yi] = new Color24(dv[rsum], dv[gsum], dv[bsum]);
                    if (x == 0)
                    {
                        vmin[y] = Math.Min(y + radius + 1, hm) * w;
                        vmax[y] = Math.Max(y - radius, 0) * w;
                    }
                    p1 = (Color24)(x + vmin[y]);
                    p2 = (Color24)(x + vmax[y]);

                    rsum += r[(int)p1] - r[(int)p2];
                    gsum += g[(int)p1] - g[(int)p2];
                    bsum += b[(int)p1] - b[(int)p2];

                    yi += w;
                }
            }
        }

        public static void Blur<TColor>(Span<TColor> pix, int w, int h, int radius, Func<TColor, Color24> convTo, Func<Color24, TColor> convFrom)
        {
            if (radius < 1) return;

            int wm = w - 1;
            int hm = h - 1;
            int wh = w * h;
            int div = radius + radius + 1;
            int[] r = new int[wh];
            int[] g = new int[wh];
            int[] b = new int[wh];
            int rsum, gsum, bsum, x, y, i, yp, yi, yw;
            Color24 p, p1, p2;
            int[] vmin = new int[Math.Max(w, h)];
            int[] vmax = new int[Math.Max(w, h)];

            int[] dv = new int[256 * div];
            for (i = 0; i < 256 * div; i++)
            {
                dv[i] = i / div;
            }

            yw = yi = 0;

            for (y = 0; y < h; y++)
            {
                rsum = gsum = bsum = 0;
                for (i = -radius; i <= radius; i++)
                {
                    p = convTo.Invoke(pix[yi + Math.Min(wm, Math.Max(i, 0))]);
                    rsum += p.R;
                    gsum += p.G;
                    bsum += p.B;
                }
                for (x = 0; x < w; x++)
                {

                    r[yi] = dv[rsum];
                    g[yi] = dv[gsum];
                    b[yi] = dv[bsum];

                    if (y == 0)
                    {
                        vmin[x] = Math.Min(x + radius + 1, wm);
                        vmax[x] = Math.Max(x - radius, 0);
                    }
                    p1 = convTo.Invoke(pix[yw + vmin[x]]);
                    p2 = convTo.Invoke(pix[yw + vmax[x]]);

                    rsum += p1.R - p2.R;
                    gsum += p1.G - p2.G;
                    bsum += p1.B - p2.B;
                    yi++;
                }
                yw += w;
            }

            for (x = 0; x < w; x++)
            {
                rsum = gsum = bsum = 0;
                yp = -radius * w;
                for (i = -radius; i <= radius; i++)
                {
                    yi = Math.Max(0, yp) + x;
                    rsum += r[yi];
                    gsum += g[yi];
                    bsum += b[yi];
                    yp += w;
                }
                yi = x;
                for (y = 0; y < h; y++)
                {
                    pix[yi] = convFrom.Invoke(new Color24(dv[rsum], dv[gsum], dv[bsum]));
                    if (x == 0)
                    {
                        vmin[y] = Math.Min(y + radius + 1, hm) * w;
                        vmax[y] = Math.Max(y - radius, 0) * w;
                    }
                    p1 = (Color24)(x + vmin[y]);
                    p2 = (Color24)(x + vmax[y]);

                    rsum += r[(int)p1] - r[(int)p2];
                    gsum += g[(int)p1] - g[(int)p2];
                    bsum += b[(int)p1] - b[(int)p2];

                    yi += w;
                }
            }
        }

        public static void Add<TSelf, TOther>(this Span<TSelf> to, ReadOnlySpan<TOther> what)
            where TSelf : IAdditionOperators<TSelf, TOther, TSelf>
        {
            for (int i = 0; i < what.Length; i++)
            { to[i] += what[i]; }
        }

        public static void Bloom(Span<Color> buffer, int w, int h, int radius)
        {
            if (radius < 1) return;
            Span<Color> bloomBuffer = new Color[buffer.Length];
            CalculateBloom(buffer, bloomBuffer, w, h, radius);
            Add(buffer, (ReadOnlySpan<Color>)bloomBuffer);
        }

        public static void CalculateBloom(Span<Color> buffer, Span<Color> bloomBuffer, int w, int h, int radius)
        {
            buffer.CopyTo(bloomBuffer);
            ColorUtils.Threshold(bloomBuffer, Color.White);
            ColorUtils.Blur(bloomBuffer, w, h, radius, v => (Color24)v, v => (Color)v);
        }
    }
}
