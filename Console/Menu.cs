using Win32;
using Win32.LowLevel;

namespace ConsoleGame
{
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

        protected readonly IRenderer<ConsoleChar> Renderer;
        protected readonly MenuOption[] Options;
        public RectInt ContentRect;

        protected int Selected;

        public Menu(IRenderer<ConsoleChar> renderer, params (string, Action)[] options)
        {
            Renderer = renderer;

            Options = new MenuOption[options.Length];
            for (int i = 0; i < options.Length; i++)
            { Options[i] = new MenuOption(options[i].Item1, options[i].Item2); }
            Selected = 0;
        }

        public void Tick(RectInt contentRect)
        {
            if (Keyboard.IsKeyDown('W') || Keyboard.IsKeyDown(VirtualKeyCode.UP))
            {
                Selected--;
                if (Selected < 0)
                { Selected = Options.Length - 1; }
                if (Selected >= Options.Length)
                { Selected = 0; }
            }

            if (Keyboard.IsKeyDown('S') || Keyboard.IsKeyDown(VirtualKeyCode.DOWN))
            {
                Selected++;
                if (Selected < 0)
                { Selected = Options.Length - 1; }
                if (Selected >= Options.Length)
                { Selected = 0; }
            }

            if (Mouse.IsPressed(MouseButton.Left) && contentRect.Contains(Mouse.RecordedPosition))
            {
                int i = Mouse.RecordedPosition.Y;
                i -= contentRect.Y;

                if (i >= 0 && i < Options.Length)
                { Selected = i; }
            }

            int clicked = -1;
            if (Keyboard.IsKeyDown(VirtualKeyCode.RETURN))
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

                GUI.Label(contentRect.X, contentRect.Y + i, label, ByteColor.Black, color);
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
}
