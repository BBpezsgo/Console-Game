using Win32;

namespace ConsoleGame
{
    public readonly struct Time
    {
        public static float DeltaTime => Game.DeltaTime;

        public static float Now => (float)DateTime.Now.TimeOfDay.TotalSeconds;
        public static float UtcNow => (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;

        unsafe public static float UtcNowKernel32
        {
            get
            {
                SYSTEMTIME time = default;
                Kernel32.GetSystemTime(&time);
                return (float)time.Hour + (float)time.Minute + (float)time.Second + ((float)time.Milliseconds * 0.001f);
            }
        }
    }
}
