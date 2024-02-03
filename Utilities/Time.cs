using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    public readonly struct Time
    {
        static ITimeProvider? _provider;
        [NotNull]
        public static ITimeProvider? Provider
        {
            get => _provider ?? throw new NullReferenceException($"{nameof(_provider)} is null");
            set => _provider = value;
        }

        public static float DeltaTime => Provider.DeltaTime;

        public static float LocalNow => (float)DateTime.Now.TimeOfDay.TotalSeconds;
        public static float Now => (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
    }
}
