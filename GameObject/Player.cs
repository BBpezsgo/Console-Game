﻿namespace ConsoleGame
{
    using Net;

    public class Player : NetworkedGameObject, IDamageable
    {
        float Reload = 0f;

        const float MaxSpeed = 3f;
        const float ReloadTime = .2f;
        const float ProjectileSpeed = 40f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        float DamageIndicator;

        public Player(Vector position, int networkId, int objectId, ObjectOwner owner)
            : base(position, networkId, objectId, owner)
        {
            Tag |= Tags.Player;
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Offline) return;
            if (!IsOwned) return;

            socket.Send(new ObjectPositionMessage()
            {
                Type = MessageType.OBJ_POSITION,
                Position = Position,
                NetworkId = NetworkId,
            });
        }

        public override void Render()
        {
            if (IsOwned)
            {
                const float blinkPerSec = 2f * 2;
                const float blinkingDuration = 1f;

                float damageIndicatorEffect = Time.Now - DamageIndicator;
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
            }

            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = ByteColor.BrightMagenta;
            Game.Renderer[Game.WorldToConsole(Position)].Char = 'O';
        }

        public override void Tick()
        {
            if (!IsOwned)
            {
                Position += Vector.MoveTowards(Position, NetPosition, Game.DeltaTime);

                if ((Position - NetPosition).SqrMagnitude > MaxSpeed * Game.NetworkTickRate + .5f)
                { Position = NetPosition; }
            }
            else
            {
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
                    Vector direction = (Mouse.WorldPosition - Position).Normalized;

                    if (Game.NetworkMode != NetworkMode.Offline)
                    { Game.Connection?.SendImmediate(MessageRpc.Make(this, 1, direction, Vector.SerializeAsDirection)); }

                    Game.Instance.Scene.AddObject(new Projectile()
                    {
                        Position = Position,
                        Speed = direction * ProjectileSpeed,
                        Owner = this,
                    });
                    Reload = ReloadTime;
                }

                if (Reload > 0f)
                { Reload -= Game.DeltaTime; }
            }
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case 1:
                    {
                        if (!IsOwned)
                        {
                            Vector direction = message.GetObjectData(Vector.DeserializeAsDirection);
                            Game.Instance.Scene.AddObject(new Projectile()
                            {
                                Position = NetPosition,
                                Speed = direction * ProjectileSpeed,
                                Owner = this,
                            });
                        }
                        break;
                    }
                default:

                    break;
            }
        }

        public void Damage(float amount, GameObject? by)
        {
            DamageIndicator = Time.Now;

            if (Game.NetworkMode == NetworkMode.Client) return;
            return;

            Health -= amount;
            if (Health <= 0f)
            {
                IsDestroyed = true;
            }
        }

        public override void OnDestroy()
        {
            Game.Instance.Scene.AddObject(new Particles(Position, PredefinedEffects.Death));
        }
    }
}
