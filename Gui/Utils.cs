using System.Diagnostics.CodeAnalysis;
using Win32;
using Win32.Common;

namespace ConsoleGame
{
    public static class GUI
    {
        static Renderer<ConsoleChar>? _renderer;
        [NotNull]
        public static Renderer<ConsoleChar>? Renderer
        {
            get => _renderer ?? throw new NullReferenceException($"{nameof(_renderer)} is null");
            set => _renderer = value;
        }
        static short Width => Renderer.Width;
        static short Height => Renderer.Height;

        #region Label

        public static int Label(int x, int y, string text)
        {
            Renderer.Text(x, y, text);
            return text.Length;
        }

        public static int Label(VectorInt pos, string text, ushort attributes) => Label(pos.X, pos.Y, text, attributes);
        public static int Label(int x, int y, string text, ushort attributes)
        {
            Renderer.Text(x, y, text, attributes);
            return text.Length;
        }

        public static int Label(VectorInt pos, string text, byte background, byte foreground) => Label(pos.X, pos.Y, text, CharColor.Make(background, foreground));
        public static int Label(int x, int y, string text, byte background, byte foreground) => Label(x, y, text, CharColor.Make(background, foreground));

        #endregion

        #region Box

        public static void Box(RectInt box) => Box(box, SideCharacters.BoxSides);
        public static void Box(RectInt box, byte background, byte foreground) => Box(box, CharColor.Make(background, foreground));
        public static void Box(RectInt box, ushort attributes) => Box(box, attributes, SideCharacters.BoxSides);

        public static void Box(RectInt box, in SideCharacters<char> sideCharacters) => Renderer.Box(new SmallRect(box.X, box.Y, box.Width, box.Height), CharColor.Black, CharColor.Silver, in sideCharacters);
        public static void Box(RectInt box, byte background, byte foreground, in SideCharacters<char> sideCharacters) => Box(box, CharColor.Make(background, foreground), in sideCharacters);
        public static void Box(RectInt box, ushort attributes, in SideCharacters<char> sideCharacters) => Renderer.Box(new SmallRect(box.X, box.Y, box.Width, box.Height), attributes, in sideCharacters);

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
