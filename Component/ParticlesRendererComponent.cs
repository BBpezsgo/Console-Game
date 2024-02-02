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
        public float InDirection;
        public Vector Direction;

        public ParticlesConfig(ParticlesConfig other)
        {
            this.Gradients = other.Gradients;
            this.ParticleCount = other.ParticleCount;
            this.ParticleSpeed = other.ParticleSpeed;
            this.ParticleLifetime = other.ParticleLifetime;
            this.Characters = other.Characters;
            this.CharacterMode = other.CharacterMode;
            this.CharacterModeParam = other.CharacterModeParam;
            this.InDirection = other.InDirection;
            this.Direction = other.Direction;
        }
    }

    internal class ParticlesRendererComponent : RendererComponent
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

        public ParticlesRendererComponent(Entity entity, ParticlesConfig config) : base(entity)
        {
            particles = new Particle[config.ParticleCount.Random()];

            config.InDirection = Math.Clamp(config.InDirection, 0f, 1f);

            for (int i = 0; i < particles.Length; i++)
            {
                Vector dir;

                if (config.InDirection == 0f)
                { dir = Random.Direction(); }
                else if (config.InDirection == 1f)
                { dir = config.Direction; }
                else
                { dir = Vector.LinearLerp(Random.Direction(), config.Direction, config.InDirection); }

                dir.Normalize();

                particles[i] = new Particle(
                    (byte)Random.Integer(0, config.Gradients.Length),
                    Vector.Zero,
                    dir * config.ParticleSpeed.Random(),
                    config.ParticleLifetime.Random());
            }

            Gradients = config.Gradients;
            Characters = config.Characters ?? Ascii.ShadeLong;
            CharacterMode = config.CharacterMode;
            CharacterModeParam = config.CharacterModeParam;
        }

        public override void Update()
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

        public override void Render()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i].IsAlive) continue;

                Vector pos = particles[i].LocalPosition + Position;

                if (!Game.IsVisible(pos)) continue;
                VectorInt p = Game.WorldToConsole(pos);

                ref float depth = ref Game.DepthBuffer[p];

                if (depth > Priority) continue;
                depth = Priority;

                float v = particles[i].AgePercent;

                if (v >= 1f) return;

                int charIndex = CharacterMode switch
                {
                    ParticleCharacterMode.LifetimeRelative => (int)MathF.Round((1f - v) * (Characters.Length - 1)),
                    ParticleCharacterMode.TimeRelative => (int)MathF.Round(Time.UtcNow * CharacterModeParam) % Characters.Length,
                    ParticleCharacterMode.Random => Random.Integer(0, Characters.Length),
                    _ => throw new NotImplementedException(),
                };
                if (charIndex < 0 || charIndex >= Characters.Length) return;

                Game.Renderer[p] = new Win32.ConsoleChar(Characters[charIndex], (byte)Gradients[particles[i].Kind].Get(v));
            }
        }
    }
}
