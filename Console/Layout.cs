namespace ConsoleGame
{
    public struct Layout
    {
        public readonly RectInt TotalSize;
        int x = 0;
        int y = 0;

        public Layout(RectInt totalSize)
        {
            TotalSize = totalSize;
        }

        public void BreakLine()
        { if (x != 0) ForceBreakLine(); }

        public void ForceBreakLine()
        {
            x = 0;
            y++;
        }

        public void BreakIfOverflow()
        { if (x >= TotalSize.Width) ForceBreakLine(); }

        public readonly RectInt CurrentLine => new(TotalSize.X, CurrentY, TotalSize.Width, 1);
        public readonly int CurrentX => TotalSize.X + x;
        public readonly int CurrentY => TotalSize.Y + y;

        public RectInt Block(int height)
        {
            BreakLine();
            RectInt result = new(CurrentX, CurrentY, TotalSize.Width, height);
            y += height;
            return result;
        }

        public RectInt Inline(int width)
        {
            width = Math.Min(width, TotalSize.Width);
            RectInt result = new(CurrentX, CurrentY, width, 1);
            x += width;
            return result;
        }

        public RectInt AlignBlock(int width, int height, Align align)
        {
            BreakLine();
            width = Math.Min(width, TotalSize.Width);
            return align switch
            {
                Align.Left => new RectInt(TotalSize.X, CurrentY, width, height),
                Align.Center => new RectInt(TotalSize.X + (TotalSize.Width / 2) - (width / 2), CurrentY, width, height),
                Align.Right => new RectInt(TotalSize.Right - width, CurrentY, width, height),
                _ => default,
            };
        }

        public enum Align
        {
            Left,
            Center,
            Right,
        }
    }
}
