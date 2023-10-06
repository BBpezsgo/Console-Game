namespace ConsoleGame
{
    public class MenuBoxed : Menu
    {
        readonly string Title;

        public MenuBoxed(ConsoleRenderer renderer, string title, params (string, Action)[] options)
            : base(renderer, options)
        {
            Title = title;
        }

        public void Tick(int width)
            => Tick(width, 2 + 2 + 1 + Options.Length);
        public void Tick(int width, int height)
        {
            RectInt borderRect = GUI.GetCenteredBox(width, height);

            GUI.Box(borderRect, ByteColor.Black, ByteColor.White, Ascii.BoxSides);

            if (!string.IsNullOrEmpty(Title))
            {
                int titleLabelX = borderRect.X + ((borderRect.Width / 2) - (Title.Length / 2));
                GUI.Label(titleLabelX, borderRect.Y, Title);
                Renderer[titleLabelX - 1, borderRect.Y].Char = ' ';
                Renderer[titleLabelX - 2, borderRect.Y].Char = '┤';
                Renderer[titleLabelX + Title.Length + 0, borderRect.Y].Char = ' ';
                Renderer[titleLabelX + Title.Length + 1, borderRect.Y].Char = '├';
            }

            base.Tick(borderRect.Expand(-3, -2, -2, -2));
            return;
        }
    }
}
