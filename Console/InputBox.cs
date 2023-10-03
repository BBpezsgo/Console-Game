using Win32;

namespace ConsoleGame
{
    public class InputBox
    {
        readonly ConsoleRenderer Renderer;
        readonly string Title;
        string Value;
        bool IsFocused;

        public InputBox(ConsoleRenderer renderer, string title, string initialValue)
        {
            Renderer = renderer;
            Title = title;
            Value = initialValue;
        }

        public void Tick(int width)
            => Tick(width, 5);
        public void Tick(int width, int height)
        {
            RectInt borderRect = Renderer.MakeMenu(width, height);

            Renderer.DrawBox(borderRect, ByteColor.Black, ByteColor.White, Ascii.BoxSides);

            if (!string.IsNullOrEmpty(Title))
            {
                int titleLabelX = borderRect.X + ((borderRect.Width / 2) - (Title.Length / 2));
                Renderer.DrawLabel(titleLabelX, borderRect.Y, Title);
                Renderer[titleLabelX - 1, borderRect.Y].Char = ' ';
                Renderer[titleLabelX - 2, borderRect.Y].Char = '┤';
                Renderer[titleLabelX + Title.Length + 0, borderRect.Y].Char = ' ';
                Renderer[titleLabelX + Title.Length + 1, borderRect.Y].Char = '├';
            }

            borderRect.Expand(-2, -2, -2, -2);

            if (Mouse.IsLeftDown) {
                if (borderRect.Contains(Mouse.X, Mouse.Y)) {
                    IsFocused = true;
                } else {
                    IsFocused = false;
                }
            }

            if (IsFocused)
            {
                if (Keyboard.IsKeyDown(VirtualKeyCodes.BACK)) {
                    if (Value.Length > 0) {
                        Value = Value[..^1];
                    }
                } else if (Keyboard.IsKeyDown(VirtualKeyCodes.OEM_PERIOD)) {

                } else {
                    char[] shiftedChars =   "ABCDEFGHIJKLMNOPQRSTUVWXYZ§'\"+!%/=()?:_-+".ToCharArray();
                    char[] chars =          "abcdefghijklmnopqrstuvwxyz0123456789,.-+".ToCharArray();
                    char[] keys =           "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,.-+".ToCharArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (Keyboard.IsKeyDown(keys[i])) {
                            if (Keyboard.IsKeyPressed(VirtualKeyCodes.SHIFT)) {
                                Value += shiftedChars[i].ToString();
                            } else {
                                Value += chars[i].ToString();
                            }
                        } else if (Keyboard.IsKeyDown(190)) {
                            if (Keyboard.IsKeyPressed(VirtualKeyCodes.SHIFT)) {
                                Value += ':';
                            } else {
                                Value += '.';
                            }
                        }
                    }
                }
            }

            Renderer.DrawLabel(borderRect.X, borderRect.Y, Value, ByteColor.Black, IsFocused ? ByteColor.BrightCyan : ByteColor.White);
        }
    }
}
