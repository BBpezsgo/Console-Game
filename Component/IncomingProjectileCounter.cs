namespace ConsoleGame
{
    public class IncomingProjectileCounter : Component
    {
        public readonly struct IncomingProjectile
        {
            public readonly Entity Entity;
            public readonly Vector ShotPoint;
            public readonly float ShotTime;
            public readonly float Damage;
            public readonly float ProjectileSpeed;

            public readonly float TimeUntilImpact(Vector end)
            {
                float distance = (end - ShotPoint).Magnitude;
                distance -= (Time.UtcNow - ShotTime) * ProjectileSpeed;
                if (distance <= 0f) return 0f;
                return distance / ProjectileSpeed;
            }

            public IncomingProjectile(Entity entity, Vector shotPoint, float shotTime, float damage, float projectileSpeed)
            {
                Entity = entity;
                ShotPoint = shotPoint;
                ShotTime = shotTime;
                Damage = damage;
                ProjectileSpeed = projectileSpeed;
            }
        }

        public readonly List<IncomingProjectile> IncomingProjectiles;

        public float EstimatedDamage
        {
            get
            {
                float result = 0f;

                for (int i = IncomingProjectiles.Count - 1; i >= 0; i--)
                {
                    if (IncomingProjectiles[i].Entity.IsDestroyed)
                    {
                        IncomingProjectiles.RemoveAt(i);
                        continue;
                    }
                    result += IncomingProjectiles[i].Damage;
                }

                return result;
            }
        }

        public IncomingProjectileCounter(Entity entity) : base(entity)
        {
            IncomingProjectiles = new List<IncomingProjectile>();
        }

        public void OnShot(IncomingProjectile projectile)
        {
            IncomingProjectiles.Add(projectile);
        }
    }
}
