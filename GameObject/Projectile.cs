namespace ConsoleGame
{
    public class Projectile : GameObject
    {
        public Projectile(Vector position) : base(position)
        {

        }

        public Projectile(Vector position, Vector speed) : base(position, speed)
        {

        }

        public override void Render()
        {
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = 0b_1110;
            Game.Renderer[Game.WorldToConsole(Position)].Char = '.';
        }

        public override void Tick()
        {
            Position += Speed * Game.DeltaTime;

            bool bounced = WorldBorders.Bounce(Game.Instance.Scene.Size, ref Position, ref Speed);
            if (bounced)
            {
                IsDestroyed = true;
                Speed = Vector.Zero;

                Game.Instance.Scene.AddObject(new Particles(Position, new ParticlesConfig()
                {
                    Gradients = new Gradient[]
                    {
                        Gradients.Fire,
                    },
                    ParticleCount = 70,
                    ParticleLifetime = (2f, 3f),
                    ParticleSpeed = (0f, 10f),
                }));

                Game.Instance.Scene.AddObject(new Particles(Position, new ParticlesConfig()
                {
                    Gradients = new Gradient[]
                    {
                        new Gradient(new Color(1f, 1f, 1f), new Color(1f, .8f, 0f)),
                    },
                    ParticleCount = 5,
                    ParticleLifetime = (.5f, 1f),
                    ParticleSpeed = (20, 30),
                }));

                // Game.Instance.Scene.AddObject(new ShockwaveEffect(Position, .5f, 15f));
            }
        }
    }
}
