namespace ConsoleGame
{
    public struct PredefinedEffects
    {
        public static ParticlesConfig LargeExplosion => new()
        {
            Gradients = new Gradient[]
            {
                Gradients.Fire,
            },
            ParticleCount = 70,
            ParticleLifetime = (2f, 3f),
            ParticleSpeed = (0f, 10f),
        };

        public static ParticlesConfig SmallExplosion => new()
        {
            Gradients = new Gradient[]
            {
                Gradients.Fire,
            },
            ParticleCount = 30,
            ParticleLifetime = (.5f, 1.5f),
            ParticleSpeed = (0f, 5f),
        };

        public static ParticlesConfig ExplosionTrailStuff => new()
        {
            Gradients = new Gradient[]
            {
                new Gradient(new Color(1f, 1f, 1f), new Color(1f, .8f, 0f)),
            },
            ParticleCount = 5,
            ParticleLifetime = (.5f, 1f),
            ParticleSpeed = (20, 30),
        };

        public static ParticlesConfig MetalSparks => new()
        {
            Gradients = new Gradient[]
            {
                new Gradient(Color.White, Color.Yellow),
            },
            ParticleCount = 5,
            ParticleLifetime = .5f,
            ParticleSpeed = (0f, 10f),
            Characters = Ascii.ShadeShort2,
        };

        public static ParticlesConfig Death => new()
        {
            Gradients = new Gradient[]
            {
                new Gradient(Color.Red, Color.Red * .5f),
            },
            ParticleCount = 10,
            ParticleLifetime = 1,
            ParticleSpeed = (5, 10),
            Characters = Ascii.CircleFilled,
        };
    }
}
