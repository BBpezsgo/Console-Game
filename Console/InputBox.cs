using System.Text;

namespace ConsoleGame;

public class InputBox
{
    readonly string Title;
    readonly Action OnOk;
    readonly Action OnCancel;
    readonly int MaxLength;
    readonly string InitialValue;

    int CursorPosition;
    int Selected;

    public StringBuilder Value;

    public InputBox(string title, string initialValue, int maxLength, Action onOk, Action onCancel)
    {
        Title = title;
        MaxLength = maxLength;
        if (initialValue.Length > maxLength)
        { initialValue = initialValue[..maxLength]; }
        InitialValue = initialValue;
        Value = new StringBuilder(initialValue);
        CursorPosition = initialValue.Length;
        OnOk = onOk;
        OnCancel = onCancel;
    }

    public void Tick(int width)
        => Tick(width, 8);
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

        borderRect.Expand(-2, -2, -2, -2);

        if (ConsoleMouse.IsPressed(MouseButton.Left) && Game.Instance.HandleInput)
        {
            if (ConsoleMouse.RecordedConsolePosition.Y == borderRect.Y && borderRect.Contains(ConsoleMouse.RecordedConsolePosition))
            {
                Selected = 0;
            }
            else
            {
                Selected = -1;
            }
        }

        if (Selected == 0)
        {
            if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Back))
            {
                if (Value.Length > 0 && CursorPosition > 0)
                {
                    Value.Remove(CursorPosition - 1, 1);
                    CursorPosition--;
                }
            }
            else if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Delete))
            {
                if (Value.Length > 0 && CursorPosition < Value.Length - 1)
                {
                    Value.Remove(CursorPosition, 1);
                }
            }
            else if (Value.Length < MaxLength)
            {
                char[] shiftedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ§'\"+!%/=()?:_-+".ToCharArray();
                char[] chars = "abcdefghijklmnopqrstuvwxyz0123456789,.-+".ToCharArray();
                char[] keys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,.-+".ToCharArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (ConsoleKeyboard.IsKeyDown(keys[i]))
                    {
                        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Shift))
                        {
                            Value.Insert(CursorPosition, shiftedChars[i]);
                        }
                        else
                        {
                            Value.Insert(CursorPosition, chars[i]);
                        }
                        CursorPosition++;
                    }
                }
            }

            if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Left))
            {
                CursorPosition = Math.Clamp(CursorPosition - 1, 0, Value.Length);
            }

            if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Right))
            {
                CursorPosition = Math.Clamp(CursorPosition + 1, 0, Value.Length);
            }
        }

        Game.Renderer.Text(borderRect.X, borderRect.Y, Value.ToString(), Selected == 0 ? CharColor.BrightCyan : CharColor.White);

        // TODO
        /*
        if (Selected == 0 && (int)(Time.Now * 2f) % 2 == 1)
        {
            if (CursorPosition < Value.Length)
            {
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Background = CharColor.White;
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.Black;
            }
            else
            {
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Char = '_';
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.White;
            }
        }
        else
        {
            if (CursorPosition < Value.Length)
            {
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Background = CharColor.Black;
            }
            else
            {
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Char = ' ';
                Game.Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.Black;
            }
        }
        */

        borderRect.Top += 2;

        if ((ConsoleKeyboard.IsKeyDown('W') || ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Up)) && Game.Instance.HandleInput)
        {
            Selected--;
            if (Selected < 0)
            { Selected = 2; }
            if (Selected > 2)
            { Selected = 0; }
        }

        if ((ConsoleKeyboard.IsKeyDown('S') || ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Down)) && Game.Instance.HandleInput)
        {
            Selected++;
            if (Selected < 0)
            { Selected = 2; }
            if (Selected > 2)
            { Selected = 0; }
        }

        if (ConsoleMouse.IsPressed(MouseButton.Left) &&
            borderRect.Contains(ConsoleMouse.RecordedConsolePosition) &&
            Game.Instance.HandleInput)
        {
            int i = ConsoleMouse.RecordedConsolePosition.Y;
            i -= borderRect.Y;

            if (i is >= 0 and <= 1)
            { Selected = i + 1; }
        }

        int clicked = -1;
        if (Selected != 0 && ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Return))
        { clicked = Selected; }

        const string LabelOk = "Ok";
        const string LabelCancel = "Cancel";

        {
            string label;
            byte color;
            if (clicked == 1)
            {
                label = $"> {LabelOk}";
                color = CharColor.BrightYellow;
            }
            else if (Selected == 1)
            {
                label = $"> {LabelOk}";
                color = CharColor.BrightCyan;
            }
            else
            {
                label = $"  {LabelOk}";
                color = CharColor.White;
            }

            Game.Renderer.Text(borderRect.X, borderRect.Y + 0, label, color);
        }

        {
            string label;
            byte color;
            if (clicked == 2)
            {
                label = $"> {LabelCancel}";
                color = CharColor.BrightYellow;
            }
            else if (Selected == 2)
            {
                label = $"> {LabelCancel}";
                color = CharColor.BrightCyan;
            }
            else
            {
                label = $"  {LabelCancel}";
                color = CharColor.White;
            }

            Game.Renderer.Text(borderRect.X, borderRect.Y + 1, label, color);
        }

        if (Selected != 0 && clicked != -1)
        {
            switch (clicked)
            {
                case 1:
                    OnOk.Invoke();
                    break;
                case 2:
                    OnCancel.Invoke();
                    break;
                default:
                    break;
            }
        }
    }

    public void Reset()
    {
        Value.Clear();
        Value.Append(InitialValue);
        CursorPosition = InitialValue.Length;
        Selected = 0;
    }
}
