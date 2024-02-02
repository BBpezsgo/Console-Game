using Win32;

namespace ConsoleGame
{
    public class ShockwaveRendererComponent : RendererComponent
    {
        public float Radius;
        public float Lifetime;
        public readonly float BornTime;

        public ShockwaveRendererComponent(Entity entity) : base(entity)
        {
            BornTime = Time.UtcNow;
        }

        public override void Render()
        {
            float life = (Time.UtcNow - BornTime) / Lifetime;
            if (life >= 1f)
            {
                Entity.IsDestroyed = true;
                return;
            }

            float radius = Radius * MathF.Sqrt(life);
            int points = (int)radius * 8;

            for (int i = 0; i < points; i++)
            {
                Vector direction = Vector.FromRad((float)i / (float)points * MathF.PI * 2f);

                Vector p = (direction * radius) + Position;

                p += direction * (Noise.Simplex(p.X, p.Y) * .5f + .5f) * 1.5f;

                if (!Game.IsVisible(p)) continue;

                VectorInt conPos = Game.WorldToConsole(p);

                if (Game.IsOnGui(conPos)) continue;

                ref float depth = ref Game.DepthBuffer[conPos];

                if (depth > Priority) continue;

                depth = Priority;

                ref ConsoleChar pixel = ref Game.Renderer[conPos];

                pixel.Background = CharColor.Black;
                pixel.Foreground = CharColor.Silver;
                pixel.Char = Ascii.BlockShade[0];
            }
        }
    }
}
