using ConsoleGame.Net;

namespace ConsoleGame
{
    public class HelperTurretBehavior : NetworkComponent, IDamageable
    {
        float Reload;

        const float ReloadTime = .7f;
        const float ProjectileSpeed = 40f;

        const float VisionRadius = 15f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        const int MaxAmmo = 20;

        int Ammo = MaxAmmo;

        Entity? Target;

        public HelperTurretBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Helper;
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new()
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.SmallExplosion));
            Game.Instance.Scene.AddEntity(newEntity);
        }

        public override void OnMessage(ObjectMessage message)
        {
            base.OnMessage(message);
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case 1:
                    {
                        if (!IsOwned)
                        {
                            RpcMessages.Shoot data = message.GetObjectData<RpcMessages.Shoot>();
                            Shoot(data.Origin, data.Direction);
                        }
                        break;
                    }
                case 2:
                    {
                        RpcMessages.Damaged data = message.GetObjectData<RpcMessages.Damaged>();
                        Damage(data.Amount, data.By);
                        break;
                    }
                default: break;
            }
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            base.Synchronize(mode, socket);
        }

        public override void Update()
        {
            if (!IsOwned) return;

            if (Ammo > 0 && Reload <= 0f)
            {
                if (Target == null || Target.IsDestroyed)
                {
                    Target = Game.Instance.Scene.ClosestObject(Position, Tags.Enemy, VisionRadius);
                }
                else
                {
                    Vector direction = (Target.Position - Position).Normalized;

                    SendRpcImmediate(1, new RpcMessages.Shoot(Position, direction));

                    Shoot(Position, direction);
                }
            }

            if (Reload > 0f)
            { Reload -= Game.DeltaTime; }
        }

        void Shoot(Vector origin, Vector direction)
        {
            if (NetworkEntity.IsOwned) SendRpcImmediate(1, new RpcMessages.Shoot(origin, direction));

            Entity projectile = new();
            projectile.SetComponents(
                    new RendererComponent(projectile)
                    {
                        Color = ByteColor.BrightYellow,
                        Character = '.',
                    },
                    new ProjectileBehavior(projectile)
                    {
                        Velocity = direction * ProjectileSpeed,
                        Owner = this,
                    }
                );
            projectile.Position = Position;
            Game.Instance.Scene.AddEntity(projectile);
            Reload = ReloadTime;
        }

        public void Damage(float amount, Component? by)
        {
            if (Game.NetworkMode == NetworkMode.Client) return;

            SendRpc(2, new RpcMessages.Damaged(amount, by));

            Health -= amount;
            if (Health <= 0f)
            {
                IsDestroyed = true;
            }
        }
    }
}
