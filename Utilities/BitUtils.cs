namespace ConsoleGame
{
    public struct BitUtils
    {
        public static bool GetBit(int segment, int bit) {
            return (segment & (1 << bit)) != 0;
        }
        
        public static void SetBit(ref int segment, int bit, bool value) {
            segment &= ~(1 << bit);
            if (value)
            { segment |= 1 << bit; }
        }
        
        public static bool GetBit(int[] segments, int bit) {
            int i = bit / 32;
            int _bit = bit % 32;

            int segment = segments[i];
            segment >>= _bit;
            segment &= 1;

            return segment != 0;
        }
        
        public static void SetBit(int[] segments, int bit, bool value) {
            int i = bit / 32;
            int _bit = bit % 32;

            int segment = segments[i];
            segment &= ~(1 << _bit);
            if (value)
            { segment |= 1 << _bit; }

            segments[i] = segment;
        }
    }
}