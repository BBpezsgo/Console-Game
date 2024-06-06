using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame;

public static class GUI
{
    static IRenderer? _renderer;
    [NotNull]
    public static IRenderer? Renderer
    {
        get => _renderer ?? throw new NullReferenceException($"{nameof(_renderer)} is null");
        set => _renderer = value;
    }
    static int Width => Renderer.Width;
    static int Height => Renderer.Height;

    public static RectInt GetCenteredBox(int width, int height) => new(
        (Width / 2) - (width / 2),
        (Height / 2) - (height / 2),
        width,
        height
    );

    public static RectInt GetCenteredBox(Vector2Int size) => new(
        (Width / 2) - (size.X / 2),
        (Height / 2) - (size.Y / 2),
        size.X,
        size.Y
    );
}
