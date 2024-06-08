namespace ConsoleGame;

public class EntityHoverPopup : RendererComponent
{
    public bool IsShown;
    public const float DistanceToShow = 2f;

    public static bool ShouldNotShow;
    public static EntityHoverPopup? AlreadyShown;

    public Vector2Int Size;

    readonly ImmutableArray<ICanDrawEntityHoverPopup> DrawerComponents;

    public EntityHoverPopup(Entity entity) : base(entity)
    {
        Size = new Vector2Int(20, 5);
        DrawerComponents = Entity.GetComponents<ICanDrawEntityHoverPopup>().ToImmutableArray();
    }

    public override void Render()
    {
        if (!IsShown) return;

        Vector2Int conPos = Game.WorldToConsole(Position);

        RectInt box = new(conPos.X + 1, conPos.Y - Size.Y, Size.X, Size.Y);

        if (box.Y < 4) box.Y = 4;
        if (box.X < 0) box.X = 0;
        if (box.Right >= Game.Renderer.Width) box.X += Game.Renderer.Width - 1 - box.Right;
        if (box.Bottom >= Game.Renderer.Height) box.X += Game.Renderer.Height - 1 - box.Bottom;

        Game.DepthBuffer.Fill(box, Depths.GUI);

        Game.Renderer.Box(box, CharColor.Black, CharColor.White, SideCharacters.BoxSides);
        box.Expand(-1);

        for (int i = 0; i < DrawerComponents.Length; i++)
        {
            DrawerComponents[i].RenderHoverPopup(box);
        }
    }

    public override void Update()
    {
        if (ShouldNotShow)
        {
            IsShown = false;
            return;
        }

        float distance = (Game.ConsoleToWorld(ConsoleMouse.RecordedConsolePosition) - Position).LengthSquared();
        bool shouldShown = distance <= (DistanceToShow * DistanceToShow);

        if (!shouldShown)
        {
            IsShown = false;
            return;
        }

        if (AlreadyShown != null)
        {
            float distance2 = (Game.ConsoleToWorld(ConsoleMouse.RecordedConsolePosition) - AlreadyShown.Position).LengthSquared();
            if (distance2 < distance)
            {
                IsShown = false;
                return;
            }
            AlreadyShown.IsShown = false;
        }

        AlreadyShown = this;
        IsShown = true;
    }
}

public interface ICanDrawEntityHoverPopup
{
    public void RenderHoverPopup(RectInt content);
}
