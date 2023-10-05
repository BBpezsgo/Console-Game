namespace ConsoleGame.Behavior
{
    internal class GranateBehavior : Component
    {
        public Vector Velocity;
        public Component? Owner;

        public const float Damage = 10f;
        public const float Radius = 7f;
        public const float TimeToExplode = 3f;

        public const float Acceleration = -6f;

        readonly float ShotTime;

        public GranateBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Projectile;
            ShotTime = Time.UtcNow;
        }

        public override void Update()
        {
            Velocity += Velocity.Normalized * Acceleration * Game.DeltaTime;

            Vector lastPosition = Position;
            Position += Velocity * Game.DeltaTime;

            if (IsDestroyed) return;
            
            bool bounced = WorldBorders.Bounce(Game.Instance.Scene.Size, ref Position, ref Velocity);
            if (bounced)
            { Velocity = Vector.Zero; }

            if (Time.UtcNow - ShotTime >= TimeToExplode)
            {
                IsDestroyed = true;
                return;
            }

            Vector positionDiff = Position - lastPosition;
            Vector direction = positionDiff.Normalized;
            float distanceTravelled = positionDiff.Magnitude;

            Vector currentPoint = lastPosition;
            for (float i = 0f; i <= distanceTravelled; i++)
            {
                currentPoint += direction;

                Entity? hit = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy);
                if (hit != null)
                {
                    Position = hit.Position - direction;
                    Velocity = Vector.Zero;
                    break;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            List<Entity> objs = Game.Instance.Scene.Entities;
            for (int i = 0; i < objs.Count; i++)
            {
                Entity obj = objs[i];
                float distance = (obj.Position - Position).SqrMagnitude;
                if (distance >= Radius * Radius) continue;
                if (!obj.TryGetComponent(out IDamageable? damageable)) continue;
                distance = MathF.Sqrt(distance);
                float damage = 1f - (distance / Radius);
                damage *= Damage;
                damageable.Damage(damage, Owner);
            }

            Entity effect1 = new("Explosion Particles")
            { Position = Position };
            effect1.SetComponents(new ParticlesRendererComponent(effect1, PredefinedEffects.LargeExplosion) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(effect1);

            Entity effect2 = new("Explosion Particles")
            { Position = Position };
            effect2.SetComponents(new ParticlesRendererComponent(effect2, PredefinedEffects.ExplosionTrailStuff) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(effect2);
        }
    }
}
