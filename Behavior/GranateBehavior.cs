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

        readonly RendererComponent? Renderer;

        bool FirstShot = true;
        Entity? ShotBy;

        public GranateBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Projectile;
            ShotTime = Time.UtcNow;
            Renderer = Entity.TryGetComponent<RendererComponent>();
        }

        public override void Update()
        {
            float t = Time.UtcNow - ShotTime;

            if (FirstShot)
            {
                FirstShot = false;
                Entity[] objs = Game.Instance.Scene.ObjectsAt(Position, 1f);
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] == Entity) continue;
                    if (!objs[i].IsSolid) continue;
                    ShotBy = objs[i];
                    break;
                }
            }

            if (Renderer != null)
            {
                float t2 = Math.Clamp(t / TimeToExplode, 0f, 1f) * 4f;
                if ((int)(ShotTime + (t2 * t2)) % 2 == 0)
                {
                    Renderer.Color = ByteColor.Gray;
                }
                else
                {
                    Renderer.Color = ByteColor.BrightRed;
                }
            }

            Velocity += Velocity.Normalized * Acceleration * Time.DeltaTime;

            Vector lastPosition = Position;

            {
                Entity[] collided = Game.Instance.Scene.ObjectsAt(Position, 1f);
                for (int i = 0; i < collided.Length; i++)
                {
                    Entity other = collided[i];
                    if (other == Entity) continue;
                    if (other == ShotBy) continue;
                    if (!other.IsSolid) continue;

                    Entity.DoBounceOff(other, ref Velocity);
                }
            }

            Position += Velocity * Time.DeltaTime;

            if (IsDestroyed) return;

            bool bounced = WorldBorders.Bounce(Game.Instance.Scene.SizeR, ref Position, ref Velocity);
            // if (bounced)
            // { Velocity = Vector.Zero; }

            if (t >= TimeToExplode)
            {
                IsDestroyed = true;
                return;
            }

            /*
            Vector positionDiff = Position - lastPosition;
            Vector direction = positionDiff.Normalized;
            float distanceTravelled = positionDiff.Magnitude;

            Vector currentPoint = lastPosition;
            for (float i = 0f; i <= distanceTravelled; i++)
            {
                currentPoint += direction;

                Entity? hit = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy, 1f);
                if (hit == null) continue;

                // Position = hit.Position - direction;
                // Velocity = Vector.Zero;
                break;
            }
            */
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

                if (obj.TryGetComponent(out IDamageable? damageable))
                {
                    distance = MathF.Sqrt(distance);
                    Game.StartTimer(new DynamicTimer(.5f * (distance / Radius), () =>
                    {
                        float damage = 1f - (distance / Radius);
                        damage *= Damage;
                        damageable.Damage(damage, Owner);
                    }));

                    continue;
                }

                if (obj.TryGetComponent(out GranateBehavior? otherGranate) && otherGranate != this)
                {
                    distance = MathF.Sqrt(distance);
                    Game.StartTimer(new DynamicTimer(.5f * (distance / Radius), () =>
                    {
                        if (distance < 1f * 1f)
                        {
                            otherGranate.IsDestroyed = true;
                            return;
                        }

                        otherGranate.Velocity += (otherGranate.Position - Position).Normalized * (1f - (distance / Radius)) * 30f;
                    }));

                    continue;
                }
            }

            Entity effect1 = new("Explosion Particles")
            { Position = Position };
            effect1.SetComponents(new ParticlesRendererComponent(effect1, PredefinedEffects.LargeExplosion) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(effect1);

            Entity effect2 = new("Explosion Particles")
            { Position = Position };
            effect2.SetComponents(new ParticlesRendererComponent(effect2, PredefinedEffects.ExplosionTrailStuff) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(effect2);

            Entity effect3 = new("Explosion Shockwave")
            { Position = Position };
            effect3.SetComponents(new ShockwaveRendererComponent(effect3)
            {
                Priority = Depths.EFFECT - 1,
                Lifetime = .5f,
                Radius = Radius,
            });
            Game.Instance.Scene.AddEntity(effect3);
        }
    }
}
