
#if false
namespace ConsoleGame
{
    public class Projectile : GameObject
    {
        public GameObject? Owner;
        public Vector Speed;

        public Projectile() : base()
        {
            Tag |= Tags.Projectile;
        }

        public override void Render()
        {
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = 0b_1110;
            Game.Renderer[Game.WorldToConsole(Position)].Char = '.';
        }

        public override void Tick()
        {
            Vector lastPosition = Position;
            Position += Speed * Game.DeltaTime;

            bool bounced = WorldBorders.Bounce(Game.Instance.Scene.Size, ref Position, ref Speed);
            if (bounced)
            { IsDestroyed = true; }

            if (IsDestroyed) return;

            Vector positionDiff = Position - lastPosition;
            Vector direction = positionDiff.Normalized;
            float distanceTravelled = positionDiff.Magnitude;

            Vector currentPoint = lastPosition;
            for (float i = 0f; i <= distanceTravelled; i++)
            {
                currentPoint += direction;

                GameObject? hit = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy);

                if (hit != null)
                {
                    Position = hit.Position;
                    IsDestroyed = true;
                    ((IDamageable)hit).Damage(1f, Owner);
                }
            }
        }

        public override void OnDestroy()
        {
            Game.Instance.Scene.AddObject(new Particles(Position, PredefinedEffects.MetalSparks));
        }
    }
}
#endif
