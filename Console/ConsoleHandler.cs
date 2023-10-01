using Win32;

namespace ConsoleGame
{
    public static class ConsoleHandler
    {
        public static void SetupConsole()
        {
            IntPtr inputHandle = Kernel32.GetStdHandle(Kernel32.STD_INPUT_HANDLE);
            uint mode = 0;

            if (Kernel32.GetConsoleMode(inputHandle, ref mode) == 0)
            { throw WindowsException.Get(); }

            InputMode.Default(ref mode);

            if (Kernel32.SetConsoleMode(inputHandle, mode) == 0)
            { throw WindowsException.Get(); }

            Console.CursorVisible = false;
        }
    }
}
