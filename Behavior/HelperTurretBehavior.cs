﻿using ConsoleGame.Net;
using Win32;

namespace ConsoleGame
{
    public class HelperTurretBehavior : NetworkComponent, IDamageable, ICanDrawEntityHoverPopup
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

        readonly DamageableRendererComponent? DamageableRenderer;
        readonly RendererComponent? Renderer;

        public HelperTurretBehavior(Entity entity) : base(entity)
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
                            RpcMessages.Shoot data = message.GetObjectData<RpcMessages.Shoot>();
                            Shoot(data.Origin, data.Direction);
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
                    Vector direction = (Target.Position - Position).Normalized;

                    SendRpcImmediate(1, new RpcMessages.Shoot(Position, direction));

                    Shoot(Position, direction);
                }
            }

            if (Ammo <= 0 && Renderer != null) Renderer.Color = ByteColor.Silver;

            if (Reload > 0f)
            { Reload -= Game.DeltaTime; }
        }

        void Shoot(Vector origin, Vector direction)
        {
            if (NetworkEntity.IsOwned) SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot(origin, direction));

            Entity projectile = new("Turret Projectile");
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
            GUI.Label(content.X, content.Y, "Turret", ByteColor.Black, ByteColor.Silver);
            GUI.Label(content.X, content.Y + 1, "♥:", ByteColor.Black, ByteColor.Silver);
            float health = (Health / MaxHealth) * (content.Width - 4);

            for (int x = 0; x < content.Width - 4; x++)
            {
                ref ConsoleChar pixel = ref Game.Renderer[x + content.X + 3, content.Y + 1];
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
