using ConsoleGame.Behavior;
using Win32;

namespace ConsoleGame
{
    public class PlayerBehavior : NetworkComponent, IDamageable, ICanPickUpItem
    {
        float Reload;
        float GranateReload;

        const float MaxSpeed = 3f;

        const float ReloadTime = .2f;
        const float GranateReloadTime = .5f;

        const float ProjectileSpeed = 40f;
        const float GranateSpeed = 15f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        float DamageIndicator;

        DamageableRendererComponent? DamageableRenderer;

        public PlayerBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Player;
        }

        public override void Make()
        {
            base.Make();
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

            if (Keyboard.IsKeyPressed('W') || Keyboard.IsKeyPressed(VirtualKeyCodes.UP))
            {
                Position.Y -= Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('A') || Keyboard.IsKeyPressed(VirtualKeyCodes.LEFT))
            {
                Position.X -= Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('S') || Keyboard.IsKeyPressed(VirtualKeyCodes.DOWN))
            {
                Position.Y += Game.DeltaTime * MaxSpeed;
            }

            if (Keyboard.IsKeyPressed('D') || Keyboard.IsKeyPressed(VirtualKeyCodes.RIGHT))
            {
                Position.X += Game.DeltaTime * MaxSpeed;
            }

            WorldBorders.Clamp(Game.Instance.Scene.SizeR, ref Position);

            if (Reload <= 0f && (Mouse.IsLeftDown || Keyboard.IsKeyPressed(VirtualKeyCodes.SPACE)))
            {
                Shoot(Position, (Mouse.WorldPosition - Position).Normalized);
            }

            if (Keyboard.IsKeyDown('X'))
            {
                Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.HELPER_TURRET](Game.Instance.Scene.GenerateNetworkId(), Owner);
                newEntity.Position = Mouse.WorldPosition;
                Game.Instance.Scene.AddEntity(newEntity);
            }

            if (GranateReload <= 0f && Keyboard.IsKeyPressed('G'))
            {
                Vector diff = Mouse.WorldPosition - Position;
                float speed = Math.Min(GranateSpeed, Acceleration.RequiredSpeedToReachDistance(GranateBehavior.Acceleration, (float)diff.Magnitude) ?? GranateSpeed);
                ShootGranate(Position, diff.Normalized, speed);
            }

            if (Keyboard.IsKeyDown('T'))
            {
                Damage(1f, null);
            }

            if (Reload > 0f)
            { Reload -= Game.DeltaTime; }

            if (GranateReload > 0f)
            { GranateReload -= Game.DeltaTime; }
        }

        void Shoot(Vector origin, Vector direction)
        {
            if (NetworkEntity.IsOwned) SendRpcImmediate(1, new RpcMessages.Shoot(origin, direction));

            Entity projectile = new("Player Projectile");
            projectile.SetComponents(
                    new RendererComponent(projectile)
                    {
                        Color = ByteColor.BrightYellow,
                        Character = '.',
                        Priority = Depths.PROJECTILE,
                    },
                    new ProjectileBehavior(projectile)
                    {
                        Velocity = direction * ProjectileSpeed,
                        Owner = this,
                    }
                );
            projectile.Position = Position;
            Game.Instance.Scene.AddEntity(projectile);

            Entity effect = new("Shoot Effect");
            effect.SetComponents(new ParticlesRendererComponent(effect, new ParticlesConfig(PredefinedEffects.Shoot) { Direction = direction }));
            effect.Position = Position + direction;
            Game.Instance.Scene.AddEntity(effect);

            Reload = ReloadTime;
        }

        void ShootGranate(Vector origin, Vector direction, float speed)
        {
            if (!NetworkEntity.IsOwned) return;

            Entity projectile = new("Player Granate");
            projectile.AddComponent(new RendererComponent(projectile)
            {
                Color = ByteColor.Silver,
                Character = '§',
                Priority = Depths.PROJECTILE,
            });
            projectile.AddComponent(new GranateBehavior(projectile)
            {
                Velocity = direction * speed,
                Owner = this,
            });
            projectile.Position = origin;
            Game.Instance.Scene.AddEntity(projectile);

            GranateReload = GranateReloadTime;
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
            Entity newEntity = new("Death Particles")
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.Death) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(newEntity);
        }

        public void OnItemPickedUp(ItemBehavior.ItemKind kind, float amount)
        {
            switch (kind)
            {
                case ItemBehavior.ItemKind.Health:
                    Health = Math.Min(Health + amount, MaxHealth);
                    break;
                default:
                    break;
            }
        }
    }
}
