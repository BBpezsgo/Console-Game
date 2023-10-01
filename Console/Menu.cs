using Win32;

namespace ConsoleGame
{
    public class Menu
    {
        readonly struct MenuOption
        {
            public readonly string Label;
            public readonly Action Callback;

            public MenuOption(string label, Action callback)
            {
                Label = label;
                Callback = callback;
            }
        }

        readonly ConsoleRenderer Renderer;
        readonly MenuOption[] Options;
        readonly string Title;

        int Selected;

        public Menu(ConsoleRenderer renderer, string title, params (string, Action)[] options)
        {
            Renderer = renderer;
            Title = title;

            Options = new MenuOption[options.Length];
            for (int i = 0; i < options.Length; i++)
            { Options[i] = new MenuOption(options[i].Item1, options[i].Item2); }
            Selected = 0;
        }

        public void Tick()
        {
            int width = 40;
            int height = 2 + 2 + 1 + Options.Length;
            RectInt rect = new((Renderer.Width / 2) - (width / 2), (Renderer.Height / 2) - (height / 2), width, height);

            Renderer.DrawBox(rect, ByteColor.Black, ByteColor.White, Ascii.BoxSides);

            if (!string.IsNullOrEmpty(Title))
            {
                int titleLabelX = rect.X + ((rect.Width / 2) - (Title.Length / 2));
                Renderer.DrawLabel(titleLabelX, rect.Y, Title);
                Renderer[titleLabelX - 1, rect.Y].Char = ' ';
                Renderer[titleLabelX - 2, rect.Y].Char = '┤';
                Renderer[titleLabelX + Title.Length + 0, rect.Y].Char = ' ';
                Renderer[titleLabelX + Title.Length + 1, rect.Y].Char = '├';
            }

            if (Keyboard.IsKeyDown('W') || Keyboard.IsKeyDown(VirtualKeyCodes.UP))
            {
                Selected--;
                if (Selected < 0)
                { Selected = Options.Length - 1; }
                if (Selected >= Options.Length)
                { Selected = 0; }
            }

            if (Keyboard.IsKeyDown('S') || Keyboard.IsKeyDown(VirtualKeyCodes.DOWN))
            {
                Selected++;
                if (Selected < 0)
                { Selected = Options.Length - 1; }
                if (Selected >= Options.Length)
                { Selected = 0; }
            }

            if (Mouse.IsLeftDown && rect.Contains(Mouse.X, Mouse.Y))
            {
                int i = Mouse.Y;
                i -= rect.Y + 3;

                if (i >= 0 && i < Options.Length)
                { Selected = i; }
            }

            int clicked = -1;
            if (Keyboard.IsKeyDown(VirtualKeyCodes.RETURN))
            { clicked = Selected; }

            for (int i = 0; i < Options.Length; i++)
            {
                MenuOption option = Options[i];

                string label;
                byte color;
                if (i == clicked)
                {
                    label = $"> {option.Label}";
                    color = ByteColor.BrightYellow;
                }
                else if (i == Selected)
                {
                    label = $"> {option.Label}";
                    color = ByteColor.BrightCyan;
                }
                else
                {
                    label = $"  {option.Label}";
                    color = ByteColor.White;
                }

                Renderer.DrawLabel(rect.X + 2, rect.Y + 3 + i, label, ByteColor.Black, color);
            }

            if (clicked != -1)
            { Options[clicked].Callback?.Invoke(); }
        }
    }
}
