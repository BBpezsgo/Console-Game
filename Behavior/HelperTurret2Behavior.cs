using ConsoleGame.Behavior;
using ConsoleGame.Net;
using Win32;

namespace ConsoleGame
{
    public class HelperTurret2Behavior : NetworkComponent, IDamageable, ICanDrawEntityHoverPopup
    {
        float Reload;

        const float ReloadTime = .7f;
        const float ProjectileSpeed = 15f;

        const float VisionRadius = 15f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        const int MaxAmmo = 20;

        int Ammo = MaxAmmo;

        Entity? Target;

        readonly DamageableRendererComponent? DamageableRenderer;
        readonly RendererComponent? Renderer;

        public HelperTurret2Behavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Helper;
            DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
            if (DamageableRenderer == null)
            { Renderer = Entity.TryGetComponent<RendererComponent>(); }
            else
            { Renderer = DamageableRenderer; }
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new("Death Explosion Particles")
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.SmallExplosion) { Priority = Depths.EFFECT });
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
                case RpcMessages.Kind.Shoot:
                    {
                        if (!IsOwned)
                        {
                            RpcMessages.Shoot2 data = message.GetObjectData<RpcMessages.Shoot2>();
                            Shoot(data.Origin, data.Direction, data.Speed);
                        }
                        break;
                    }
                case RpcMessages.Kind.Damage:
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
                    Vector diff = Target.Position - Position;
                    Vector direction = diff.Normalized;
                    float speed = Math.Min(ProjectileSpeed, Acceleration.RequiredSpeedToReachDistance(GranateBehavior.Acceleration, (float)diff.Magnitude) ?? ProjectileSpeed);

                    Vector.RotateByDeg(ref direction, Random.Float(-1f, 1f));

                    SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot2(Position, direction, speed));

                    Shoot(Position, direction, speed);
                }
            }

            if (Ammo <= 0 && Renderer != null) Renderer.Color = ByteColor.Silver;

            if (Reload > 0f)
            { Reload -= Game.DeltaTime; }
        }

        void Shoot(Vector origin, Vector direction, float speed)
        {
            if (NetworkEntity.IsOwned) SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot2(origin, direction, speed));

            Entity granate = new("Granate");
            granate.SetComponents(
                    new RendererComponent(granate)
                    {
                        Color = ByteColor.Silver,
                        Character = '§',
                        Priority = Depths.PROJECTILE,
                    },
                    new GranateBehavior(granate)
                    {
                        Velocity = direction * speed,
                        Owner = this,
                    }
                );
            granate.Position = Position;
            Game.Instance.Scene.AddEntity(granate);

            Entity effect = new("Shoot Particles");
            effect.SetComponents(new ParticlesRendererComponent(effect, new ParticlesConfig(PredefinedEffects.Shoot) { Direction = direction }) { Priority = Depths.EFFECT });
            effect.Position = Position + direction;
            Game.Instance.Scene.AddEntity(effect);

            Reload = ReloadTime;
            Ammo--;
        }

        public void Damage(float amount, Component? by)
        {
            DamageableRenderer?.OnDamage();

            if (Game.NetworkMode == NetworkMode.Client) return;

            SendRpc(RpcMessages.Kind.Damage, new RpcMessages.Damaged(amount, by));

            Health -= amount;
            if (Health <= 0f)
            {
                IsDestroyed = true;
            }
        }

        public void RenderHoverPopup(RectInt content)
        {
            GUI.Label(content.X, content.Y, "Turret2", ByteColor.Black, ByteColor.Silver);
            GUI.Label(content.X, content.Y + 1, "♥:", ByteColor.Black, ByteColor.Silver);
            float health = (Health / MaxHealth) * (content.Width - 4);

            for (int x = 0; x < content.Width - 4; x++)
            {
                ref CharInfo pixel = ref Game.Renderer[x + content.X + 3, content.Y + 1];
                pixel.Background = ByteColor.Gray;
                pixel.Foreground = ByteColor.BrightRed;
                if (health > x)
                {
                    pixel.Char = Ascii.Blocks.Full;
                }
                else if (health <= x)
                {
                    pixel.Char = ' ';
                }
            }

            GUI.Label(content.X, content.Y + 2, "∆:", ByteColor.Black, ByteColor.Silver);

            GUI.Label(content.X + 3, content.Y + 2, Ammo.ToString(), ByteColor.Black, ByteColor.White);
        }
    }
}
