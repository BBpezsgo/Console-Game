namespace ConsoleGame
{
    public class ProjectileBehavior : Component
    {
        public Vector Velocity;
        public Component? Owner;

        public const float Damage = 1f;

        public ProjectileBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Projectile;
        }

        public override void Update()
        {
            Vector lastPosition = Position;
            Position += Velocity * Game.DeltaTime;

            bool bounced = WorldBorders.Bounce(Game.Instance.Scene.SizeR, ref Position, ref Velocity);
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

                Entity? hitEntity = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy);
                if (hitEntity != null)
                {
                    IDamageable? hit = hitEntity?.TryGetComponent<IDamageable>();

                    if (hit != null)
                    {
                        Position = ((Component)hit).Position;
                        IsDestroyed = true;
                        hit.Damage(Damage, Owner);
                        return;
                    }
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new("Projectile Impact Particles")
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.MetalSparks) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(newEntity);
        }
    }
}
