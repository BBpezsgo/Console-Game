using ConsoleGame.Net;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class PlayerBehavior : NetworkComponent, IDamageable
    {
        float Reload;

        const float MaxSpeed = 3f;
        const float ReloadTime = .2f;
        const float ProjectileSpeed = 40f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        float DamageIndicator;

        DamageableRendererComponent? DamageableRenderer;

        public PlayerBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Player;
            DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
        }

        public override void Update()
        {
            if (!IsOwned)
            { return; }

            const float blinkPerSec = 2f * 2;
            const float blinkingDuration = 1f;

            float damageIndicatorEffect = Time.UtcNow - DamageIndicator;
            if (damageIndicatorEffect < blinkingDuration && (int)(damageIndicatorEffect * blinkPerSec) % 2 == 0)
            {
                for (int y = 4; y < Game.Renderer.Height; y++)
                {
                    Game.Renderer[0, y].Background = ByteColor.Red;
                    Game.Renderer[Game.Renderer.Width - 1, y].Background = ByteColor.Red;
                }
                for (int x = 0; x < Game.Renderer.Width; x++)
                {
                    Game.Renderer[x, 4].Background = ByteColor.Red;
                    Game.Renderer[x, Game.Renderer.Height - 1].Background = ByteColor.Red;
                }
            }

            if (Keyboard.IsKeyPressed('W'))
            {
                Position.Y -= Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('A'))
            {
                Position.X -= Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('S'))
            {
                Position.Y += Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('D'))
            {
                Position.X += Game.DeltaTime * MaxSpeed;
            }

            WorldBorders.Clamp(Game.Instance.Scene.Size, ref Position);

            if (Reload <= 0f && Mouse.IsLeftDown)
            {
                Shoot(Position, (Mouse.WorldPosition - Position).Normalized);
            }

            if (Keyboard.IsKeyDown('X'))
            {
                var newEntity = new Entity();
                newEntity.SetComponents(
                    new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = Game.Instance.Scene.GenerateNetworkId(),
                        ObjectId = GameObjectPrototype.HELPER_TURRET,
                        Owner = Owner,
                    },
                    new DamageableRendererComponent(newEntity)
                    {
                        Character = 'X',
                        Color = ByteColor.BrightBlue,
                    },
                    new HelperTurretBehavior(newEntity)
                    );
                Game.Instance.Scene.AddEntity(newEntity);
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

        public override void OnRpc(MessageRpc message)
        {
            base.OnRpc(message);

            switch (message.RpcKind)
            {
                case 1:
                    {
                        if (!NetworkEntity.IsOwned)
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

        public void Damage(float amount, Component? by)
        {
            DamageableRenderer?.OnDamage();
            DamageIndicator = Time.UtcNow;

            if (Game.NetworkMode == NetworkMode.Client) return;

            SendRpc(2, new RpcMessages.Damaged(amount, by));

            Health -= amount;
            if (Health <= 0f)
            {
                IsDestroyed = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new()
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.Death));
            Game.Instance.Scene.AddEntity(newEntity);
        }
    }
}
