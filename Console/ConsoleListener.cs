using Win32;

namespace ConsoleGame
{
    public delegate void ConsoleEvent<T>(T e);

    public static class ConsoleListener
    {
        public static event ConsoleEvent<MouseEvent>? MouseEvent;
        public static event ConsoleEvent<KeyEvent>? KeyEvent;
        public static event ConsoleEvent<WindowBufferSizeEvent>? WindowBufferSizeEvent;

        static bool Run;
        static IntPtr Handle;

        /// <exception cref="WindowsException"/>
        public static void Start()
        {
            if (Run) return;

            Run = true;
            Handle = Kernel32.GetStdHandle(StdHandle.STD_INPUT_HANDLE);

            if (Handle == Kernel32.INVALID_HANDLE_VALUE)
            { throw WindowsException.Get(); }

            new Thread(ThreadJob).Start();
        }

        /// <exception cref="WindowsException"/>
        static void ThreadJob()
        {
            InputEvent[] record = new InputEvent[1];

            while (Run)
            {
                uint numRead = 0;

                record[0] = new InputEvent();

                if (Kernel32.ReadConsoleInput(Handle, record, (uint)record.Length, ref numRead) == 0)
                { throw WindowsException.Get(); }

                if (!Run) break;

                switch (record[0].EventType)
                {
                    case EventType.MOUSE:
                        MouseEvent?.Invoke(record[0].MouseEvent);
                        break;
                    case EventType.KEY:
                        KeyEvent?.Invoke(record[0].KeyEvent);
                        break;
                    case EventType.WINDOW_BUFFER_SIZE:
                        WindowBufferSizeEvent?.Invoke(record[0].WindowBufferSizeEvent);
                        break;
                }
            }

            {
                uint numWritten = 0;
                _ = Kernel32.WriteConsoleInput(Handle, record, 1, ref numWritten);
                Console.CursorVisible = true;
            }
        }

        public static void Stop() => Run = false;
    }
}
