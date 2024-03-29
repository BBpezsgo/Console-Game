﻿using System.Runtime.Versioning;
using Win32;
using Win32.Gdi32;

namespace ConsoleGame
{
    public partial class MeshRenderer
    {
        int RendererMode = 4;

        readonly Camera camera = new();

        Vector2Int LastMousePosition;

        bool LockCursor;

        [SupportedOSPlatform("windows")]
        unsafe void Tick()
        {
            if (renderer is null) throw new NullReferenceException($"{nameof(renderer)} is null");

            Keyboard.Tick();

            renderer.Clear();
            renderer.RefreshBufferSize();

            FpsCounter.Sample((int)MathF.Round(1f / deltaTime));

            if (Keyboard.IsKeyDown('Q'))
            {
                RendererMode++;
                RendererMode %= 5;
            }

            if (true)
            {
                if (Keyboard.IsKeyDown('C'))
                { LockCursor = !LockCursor; }

                if (RendererMode == 0)
                {
                    Do3DStuff(renderer, renderer.DepthBuffer, v => new ConsoleChar(' ', 0, CharColor.From24bitColor((GdiColor)v)));
                }
                else if (RendererMode == 1)
                {
                    Do3DStuff(renderer, renderer.DepthBuffer, v => CharColor.ToCharacterShaded((GdiColor)v));
                }
                else if (RendererMode == 2)
                {
                    Do3DStuff(renderer, renderer.DepthBuffer, v => CharColor.ToCharacterColored((GdiColor)v));
                }
                else if (RendererMode == 3)
                {
                    AnsiRenderer renderer = new(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.Extended };
                    Do3DStuff(renderer, renderer.DepthBuffer);
                }
                else
                {
                    AnsiRenderer renderer = new(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.TrueColor };
                    Do3DStuff(renderer, renderer.DepthBuffer);
                }
            }
            else
            {
                if (RendererMode == 0)
                {
                    DoColorTest(this.renderer, v => new ConsoleChar(' ', 0, CharColor.From24bitColor((GdiColor)v)));
                }
                else if (RendererMode == 1)
                {
                    DoColorTest(this.renderer, v => CharColor.ToCharacterShaded((GdiColor)v));
                }
                else if (RendererMode == 2)
                {
                    DoColorTest(this.renderer, v => CharColor.ToCharacterColored((GdiColor)v));
                }
                else if (RendererMode == 3)
                {
                    DoColorTest(new AnsiRenderer(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.Extended });
                }
                else
                {
                    DoColorTest(new AnsiRenderer(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.TrueColor });
                }
            }
        }

        [SupportedOSPlatform("windows")]
        void Do3DStuff<T>(Renderer<T> renderer, Buffer<float>? depth, Func<ColorF, T> converter)
        {
            camera.HandleInput(LockCursor, ref LastMousePosition);
            camera.DoMath(renderer.Size);

            Renderer3D.Render(renderer, depth, MeshToRender, camera, ImageToRender, converter);

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }

        [SupportedOSPlatform("windows")]
        void Do3DStuff(Renderer<ColorF> renderer, Buffer<float>? depth)
        {
            camera.HandleInput(LockCursor, ref LastMousePosition);
            camera.DoMath(renderer.Size);

            Renderer3D.Render(renderer, depth, MeshToRender, camera, ImageToRender);

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }

        float ah2 = .5f;
        void DoColorTest<T>(Renderer<T> renderer, Func<ColorF, T> converter)
        {
            for (int y = 0; y < renderer.Height; y++)
            {
                for (int x = 0; x < renderer.Width; x++)
                {
                    float vx = (float)x / (float)renderer.Width;
                    float vy = (float)y / (float)renderer.Height;
                    ColorF c = ColorF.FromHSL(vx, vy, ah2);
                    renderer[x, y] = converter.Invoke(c);
                }
            }

            if (Keyboard.IsKeyDown('W'))
            { ah2 = Math.Clamp(ah2 + .1f, 0f, 1f); }
            if (Keyboard.IsKeyDown('S'))
            { ah2 = Math.Clamp(ah2 - .1f, 0f, 1f); }

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }
        void DoColorTest(Renderer<ColorF> renderer)
        {
            for (int y = 0; y < renderer.Height; y++)
            {
                for (int x = 0; x < renderer.Width; x++)
                {
                    float vx = (float)x / (float)renderer.Width;
                    float vy = (float)y / (float)renderer.Height;
                    ColorF c = ColorF.FromHSL(vx, vy, ah2);
                    renderer[x, y] = c;
                }
            }

            if (Keyboard.IsKeyDown('W'))
            { ah2 = Math.Clamp(ah2 + .1f, 0f, 1f); }
            if (Keyboard.IsKeyDown('S'))
            { ah2 = Math.Clamp(ah2 - .1f, 0f, 1f); }

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }
    }
}
