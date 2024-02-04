namespace ConsoleGame
{
    public struct PredefinedEffects
    {
        public static ParticlesConfig Stuff => new()
        {
            Characters = Ascii.Stuff1,
            Gradients = new Gradient[]
            { new(ColorF.White, ColorF.Red) },
            ParticleCount = (1, 3),
            ParticleSpeed = (4, 7),
            ParticleLifetime = (0.2f, 0.6f),
        };

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
                new(new ColorF(1f, 1f, 1f), new ColorF(1f, .8f, 0f)),
            },
            ParticleCount = 5,
            ParticleLifetime = (.5f, 1f),
            ParticleSpeed = (20, 30),
        };

        public static ParticlesConfig MetalSparks => new()
        {
            Gradients = new Gradient[]
            {
                new(ColorF.White, ColorF.Yellow),
            },
            ParticleCount = 5,
            ParticleLifetime = .5f,
            ParticleSpeed = (0f, 10f),
            Characters = Ascii.ShadeShort2,
        };
        public static ParticlesConfig Shoot => new()
        {
            Gradients = new Gradient[]
            {
                new(ColorF.White, ColorF.Yellow),
            },
            ParticleCount = 2,
            ParticleLifetime = .3f,
            ParticleSpeed = (2f, 4f),
            Characters = Ascii.Stars,
            InDirection = .8f,
        };

        public static ParticlesConfig Death => new()
        {
            Gradients = new Gradient[]
            {
                new(ColorF.Red, ColorF.Red * .5f),
            },
            ParticleCount = 10,
            ParticleLifetime = 1,
            ParticleSpeed = (5, 10),
            Characters = Ascii.CircleFilled,
        };
    }
}
