using Win32;

namespace ConsoleGame
{
    public static class ConsoleListener
    {
        public static event ConsoleEvent<MouseEvent>? MouseEvent;
        public static event ConsoleEvent<KeyEvent>? KeyEvent;
        public static event ConsoleEvent<WindowBufferSizeEvent>? WindowBufferSizeEvent;

        static bool Run;
        static IntPtr Handle;

        public static void Start()
        {
            if (Run) return;
            Run = true;
            Handle = Kernel32.GetStdHandle(Kernel32.STD_INPUT_HANDLE);
            new Thread(ThreadJob).Start();
        }

        static void ThreadJob()
        {
            while (true)
            {
                uint numRead = 0;
                InputEvent[] record = new InputEvent[1];
                record[0] = new InputEvent();
                _ = Kernel32.ReadConsoleInput(Handle, record, 1, ref numRead);
                if (Run)
                {
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
                else
                {
                    uint numWritten = 0;
                    _ = Kernel32.WriteConsoleInput(Handle, record, 1, ref numWritten);
                    Console.CursorVisible = true;
                    return;
                }
            }
        }

        public static void Stop() => Run = false;
    }

    public delegate void ConsoleEvent<T>(T e);
}
