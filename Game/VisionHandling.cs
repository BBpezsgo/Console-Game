namespace ConsoleGame;

public partial class Game
{
    public Vector2Int ViewportWorldPosition = new(0, -4);

    public Entity? FollowEntity;

    public static Vector2 WorldToViewport(Vector2 worldPosition)
        => worldPosition - (Vector2)Game.Instance.ViewportWorldPosition;

    public static Vector2 ViewportToWorld(Vector2 viewportPosition)
        => viewportPosition + (Vector2)Game.Instance.ViewportWorldPosition;

    public static Vector2Int ViewportToConsole(Vector2 viewportPosition)
        => Vector.Round(viewportPosition * new Vector2(2f, 1f));
    public static Vector2Int ViewportToConsole(float x, float y)
        => Vector.Round(x * 2f, y);

    public static Vector2 ConsoleToViewport(Vector2Int consolePosition)
        => (Vector2)consolePosition * new Vector2(0.5f, 1f);
    public static Vector2 ConsoleToViewport(int x, int y)
        => new(x * 0.5f, y);

    public static Vector2 ConsoleToWorld(Vector2Int consolePosition)
        => ((Vector2)consolePosition * new Vector2(0.5f, 1f)) + (Vector2)Game.Instance.ViewportWorldPosition;
    public static Vector2 ConsoleToWorld(int x, int y)
        => new Vector2(x * 0.5f, y) + (Vector2)Game.Instance.ViewportWorldPosition;

    public static Vector2Int WorldToConsole(Vector2 worldPosition)
        => Vector.Round((worldPosition - (Vector2)Game.Instance.ViewportWorldPosition) * new Vector2(2f, 1f));
    public static Vector2Int WorldToConsole(Vector2Int worldPosition)
        => (worldPosition - Game.Instance.ViewportWorldPosition) * new Vector2Int(2, 1);
    public static Vector2Int WorldToConsole(float x, float y)
        => Vector.Round((x - Game.Instance.ViewportWorldPosition.X) * 2, y - Game.Instance.ViewportWorldPosition.Y);

    public static RectInt WorldToConsole(Rect worldRect) => new(WorldToConsole(worldRect.Position), Vector.Round(worldRect.Size * new Vector2(2f, 1f)));

    public static Rect VisibleWorldRect() => new()
    {
        X = Game.Instance.ViewportWorldPosition.X,
        Y = Game.Instance.ViewportWorldPosition.Y,
        Width = Renderer.Width / 2,
        Height = Renderer.Height,
    };

    public static bool IsVisible(Vector2 worldPosition)
    {
        Vector2 viewportPosition = Game.WorldToViewport(worldPosition);
        if (viewportPosition.X < 0f || viewportPosition.Y < 0f) return false;

        Vector2Int consolePosition = Game.ViewportToConsole(viewportPosition);
        if (consolePosition.X >= Renderer.Width || consolePosition.Y >= Renderer.Height) return false;

        return !IsOnGui(consolePosition);
    }

    public static bool IsOnGui(Vector2Int consolePosition)
    {
        if (consolePosition.Y < 4) return true;

        return false;
    }
}
