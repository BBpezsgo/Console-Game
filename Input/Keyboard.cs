using Win32;

namespace ConsoleGame
{
    public static class Keyboard
    {
        static readonly Dictionary<char, ushort> AsciiToKey = new()
        {
            { '0', 0x30 },
            { '1', 0x31 },
            { '2', 0x32 },
            { '3', 0x33 },
            { '4', 0x34 },
            { '5', 0x35 },
            { '6', 0x36 },
            { '7', 0x37 },
            { '8', 0x38 },
            { '9', 0x39 },
            { 'A', 0x41 },
            { 'B', 0x42 },
            { 'C', 0x43 },
            { 'D', 0x44 },
            { 'E', 0x45 },
            { 'F', 0x46 },
            { 'G', 0x47 },
            { 'H', 0x48 },
            { 'I', 0x49 },
            { 'J', 0x4A },
            { 'K', 0x4B },
            { 'L', 0x4C },
            { 'M', 0x4D },
            { 'N', 0x4E },
            { 'O', 0x4F },
            { 'P', 0x50 },
            { 'Q', 0x51 },
            { 'R', 0x52 },
            { 'S', 0x53 },
            { 'T', 0x54 },
            { 'U', 0x55 },
            { 'V', 0x56 },
            { 'W', 0x57 },
            { 'X', 0x58 },
            { 'Y', 0x59 },
            { 'Z', 0x5A },

            { '.', VirtualKeyCodes.OEM_PERIOD },
            { ',', VirtualKeyCodes.OEM_COMMA },
            { '-', VirtualKeyCodes.OEM_MINUS },
            { '+', VirtualKeyCodes.OEM_PLUS },
            { '\r', VirtualKeyCodes.RETURN },
            { '\t', VirtualKeyCodes.TAB },
        };

        public static int[] Accumulated = new int[8];

        public static int[] Stage1 = new int[8];
        public static int[] Stage2 = new int[8];
        public static int[] Stage3 = new int[8];

        public static bool IsKeyPressed(char key) => IsKeyPressed(AsciiToKey[key]);
        public static bool IsKeyPressed(ushort key) => BitUtils.GetBit(Accumulated, key) || BitUtils.GetBit(Stage1, key) || BitUtils.GetBit(Stage2, key);

        public static bool IsKeyHold(char key) => IsKeyHold(AsciiToKey[key]);
        public static bool IsKeyHold(ushort key)
        {
            bool stage1 = BitUtils.GetBit(Stage1, key);
            bool stage2 = BitUtils.GetBit(Stage2, key);
            bool stage3 = BitUtils.GetBit(Stage3, key);
            return stage2;
        }

        public static bool IsKeyDown(char key) => IsKeyDown(AsciiToKey[key]);
        public static bool IsKeyDown(ushort key)
        {
            bool stage1 = BitUtils.GetBit(Stage1, key);
            bool stage2 = BitUtils.GetBit(Stage2, key);
            bool stage3 = BitUtils.GetBit(Stage3, key);
            return stage1 && !stage2 && !stage3;
        }

        public static bool IsKeyUp(char key) => IsKeyUp(AsciiToKey[key]);
        public static bool IsKeyUp(ushort key)
        {
            bool stage1 = BitUtils.GetBit(Stage1, key);
            bool stage2 = BitUtils.GetBit(Stage2, key);
            bool stage3 = BitUtils.GetBit(Stage3, key);
            return !stage1 && !stage2 && stage3;
        }

        public static void Feed(KeyEvent e)
        {
            BitUtils.SetBit(Accumulated, e.VirtualKeyCode, e.IsDown != 0);
        }

        public static void BeginTick()
        {
            int[] savedStage3 = Stage3;
            Stage3 = Stage2;
            Stage2 = Stage1;
            Stage1 = savedStage3;
            Buffer.BlockCopy(Accumulated, 0, Stage1, 0, 8 * sizeof(int));
        }
    }
}
