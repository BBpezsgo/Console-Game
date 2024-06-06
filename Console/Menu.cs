namespace ConsoleGame;

public class Menu
{
    protected readonly struct MenuOption
    {
        public readonly string Label;
        public readonly Action Callback;

        public MenuOption(string label, Action callback)
        {
            Label = label;
            Callback = callback;
        }
    }

    protected readonly MenuOption[] Options;
    public RectInt ContentRect;

    protected int Selected;

    public Menu(params (string, Action)[] options)
    {
        Options = new MenuOption[options.Length];
        for (int i = 0; i < options.Length; i++)
        { Options[i] = new MenuOption(options[i].Item1, options[i].Item2); }
        Selected = 0;
    }

    public void Tick(RectInt contentRect)
    {
        if ((ConsoleKeyboard.IsKeyDown('W') || ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Up)) && Game.Instance.HandleInput)
        {
            Selected--;
            if (Selected < 0)
            { Selected = Options.Length - 1; }
            if (Selected >= Options.Length)
            { Selected = 0; }
        }

        if ((ConsoleKeyboard.IsKeyDown('S') || ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Down)) && Game.Instance.HandleInput)
        {
            Selected++;
            if (Selected < 0)
            { Selected = Options.Length - 1; }
            if (Selected >= Options.Length)
            { Selected = 0; }
        }

        if (ConsoleMouse.IsPressed(MouseButton.Left) &&
            contentRect.Contains(ConsoleMouse.RecordedConsolePosition) &&
            Game.Instance.HandleInput)
        {
            int i = ConsoleMouse.RecordedConsolePosition.Y;
            i -= contentRect.Y;

            if (i >= 0 && i < Options.Length)
            { Selected = i; }
        }

        int clicked = -1;
        if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Return))
        { clicked = Selected; }

        for (int i = 0; i < Options.Length; i++)
        {
            MenuOption option = Options[i];

            string label;
            byte color;
            if (i == clicked)
            {
                label = $"> {option.Label}";
                color = CharColor.BrightYellow;
            }
            else if (i == Selected)
            {
                label = $"> {option.Label}";
                color = CharColor.BrightCyan;
            }
            else
            {
                label = $"  {option.Label}";
                color = CharColor.White;
            }

            Game.Renderer.Text(contentRect.X, contentRect.Y + i, label, color);
        }

        if (clicked != -1)
        { Select(clicked); }
    }

    public void Select(int optionIndex)
    {
        Options[optionIndex].Callback?.Invoke();
        Selected = 0;
    }

    public void Select(string option)
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (string.Equals(Options[i].Label, option, StringComparison.Ordinal))
            {
                Select(i);
                break;
            }
        }
    }
}
