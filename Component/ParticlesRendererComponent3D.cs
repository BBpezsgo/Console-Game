using System.Numerics;

namespace ConsoleGame
{
    internal class ParticlesRendererComponent3D : PostRendererComponent3D
    {
        struct Particle
        {
            public readonly float Lifetime;
            public readonly byte Kind;
            public readonly Vector3 LocalSpeed;
            public readonly float BornAt;

            public Vector3 LocalPosition;
            public bool IsAlive;

            public readonly float Age => Time.Now - BornAt;
            public readonly float AgePercent => Age / Lifetime;

            public Particle(byte kind, Vector3 localPosition, Vector3 localSpeed, float lifetime)
            {
                Kind = kind;
                LocalPosition = localPosition;
                LocalSpeed = localSpeed;
                IsAlive = true;
                Lifetime = lifetime;
                BornAt = Time.Now;
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

        public ParticlesRendererComponent3D(Entity entity, ParticlesConfig config) : base(entity)
        {
            particles = new Particle[config.ParticleCount.Random()];

            config.InDirection = Math.Clamp(config.InDirection, 0f, 1f);

            for (int i = 0; i < particles.Length; i++)
            {
                Vector3 dir;

                if (config.InDirection == 0f)
                { dir = Random.Direction3(); }
                else if (config.InDirection == 1f)
                { dir = new Vector3(config.Direction.X, 0f, config.Direction.Y); }
                else
                { dir = Vector.LinearLerp(Random.Direction3(), new Vector3(config.Direction.X, 0f, config.Direction.Y), config.InDirection); }

                dir = Vector3.Normalize(dir);

                particles[i] = new Particle(
                    (byte)Random.Integer(0, config.Gradients.Length),
                    Vector3.Zero,
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

                Vector3 pos = particles[i].LocalPosition + new Vector3(Position.X * 2f, 0f, Position.Y * 2f);

                float v = particles[i].AgePercent;

                if (v >= 1f) return;

                Vector2Int p = Renderer3D.DoMathWithThis(Game.Renderer, pos, Game.Instance.Scene.Camera, out float depth);

                if (!Game.Renderer.IsVisible(p)) return;

                if (Game.Renderer is IRendererWithDepth depth_)
                {
                    if (depth_.DepthBuffer[p] > depth)
                    { return; }
                    depth_.DepthBuffer[p] = depth;
                }

                int charIndex = CharacterMode switch
                {
                    ParticleCharacterMode.LifetimeRelative => (int)MathF.Round((1f - v) * (Characters.Length - 1)),
                    ParticleCharacterMode.TimeRelative => (int)MathF.Round(Time.Now * CharacterModeParam) % Characters.Length,
                    ParticleCharacterMode.Random => Random.Integer(0, Characters.Length),
                    _ => throw new NotImplementedException(),
                };
                if (charIndex < 0 || charIndex >= Characters.Length) return;

                Game.Renderer[p] = new Win32.ConsoleChar(Characters[charIndex], (byte)Gradients[particles[i].Kind].Get(v));
            }
        }
    }
}
