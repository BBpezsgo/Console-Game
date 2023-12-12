using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    public readonly struct Time
    {
        static ITimeProvider? _provider;
        [NotNull]
        public static ITimeProvider? Provider
        {
            get
            {
                if (_provider is null) throw new NullReferenceException($"{nameof(_provider)} is null");
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }

        public static float DeltaTime => Provider.DeltaTime;

        public static float Now => (float)DateTime.Now.TimeOfDay.TotalSeconds;
        public static float UtcNow => (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
    }
}
