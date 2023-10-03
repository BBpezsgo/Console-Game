namespace ConsoleGame
{
    public enum ParticleCharacterMode
    {
        LifetimeRelative,
        TimeRelative,
        Random,
    }

    public struct ParticlesConfig
    {
        public Gradient[] Gradients;
        public RangeInt ParticleCount;
        public Range ParticleSpeed;
        public Range ParticleLifetime;
        public char[]? Characters;
        public ParticleCharacterMode CharacterMode;
        public float CharacterModeParam;
    }

    public class Particles : Effect
    {
        struct Particle
        {
            public readonly float Lifetime;
            public readonly byte Kind;
            public readonly Vector LocalSpeed;
            public readonly float BornAt;

            public Vector LocalPosition;
            public bool IsAlive;

            public readonly float Age => Time.UtcNow - BornAt;
            public readonly float AgePercent => Age / Lifetime;

            public Particle(byte kind, Vector localPosition, Vector localSpeed, float lifetime)
            {
                Kind = kind;
                LocalPosition = localPosition;
                LocalSpeed = localSpeed;
                IsAlive = true;
                Lifetime = lifetime;
                BornAt = Time.UtcNow;
            }

            public void Tick()
            {
                if (!this.IsAlive) return;

                this.LocalPosition += this.LocalSpeed * (1f - this.AgePercent) * Time.DeltaTime;

                if (this.Age >= this.Lifetime)
                { this.IsAlive = false; }
            }
        }

        readonly Particle[] particles;
        readonly Gradient[] Gradients;
        readonly char[] Characters;
        readonly ParticleCharacterMode CharacterMode;
        readonly float CharacterModeParam;

        public Particles(Vector position, ParticlesConfig config) : base()
        {
            Position = position;
            particles = new Particle[config.ParticleCount.Random()];

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle(
                    (byte)Random.Integer(0, config.Gradients.Length),
                    Vector.Zero,
                    Random.Direction() * config.ParticleSpeed.Random(),
                    config.ParticleLifetime.Random());
            }

            Gradients = config.Gradients;
            Characters = config.Characters ?? Ascii.ShadeLong;
            CharacterMode = config.CharacterMode;
            CharacterModeParam = config.CharacterModeParam;
        }

        public override void Render()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i].IsAlive) continue;
                Vector pos = particles[i].LocalPosition + Position;
                if (!Game.Instance.Scene.Size.Contains(pos)) continue;
                ref Win32.CharInfo pixel = ref Game.Renderer[Game.WorldToConsole(pos)];
                float v = particles[i].AgePercent;

                Color color = Gradients[particles[i].Kind].Get(v);

                pixel.Foreground = (byte)color;

                int charIndex = CharacterMode switch
                {
                    ParticleCharacterMode.LifetimeRelative => (int)MathF.Round((1f - v) * (Characters.Length - 1)),
                    ParticleCharacterMode.TimeRelative => (int)MathF.Round(Time.UtcNow * CharacterModeParam) % Characters.Length,
                    ParticleCharacterMode.Random => Random.Integer(0, Characters.Length),
                    _ => throw new NotImplementedException(),
                };
                pixel.Char = Characters[charIndex];
            }
        }

        public override void Tick()
        {
            bool shouldDestroy = true;

            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i].IsAlive) continue;

                particles[i].Tick();
                shouldDestroy = false;
            }

            if (shouldDestroy)
            { this.IsDestroyed = true; }
        }
    }
}
