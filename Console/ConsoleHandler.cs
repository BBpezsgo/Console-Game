using Win32;

namespace ConsoleGame
{
    public static class ConsoleHandler
    {
        static uint SavedMode;
        static IntPtr StdinHandle = Kernel32.INVALID_HANDLE_VALUE;

        /// <exception cref="WindowsException"/>
        public static void Setup()
        {
            StdinHandle = Kernel32.GetStdHandle(StdHandle.STD_INPUT_HANDLE);

            if (StdinHandle == Kernel32.INVALID_HANDLE_VALUE)
            { throw WindowsException.Get(); }

            uint mode = 0;
            if (Kernel32.GetConsoleMode(StdinHandle, ref mode) == 0)
            { throw WindowsException.Get(); }

            SavedMode = mode;

            mode &= ~InputMode.ENABLE_QUICK_EDIT_MODE;
            mode |= InputMode.ENABLE_WINDOW_INPUT;
            mode |= InputMode.ENABLE_MOUSE_INPUT;

            if (Kernel32.SetConsoleMode(StdinHandle, mode) == 0)
            { throw WindowsException.Get(); }

            Console.CursorVisible = false;
        }

        /// <exception cref="WindowsException"/>
        /// <exception cref="Exception"/>
        public static void Restore()
        {
            Console.CursorVisible = true;

            if (StdinHandle == Kernel32.INVALID_HANDLE_VALUE)
            { throw new Exception($"Invalid handle"); }

            if (Kernel32.SetConsoleMode(StdinHandle, SavedMode) == 0)
            { throw WindowsException.Get(); }
        }
    }
}
