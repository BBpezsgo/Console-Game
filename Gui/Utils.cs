namespace ConsoleGame
{
    public static class GUI
    {
        static ConsoleRenderer Renderer => Game.Renderer;
        static short Width => Game.Renderer.Width;
        static short Height => Game.Renderer.Height;

        #region Label

        public static int Label(VectorInt pos, string text) => Label(pos.X, pos.Y, text);
        public static int Label(int x, int y, string text)
        {
            int w = 0;
            for (int i = 0; i < text.Length; i++)
            {
                int _x = x + i;

                if (_x >= Width) break;
                if (y >= Height) break;

                Renderer[_x, y].Char = text[i];
                w++;
            }
            return w;
        }

        public static int Label(VectorInt pos, string text, ushort attributes) => Label(pos.X, pos.Y, text, attributes);
        public static int Label(int x, int y, string text, ushort attributes)
        {
            int w = 0;
            for (int i = 0; i < text.Length; i++)
            {
                int _x = x + i;

                if (_x >= Width) break;
                if (y >= Height) break;

                Renderer[_x, y].Char = text[i];
                Renderer[_x, y].Attributes = attributes;
                w++;
            }
            return w;
        }

        public static int Label(VectorInt pos, string text, byte background, byte foreground) => Label(pos.X, pos.Y, text, ConsoleRenderer.MakeAttributes(background, foreground));
        public static int Label(int x, int y, string text, byte background, byte foreground) => Label(x, y, text, ConsoleRenderer.MakeAttributes(background, foreground));

        #endregion

        #region Box

        public static void Box(RectInt box) => Box(box, Ascii.BoxSides);
        public static void Box(RectInt box, byte background, byte foreground) => Box(box, ConsoleRenderer.MakeAttributes(background, foreground));
        public static void Box(RectInt box, ushort attributes) => Box(box, attributes, Ascii.BoxSides);
        
        public static void Box(RectInt box, char[] sideCharacters)
        {
            for (int _y = 0; _y < box.Height; _y++)
            {
                int actualY = box.Y + _y;
                if (actualY >= Height) break;

                for (int _x = 0; _x < box.Width; _x++)
                {
                    int actualX = box.X + _x;

                    if (actualX >= Width) break;

                    int size = 0b_0000;

                    if (_y == 0) size |= 0b_1000; // Top
                    if (_x == 0) size |= 0b_0100; // Left
                    if (_y == box.Height - 1) size |= 0b_0010; // Bottom
                    if (_x == box.Width - 1) size |= 0b_0001; // Right

                    char c = sideCharacters[size];

                    Renderer[actualX, actualY].Char = c;
                }
            }
        }
        public static void Box(RectInt box, byte background, byte foreground, char[] sideCharacters) => Box(box, ConsoleRenderer.MakeAttributes(background, foreground), sideCharacters);
        public static void Box(RectInt box, ushort attributes, char[] sideCharacters)
        {
            for (int _y = 0; _y < box.Height; _y++)
            {
                int actualY = box.Y + _y;
                if (actualY >= Height) break;

                for (int _x = 0; _x < box.Width; _x++)
                {
                    int actualX = box.X + _x;

                    if (actualX >= Width) break;

                    int size = 0b_0000;

                    if (_y == 0) size |= 0b_1000; // Top
                    if (_x == 0) size |= 0b_0100; // Left
                    if (_y == box.Height - 1) size |= 0b_0010; // Bottom
                    if (_x == box.Width - 1) size |= 0b_0001; // Right

                    char c = sideCharacters[size];

                    Renderer[actualX, actualY].Char = c;
                    Renderer[actualX, actualY].Attributes = attributes;
                }
            }
        }

        #endregion

        public static RectInt GetCenteredBox(int width, int height) => new(
            (Width / 2) - (width / 2),
            (Height / 2) - (height / 2),
            width,
            height
        );
        public static RectInt GetCenteredBox(VectorInt size) => new(
            (Width / 2) - (size.X / 2),
            (Height / 2) - (size.Y / 2),
            size.X,
            size.Y
        );
    }
}
