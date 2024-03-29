﻿using System.Numerics;
using System.Runtime.Versioning;
using Win32;
using Win32.Gdi32;

namespace ConsoleGame
{
    public enum ImageRenderMode
    {
        Normal,
        Shaded,
        Subpixels,
    }

    public delegate void SimpleEventHandler();

    public class ConsoleRenderer : Win32.ConsoleRenderer, IRendererWithDepth
    {
        bool shouldResize;

        public bool IsBloomEnabled;

        public Buffer<float> DepthBuffer { get; }

        public ref ConsoleChar this[Vector2Int screenPosition] => ref ConsoleBuffer[(screenPosition.Y * BufferWidth) + screenPosition.X];

        public event SimpleEventHandler? OnResized;

        [SupportedOSPlatform("windows")]
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
            Vector2Int imageSize = new(image.Width, image.Height - 1);

            for (int y_ = 0; y_ < rect.Height * 2; y_++)
            {
                for (int x_ = 0; x_ < rect.Width * 2; x_++)
                {
                    Vector2Int pointTL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    Vector2Int pointTR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    Vector2Int pointBL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);
                    Vector2Int pointBR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);


                    if (!IsVisible(pointTL)) continue;

                    byte colorTL = CharColor.From24bitColor(image.GetPixelWithUV(rect.Size, pointTL));

                    byte fg = colorTL;
                    byte bg = colorTL;
                    char c = ' ';

                    if (IsVisible(pointBR))
                    {
                        byte colorTR = CharColor.From24bitColor(image.GetPixelWithUV(rect.Size, pointTR));
                        byte colorBL = CharColor.From24bitColor(image.GetPixelWithUV(rect.Size, pointBL));
                        byte colorBR = CharColor.From24bitColor(image.GetPixelWithUV(rect.Size, pointBR));

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
            Vector2Int imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    Vector2Int point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector2 uv = (Vector2)point / (Vector2)rect.Size;
                    uv *= (Vector2)imageSize;
                    Vector2Int imageCoord = Vector.Floor(uv);

                    ColorF pixel = image[imageCoord.X, imageCoord.Y];
                    this[point] = CharColor.ToCharacterColored((GdiColor)pixel);
                    // BloomBlur[point] = pixel;
                }
            }
        }
        void DrawImageNormal(Image image, RectInt rect)
        {
            Vector2Int imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    Vector2Int point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector2 uv = (Vector2)point / (Vector2)rect.Size;
                    uv *= (Vector2)imageSize;
                    Vector2Int imageCoord = Vector.Floor(uv);

                    ColorF pixel = image[imageCoord.X, imageCoord.Y];
                    byte convertedPixel = CharColor.From24bitColor(pixel);
                    this[point] = new ConsoleChar(' ', CharColor.Black, convertedPixel);
                    // BloomBlur[point] = pixel;
                }
            }
        }

        void DrawImageSubpixel(TransparentImage image, RectInt rect)
        {
            Vector2Int imageSize = new(image.Width, image.Height - 1);

            for (int y_ = 0; y_ < rect.Height * 2; y_++)
            {
                for (int x_ = 0; x_ < rect.Width * 2; x_++)
                {
                    Vector2Int pointTL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    Vector2Int pointTR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Floor(y_ / 2f) + rect.Y);
                    Vector2Int pointBL = new((int)Math.Floor(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);
                    Vector2Int pointBR = new((int)Math.Ceiling(x_ / 2f) + rect.X, (int)Math.Ceiling(y_ / 2f) + rect.Y);

                    if (!IsVisible(pointTL)) continue;

                    byte colorTL = CharColor.From24bitColor((ColorF)image.GetPixelWithUV(rect.Size, pointTL));

                    byte fg = colorTL;
                    byte bg = colorTL;
                    char c = ' ';

                    if (IsVisible(pointBR))
                    {
                        byte colorTR = CharColor.From24bitColor((ColorF)image.GetPixelWithUV(rect.Size, pointTR));
                        byte colorBL = CharColor.From24bitColor((ColorF)image.GetPixelWithUV(rect.Size, pointBL));
                        byte colorBR = CharColor.From24bitColor((ColorF)image.GetPixelWithUV(rect.Size, pointBR));

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
            Vector2Int imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    Vector2Int point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector2 uv = (Vector2)point / (Vector2)rect.Size;
                    uv *= (Vector2)imageSize;
                    Vector2Int imageCoord = Vector.Floor(uv);

                    TransparentColor pixel = image[imageCoord.X, imageCoord.Y];
                    if (pixel.A <= float.Epsilon) continue;
                    ColorF alreadyThere = CharColor.FromCharacter(this[point]);
                    ColorF c = pixel.Blend(alreadyThere);
                    this[point] = CharColor.ToCharacterColored((GdiColor)c);
                    // BloomBlur[point] = c;
                }
            }
        }
        void DrawImageNormal(TransparentImage image, RectInt rect)
        {
            Vector2Int imageSize = new(image.Width, image.Height);

            for (int y_ = 0; y_ < rect.Height; y_++)
            {
                for (int x_ = 0; x_ < rect.Width; x_++)
                {
                    Vector2Int point = new(x_ + rect.X, y_ + rect.Y);
                    if (!IsVisible(point)) continue;
                    Vector2 uv = (Vector2)point / (Vector2)rect.Size;
                    uv *= (Vector2)imageSize;
                    Vector2Int imageCoord = Vector.Floor(uv);

                    TransparentColor pixel = image[imageCoord.X, imageCoord.Y];
                    if (pixel.A <= float.Epsilon) continue;
                    ColorF alreadyThere = CharColor.FromCharacter(this[point]);
                    ColorF c = pixel.Blend(alreadyThere);
                    byte convertedPixel = CharColor.From24bitColor(c);
                    this[point] = new ConsoleChar(' ', CharColor.Black, convertedPixel);
                    // BloomBlur[point] = c;
                }
            }
        }

        public bool IsVisible(Vector2Int position) => IsVisible(position.X, position.Y);

        public void ShouldResize() => shouldResize = true;

        [SupportedOSPlatform("windows")]
        public override void RefreshBufferSize()
        {
            if (!shouldResize) return;
            shouldResize = false;

            Console.Clear();

            base.RefreshBufferSize();

            DepthBuffer.Resize();

            OnResized?.Invoke();
        }
    }
}
