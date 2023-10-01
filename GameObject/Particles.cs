namespace ConsoleGame
{
    public struct ParticlesConfig
    {
        public Gradient[] Gradients;
        public RangeInt ParticleCount;
        public Range ParticleSpeed;
        public Range ParticleLifetime;
    }

    public class Particles : Effect
    {
        struct Particle
        {
            public readonly float Lifetime;
            public readonly byte Kind;
            public readonly Vector LocalSpeed;

            public Vector LocalPosition;
            public bool IsAlive;
            public float Age;

            public readonly float AgePercent => Age / Lifetime;

            public Particle(byte kind, Vector localPosition, Vector localSpeed, float lifetime)
            {
                Kind = kind;
                LocalPosition = localPosition;
                LocalSpeed = localSpeed;
                IsAlive = true;
                Lifetime = lifetime;
                Age = 0f;
            }

            public void Tick()
            {
                if (!this.IsAlive) return;

                this.Age += Game.DeltaTime;
                this.LocalPosition += this.LocalSpeed * (1f - this.AgePercent) * Game.DeltaTime;

                if (this.Age >= this.Lifetime)
                { this.IsAlive = false; }
            }
        }

        readonly Particle[] particles;
        readonly Gradient[] Gradients;
        readonly char[] Characters;

        public Particles(Vector position, ParticlesConfig config) : base(position)
        {
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
            Characters = Ascii.ShadeLong;
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

                pixel.Char = Characters[(int)MathF.Floor((1f - v) * Characters.Length)];
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
