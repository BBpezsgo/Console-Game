using System;

namespace ConsoleGame
{
    public partial class ConsoleRenderer
    {
        public void DrawLabel(VectorInt pos, string text)
            => DrawLabel(pos.X, pos.Y, text);
        public void DrawLabel(int x, int y, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int _x = x + i;

                if (_x >= width) break;
                if (y >= height) break;

                this[_x, y].Char = text[i];
            }
        }

        public void DrawLabel(VectorInt pos, string text, ushort attributes)
            => DrawLabel(pos.X, pos.Y, text, attributes);
        public void DrawLabel(int x, int y, string text, ushort attributes)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int _x = x + i;

                if (_x >= width) break;
                if (y >= height) break;

                this[_x, y].Char = text[i];
                this[_x, y].Attributes = attributes;
            }
        }

        public void DrawLabel(int x, int y, string text, byte background, byte foreground)
            => DrawLabel(x, y, text, unchecked((ushort)((background << 4) | foreground)));

        public void DrawBox(RectInt box)
            => DrawBox(box, Ascii.BoxSides);
        public void DrawBox(RectInt box, byte background, byte foreground)
            => DrawBox(box, unchecked((ushort)((background << 4) | foreground)));
        public void DrawBox(RectInt box, ushort attributes)
            => DrawBox(box, attributes, Ascii.BoxSides);
        public void DrawBox(RectInt box, char[] sideCharacters)
        {
            for (int _y = 0; _y < box.Height; _y++)
            {
                int actualY = box.Y + _y;
                if (actualY >= this.height) break;

                for (int _x = 0; _x < box.Width; _x++)
                {
                    int actualX = box.X + _x;

                    if (actualX >= this.width) break;

                    int size = 0b_0000;

                    if (_y == 0) size |= 0b_1000; // Top
                    if (_x == 0) size |= 0b_0100; // Left
                    if (_y == box.Height - 1) size |= 0b_0010; // Bottom
                    if (_x == box.Width - 1) size |= 0b_0001; // Right

                    char c = sideCharacters[size];

                    this[actualX, actualY].Char = c;
                }
            }
        }
        public void DrawBox(RectInt box, byte background, byte foreground, char[] sideCharacters)
            => DrawBox(box, unchecked((ushort)((background << 4) | foreground)), sideCharacters);
        public void DrawBox(RectInt box, ushort attributes, char[] sideCharacters)
        {
            for (int _y = 0; _y < box.Height; _y++)
            {
                int actualY = box.Y + _y;
                if (actualY >= this.height) break;

                for (int _x = 0; _x < box.Width; _x++)
                {
                    int actualX = box.X + _x;

                    if (actualX >= this.width) break;

                    int size = 0b_0000;

                    if (_y == 0) size |= 0b_1000; // Top
                    if (_x == 0) size |= 0b_0100; // Left
                    if (_y == box.Height - 1) size |= 0b_0010; // Bottom
                    if (_x == box.Width - 1) size |= 0b_0001; // Right

                    char c = sideCharacters[size];

                    this[actualX, actualY].Char = c;
                    this[actualX, actualY].Attributes = attributes;
                }
            }
        }

        public static ButtonState MakeButton(int x, int y, int width, int height)
        {
            if (Mouse.X >= x && Mouse.Y >= y && Mouse.X <= x + width && Mouse.Y <= y + height)
            {
                if (Mouse.IsLeftDown)
                {
                    return ButtonState.Click;
                }
                else
                {
                    return ButtonState.Hover;
                }
            }
            else
            {
                return ButtonState.None;
            }
        }
    }

    public enum ButtonState : byte
    {
        None = 0,
        Hover = 1,
        Click = 2,
    }
}
