using Win32;
using static Win32.Utilities.Keyboard;

namespace ConsoleGame
{
    public static class Keyboard
    {
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
            System.Buffer.BlockCopy(Accumulated, 0, Stage1, 0, 8 * sizeof(int));
        }
    }
}
