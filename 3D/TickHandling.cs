using Win32;

namespace ConsoleGame
{
    public partial class MeshRenderer
    {
        int RendererMode = 1;

        readonly Camera camera = new();

        VectorInt LastMousePosition;

        bool LockCursor;

        unsafe void Tick()
        {
            if (renderer is null) throw new NullReferenceException($"{nameof(renderer)} is null");

            Keyboard.Tick();

            renderer.Clear();
            renderer.Resize();

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
                    Do3DStuff(this.renderer, v => new ConsoleChar(' ', 0, Color.To4bitIRGB(v)));
                }
                else if (RendererMode == 1)
                {
                    Do3DStuff(this.renderer, Color.ToCharacterShaded);
                }
                else if (RendererMode == 2)
                {
                    Do3DStuff(this.renderer, Color.ToCharacterColored);
                }
                else if (RendererMode == 3)
                {
                    Do3DStuff(new AnsiRenderer(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.Extended });
                }
                else
                {
                    Do3DStuff(new AnsiRenderer(Console.WindowWidth, Console.WindowHeight)
                    { ColorType = AnsiColorType.TrueColor });
                }
            }
            else
            {
                if (RendererMode == 0)
                {
                    DoColorTest(this.renderer, v => new ConsoleChar(' ', 0, Color.To4bitIRGB(v)));
                }
                else if (RendererMode == 1)
                {
                    DoColorTest(this.renderer, Color.ToCharacterShaded);
                }
                else if (RendererMode == 2)
                {
                    DoColorTest(this.renderer, Color.ToCharacterColored);
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

        void Do3DStuff<T>(IRenderer<T> renderer, Func<Color, T> converter)
        {
            camera.HandleInput(LockCursor, ref LastMousePosition);
            camera.DoMath(renderer.Rect, out _, out _);

            Renderer3D.Render(renderer, MeshToRender, camera, ImageToRender, converter);

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }
        void Do3DStuff(IRenderer<Color> renderer)
        {
            camera.HandleInput(LockCursor, ref LastMousePosition);
            camera.DoMath(renderer.Rect, out _, out _);

            Renderer3D.Render(renderer, MeshToRender, camera, ImageToRender);

            GUI.Label(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

            renderer.Render();
        }

        float ah2 = .5f;
        void DoColorTest<T>(IRenderer<T> renderer, Func<Color, T> converter)
        {
            for (int y = 0; y < renderer.Height; y++)
            {
                for (int x = 0; x < renderer.Width; x++)
                {
                    float vx = (float)x / (float)renderer.Width;
                    float vy = (float)y / (float)renderer.Height;
                    Color c = Color.FromHSL(vx, vy, ah2);
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
        void DoColorTest(IRenderer<Color> renderer)
        {
            for (int y = 0; y < renderer.Height; y++)
            {
                for (int x = 0; x < renderer.Width; x++)
                {
                    float vx = (float)x / (float)renderer.Width;
                    float vy = (float)y / (float)renderer.Height;
                    Color c = Color.FromHSL(vx, vy, ah2);
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
