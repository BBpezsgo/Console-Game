namespace ConsoleGame
{
    public partial class Game
    {
        public VectorInt ViewportWorldPosition = new(0, -4);

        public Entity? FollowEntity;

        public static Vector WorldToViewport(Vector worldPosition)
            => worldPosition - Game.Instance.ViewportWorldPosition;

        public static Vector ViewportToWorld(Vector viewportPosition)
            => viewportPosition + Game.Instance.ViewportWorldPosition;

        public static VectorInt ViewportToConsole(Vector viewportPosition)
            => Vector.Round(viewportPosition * new Vector(2f, 1f));
        public static VectorInt ViewportToConsole(float x, float y)
            => Vector.Round(x * 2f, y);

        public static Vector ConsoleToViewport(VectorInt consolePosition)
            => consolePosition * new Vector(0.5f, 1f);
        public static Vector ConsoleToViewport(int x, int y)
            => new(x * 0.5f, y);

        public static Vector ConsoleToWorld(VectorInt consolePosition)
            => (consolePosition * new Vector(0.5f, 1f)) + Game.Instance.ViewportWorldPosition;
        public static Vector ConsoleToWorld(int x, int y)
            => new Vector(x * 0.5f, y) + Game.Instance.ViewportWorldPosition;

        public static VectorInt WorldToConsole(Vector worldPosition)
            => Vector.Round((worldPosition - Game.Instance.ViewportWorldPosition) * new Vector(2f, 1f));
        public static VectorInt WorldToConsole(float x, float y)
            => Vector.Round((x - Game.Instance.ViewportWorldPosition.X) * 2, y - Game.Instance.ViewportWorldPosition.Y);

        public static RectInt WorldToConsole(Rect worldRect) => new(WorldToConsole(worldRect.Position), Vector.Round(worldRect.Size * new Vector(2f, 1f)));

        public static Rect VisibleWorldRect() => new()
        {
            X = Game.Instance.ViewportWorldPosition.X,
            Y = Game.Instance.ViewportWorldPosition.Y,
            Width = Renderer.Width / 2f,
            Height = Renderer.Height,
        };

        public static bool IsVisible(Vector worldPosition)
        {
            Vector viewportPosition = Game.WorldToViewport(worldPosition);
            if (viewportPosition.X < 0f || viewportPosition.Y < 0f) return false;

            VectorInt consolePosition = Game.ViewportToConsole(viewportPosition);
            if (consolePosition.X >= Renderer.Width || consolePosition.Y >= Renderer.Height) return false;

            return !IsOnGui(consolePosition);
        }

        public static bool IsOnGui(VectorInt consolePosition)
        {
            if (consolePosition.Y < 4) return true;

            return false;
        }
    }
}
