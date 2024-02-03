using System.Text;
using Win32;
using Win32.Common;
using Win32.LowLevel;

namespace ConsoleGame
{
    public class InputBox
    {
        readonly Renderer<ConsoleChar> Renderer;
        readonly string Title;
        readonly Action OnOk;
        readonly Action OnCancel;
        readonly int MaxLength;
        readonly string InitialValue;

        int CursorPosition;
        int Selected;

        public StringBuilder Value;

        public InputBox(Renderer<ConsoleChar> renderer, string title, string initialValue, int maxLength, Action onOk, Action onCancel)
        {
            Renderer = renderer;
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

            GUI.Box(borderRect, CharColor.Black, CharColor.White, SideCharacters.BoxSides);

            if (!string.IsNullOrEmpty(Title))
            {
                int titleLabelX = borderRect.X + ((borderRect.Width / 2) - (Title.Length / 2));
                GUI.Label(titleLabelX, borderRect.Y, Title);
                Renderer[titleLabelX - 1, borderRect.Y].Char = ' ';
                Renderer[titleLabelX - 2, borderRect.Y].Char = '┤';
                Renderer[titleLabelX + Title.Length + 0, borderRect.Y].Char = ' ';
                Renderer[titleLabelX + Title.Length + 1, borderRect.Y].Char = '├';
            }

            borderRect.Expand(-2, -2, -2, -2);

            if (Mouse.IsPressed(MouseButton.Left))
            {
                if (Mouse.RecordedConsolePosition.Y == borderRect.Y && borderRect.Contains(Mouse.RecordedConsolePosition))
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
                if (Keyboard.IsKeyDown(VirtualKeyCode.BACK))
                {
                    if (Value.Length > 0 && CursorPosition > 0)
                    {
                        Value.Remove(CursorPosition - 1, 1);
                        CursorPosition--;
                    }
                }
                else if (Keyboard.IsKeyDown(VirtualKeyCode.DELETE))
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
                        if (Keyboard.IsKeyDown(keys[i]))
                        {
                            if (Keyboard.IsKeyPressed(VirtualKeyCode.SHIFT))
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

                if (Keyboard.IsKeyDown(VirtualKeyCode.LEFT))
                {
                    CursorPosition = Math.Clamp(CursorPosition - 1, 0, Value.Length);
                }

                if (Keyboard.IsKeyDown(VirtualKeyCode.RIGHT))
                {
                    CursorPosition = Math.Clamp(CursorPosition + 1, 0, Value.Length);
                }
            }

            GUI.Label(borderRect.X, borderRect.Y, Value.ToString(), CharColor.Black, Selected == 0 ? CharColor.BrightCyan : CharColor.White);

            if (Selected == 0 && (int)(Time.Now * 2f) % 2 == 1)
            {
                if (CursorPosition < Value.Length)
                {
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Background = CharColor.White;
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.Black;
                }
                else
                {
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Char = '_';
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.White;
                }
            }
            else
            {
                if (CursorPosition < Value.Length)
                {
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Background = CharColor.Black;
                }
                else
                {
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Char = ' ';
                    Renderer[borderRect.X + CursorPosition, borderRect.Y].Foreground = CharColor.Black;
                }
            }

            borderRect.Top += 2;

            if (Keyboard.IsKeyDown('W') || Keyboard.IsKeyDown(VirtualKeyCode.UP))
            {
                Selected--;
                if (Selected < 0)
                { Selected = 2; }
                if (Selected > 2)
                { Selected = 0; }
            }

            if (Keyboard.IsKeyDown('S') || Keyboard.IsKeyDown(VirtualKeyCode.DOWN))
            {
                Selected++;
                if (Selected < 0)
                { Selected = 2; }
                if (Selected > 2)
                { Selected = 0; }
            }

            if (Mouse.IsPressed(MouseButton.Left) && borderRect.Contains(Mouse.RecordedConsolePosition))
            {
                int i = Mouse.RecordedConsolePosition.Y;
                i -= borderRect.Y;

                if (i is >= 0 and <= 1)
                { Selected = i + 1; }
            }

            int clicked = -1;
            if (Selected != 0 && Keyboard.IsKeyDown(VirtualKeyCode.RETURN))
            { clicked = Selected; }

            const string LabelOk = "Ok";
            const string LabelCancel = "Cancel";

            {
                string label;
                byte color;
                if (1 == clicked)
                {
                    label = $"> {LabelOk}";
                    color = CharColor.BrightYellow;
                }
                else if (1 == Selected)
                {
                    label = $"> {LabelOk}";
                    color = CharColor.BrightCyan;
                }
                else
                {
                    label = $"  {LabelOk}";
                    color = CharColor.White;
                }

                GUI.Label(borderRect.X, borderRect.Y + 0, label, CharColor.Black, color);
            }

            {
                string label;
                byte color;
                if (2 == clicked)
                {
                    label = $"> {LabelCancel}";
                    color = CharColor.BrightYellow;
                }
                else if (2 == Selected)
                {
                    label = $"> {LabelCancel}";
                    color = CharColor.BrightCyan;
                }
                else
                {
                    label = $"  {LabelCancel}";
                    color = CharColor.White;
                }

                GUI.Label(borderRect.X, borderRect.Y + 1, label, CharColor.Black, color);
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
}
