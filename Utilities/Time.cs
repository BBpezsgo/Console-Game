using Win32;

namespace ConsoleGame
{
    public readonly struct Time
    {
        public static float DeltaTime => Game.DeltaTime;

        public static float Now => (float)DateTime.Now.TimeOfDay.TotalSeconds;
        public static float UtcNow => (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
    }
}
