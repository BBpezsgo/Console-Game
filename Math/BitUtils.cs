using Win32;

namespace ConsoleGame
{
    public struct BitUtils
    {
        public static bool GetBit(int segment, int bit)
        {
            return (segment & (1 << bit)) != 0;
        }

        public static void SetBit(ref int segment, int bit, int value)
            => SetBit(ref segment, bit, value != 0);
        public static void SetBit(ref int segment, int bit, bool value)
        {
            segment &= ~(1 << bit);
            if (value)
            { segment |= 1 << bit; }
        }

        public static bool GetBit(int[] segments, int bit)
        {
            int i = bit / 32;
            int _bit = bit % 32;

            int segment = segments[i];
            segment >>= _bit;
            segment &= 1;

            return segment != 0;
        }

        public static void SetBit(int[] segments, int bit, int value)
            => SetBit(segments, bit, value != 0);
        public static void SetBit(int[] segments, int bit, bool value)
        {
            int i = bit / 32;
            int _bit = bit % 32;

            int segment = segments[i];
            segment &= ~(1 << _bit);
            if (value)
            { segment |= 1 << _bit; }
            segments[i] = segment;
        }

        public static void RenderBits(ConsoleRenderer renderer, Vector2Int position, int[] segments)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                RenderBits(renderer, new Vector2Int(position.X + i * 32, position.Y), segments[i]);
            }
        }
        public static void RenderBits(ConsoleRenderer renderer, Vector2Int position, int segment)
        {
            for (int i = 0; i < 32; i++)
            {
                bool isSet = (segment & (1 << i)) != 0;
                renderer[position.Y * renderer.Width + position.X + i].Char = isSet ? '1' : '0';
                renderer[position.Y * renderer.Width + position.X + i].Background = CharColor.Black;
                renderer[position.Y * renderer.Width + position.X + i].Foreground = isSet ? CharColor.BrightBlue : CharColor.Blue;
            }
        }
    }
}
