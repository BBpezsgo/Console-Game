namespace ConsoleGame;

public class MenuBoxed : Menu
{
    readonly string Title;

    public MenuBoxed(string title, params (string, Action)[] options) : base(options)
    {
        Title = title;
    }

    public void Tick(int width)
        => Tick(width, 2 + 2 + 1 + Options.Length);
    public void Tick(int width, int height)
    {
        RectInt borderRect = GUI.GetCenteredBox(width, height);

        Game.Renderer.Box(borderRect, CharColor.Black, CharColor.White, SideCharacters.BoxSides);

        if (!string.IsNullOrEmpty(Title))
        {
            int titleLabelX = borderRect.X + ((borderRect.Width / 2) - (Title.Length / 2));
            Game.Renderer.Text(titleLabelX, borderRect.Y, Title);
            Game.Renderer.Set(titleLabelX - 1, borderRect.Y, new ConsoleChar(' ', CharColor.White));
            Game.Renderer.Set(titleLabelX - 2, borderRect.Y, new ConsoleChar('┤', CharColor.White));
            Game.Renderer.Set(titleLabelX + Title.Length + 0, borderRect.Y, new ConsoleChar(' ', CharColor.White));
            Game.Renderer.Set(titleLabelX + Title.Length + 1, borderRect.Y, new ConsoleChar('├', CharColor.White));
        }

        base.Tick(borderRect.Expand(-3, -2, -2, -2));
    }
}
