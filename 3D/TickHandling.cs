using System.Runtime.Versioning;

namespace ConsoleGame;

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

        ConsoleKeyboard.Tick();

        renderer.Clear();
        renderer.RefreshBufferSize();

        FpsCounter.Sample((int)MathF.Round(1f / DeltaTime));

        if (ConsoleKeyboard.IsKeyDown('Q'))
        {
            RendererMode++;
            RendererMode %= 5;
        }

        if (true)
        {
            if (ConsoleKeyboard.IsKeyDown('C'))
            { LockCursor = !LockCursor; }

            if (RendererMode == 0)
            {
                Do3DStuff(renderer, depthBuffer, v => new ConsoleChar(' ', 0, CharColor.From24bitColor((GdiColor)v)));
            }
            else if (RendererMode == 1)
            {
                Do3DStuff(renderer, depthBuffer, v => CharColor.ToCharacterShaded((GdiColor)v));
            }
            else if (RendererMode == 2)
            {
                Do3DStuff(renderer, depthBuffer, v => CharColor.ToCharacterColored((GdiColor)v));
            }
            else if (RendererMode == 3)
            {
                AnsiRenderer renderer = new();
                Do3DStuff(renderer, depthBuffer, v => new AnsiChar(' ', 0, Ansi.ToAnsi256((GdiColor)v)));
            }
            else
            {
                AnsiRendererTrueColor renderer = new();
                Do3DStuff(renderer, depthBuffer, v => new ColoredChar(' ', 0, (GdiColor)v));
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
                DoColorTest(new AnsiRenderer(), v => new AnsiChar(' ', 0, Ansi.ToAnsi256((GdiColor)v)));
            }
            else
            {
                DoColorTest(new AnsiRendererTrueColor(), v => new ColoredChar(' ', 0, (GdiColor)v));
            }
        }
    }

    [SupportedOSPlatform("windows")]
    void Do3DStuff<T>(IRenderer<T> renderer, Buffer<float>? depth, Func<ColorF, T> converter)
    {
        camera.HandleInput(LockCursor, ref LastMousePosition);
        camera.DoMath(new Size(renderer.Width, renderer.Height));

        Renderer3D.Render(renderer, depth, MeshToRender, camera, ImageToRender, converter);

        // renderer.Text(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

        renderer.Render();
    }

    [SupportedOSPlatform("windows")]
    void Do3DStuff(IRenderer<ColorF> renderer, Buffer<float>? depth)
    {
        camera.HandleInput(LockCursor, ref LastMousePosition);
        camera.DoMath(new Size(renderer.Width, renderer.Height));

        Renderer3D.Render(renderer, depth, MeshToRender, camera, ImageToRender);

        // renderer.Text(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

        renderer.Render();
    }

    float ah2 = .5f;
    void DoColorTest<T>(IRenderer<T> renderer, Func<ColorF, T> converter)
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

        if (ConsoleKeyboard.IsKeyDown('W'))
        { ah2 = Math.Clamp(ah2 + .1f, 0f, 1f); }
        if (ConsoleKeyboard.IsKeyDown('S'))
        { ah2 = Math.Clamp(ah2 - .1f, 0f, 1f); }

        // renderer.Text(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

        renderer.Render();
    }
    void DoColorTest(IRenderer<ColorF> renderer)
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

        if (ConsoleKeyboard.IsKeyDown('W'))
        { ah2 = Math.Clamp(ah2 + .1f, 0f, 1f); }
        if (ConsoleKeyboard.IsKeyDown('S'))
        { ah2 = Math.Clamp(ah2 - .1f, 0f, 1f); }

        // renderer.Text(0, 0, $"FPS: {FpsCounter.Value}", CharColor.Silver);

        renderer.Render();
    }
}
