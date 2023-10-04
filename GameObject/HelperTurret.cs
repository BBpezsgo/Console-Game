using ConsoleGame.Net;
using DataUtilities.Serializer;

#if false
namespace ConsoleGame
{
    internal class HelperTurret : NetworkedGameObject, IDamageable
    {
        struct RpcData_Shoot : ISerializable<RpcData_Shoot>
        {
            public Vector Origin;
            public Vector Direction;

            public RpcData_Shoot()
            {
                Origin = Vector.Zero;
                Direction = Vector.Zero;
            }

            public RpcData_Shoot(Vector origin, Vector direction)
            {
                Origin = origin;
                Direction = direction;
            }

            public void Deserialize(Deserializer deserializer)
            {
                Origin = deserializer.DeserializeObject<Vector>();
                Direction = deserializer.DeserializeObject(Vector.DeserializeAsDirection);
            }

            public readonly void Serialize(Serializer serializer)
            {
                serializer.Serialize(Origin);
                serializer.Serialize(Direction, Vector.SerializeAsDirection);
            }
        }

        float Reload;

        const float ReloadTime = .7f;
        const float ProjectileSpeed = 40f;

        const float VisionRadius = 15f;

        public const float MaxHealth = 10f;
        public float Health = MaxHealth;

        const int MaxAmmo = 20;

        int Ammo = MaxAmmo;

        GameObject? Target;

        public HelperTurret(Vector position, int networkId, int objectId, ObjectOwner owner)
            : base(position, networkId, objectId, owner)
        {
            Tag = Tags.Helper;
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Offline) return;
            if (!IsOwned) return;
        }

        public override void Render()
        {
            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = ByteColor.BrightCyan;
            Game.Renderer[Game.WorldToConsole(Position)].Char = 'X';
        }

        public override void Tick()
        {
            if (!IsOwned)
            {

            }
            else
            {
                if (Ammo > 0 && Reload <= 0f)
                {
                    if (Target == null || Target.IsDestroyed)
                    {
                        Target = Game.Instance.Scene.ClosestObject(Position, Tags.Enemy, VisionRadius);
                    }
                    else
                    {
                        Vector direction = (Target.Position - Position).Normalized;

                        SendRpcImmediate(1, new RpcData_Shoot(Position, direction));

                        Game.Instance.Scene.AddObject(new Projectile()
                        {
                            Position = Position,
                            Speed = direction * ProjectileSpeed,
                            Owner = this,
                        });
                        Reload = ReloadTime;

                        Ammo--;
                    }
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
                            RpcData_Shoot data = message.GetObjectData<RpcData_Shoot>();
                            Game.Instance.Scene.AddObject(new Projectile()
                            {
                                Position = data.Origin,
                                Speed = data.Direction * ProjectileSpeed,
                                Owner = this,
                            });
                        }
                        break;
                    }
                case 2:
                    {
                        RpcData_Damage data = message.GetObjectData<RpcData_Damage>();
                        Damage(data.Amount, data.By);
                        break;
                    }
                default: break;
            }
        }

        public void Damage(float amount, GameObject? by)
        {
            if (Game.NetworkMode == NetworkMode.Client) return;

            SendRpc(2, new RpcData_Damage(amount, by));

            Health -= amount;
            if (Health <= 0f)
            {
                IsDestroyed = true;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Game.Instance.Scene.AddObject(new Particles(Position, PredefinedEffects.SmallExplosion));
        }
    }
}
#endif
