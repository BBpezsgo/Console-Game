using ConsoleGame.Net;

#if false

namespace ConsoleGame
{
    public class Enemy : NetworkedGameObject, IDamageable
    {
        public float Health;
        public const float MaxHealth = 5f;

        const float MaxSpeed = 2.5f;

        float LastDamaged;
        const float DamageIndicatorBlinkPerSec = 3f * 2;
        const float DamageIndicatorDuration = 1f;

        const float visionRange = 10f;

        const float MeleeAttackCooldown = 1f;
        float MeleeAttackTimer;
        const float MeleeAttackRange = 2f;
        const float MeleeAttackDamage = 1f;

        GameObject? PriorityTarget;
        GameObject? Target;

        public Enemy(Vector position, int networkId, int objectId, ObjectOwner owner) : base(position, networkId, objectId, owner)
        {
            Health = MaxHealth;
            Tag |= Tags.Enemy;
        }

        public override void Render()
        {
            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            ref Win32.CharInfo p = ref Game.Renderer[Game.WorldToConsole(Position)];
            p.Foreground = ByteColor.BrightRed;
            p.Char = '@';

            float damageEffect = Time.UtcNow - LastDamaged;
            if (damageEffect < DamageIndicatorDuration && (int)(damageEffect * DamageIndicatorBlinkPerSec) % 2 == 0)
            {
                p.Foreground = ByteColor.White;
            }
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Server)
            {
                socket.Send(new ObjectPositionMessage()
                {
                    Type = MessageType.OBJ_POSITION,
                    Position = Position,
                    NetworkId = NetworkId,
                });
            }
        }

        public override void Tick()
        {
            if (Game.NetworkMode == NetworkMode.Client)
            {
                Position += Vector.MoveTowards(Position, NetPosition, Game.DeltaTime);
                return;
            }

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            if (MeleeAttackTimer > 0f)
            {
                MeleeAttackTimer -= Time.DeltaTime;
            }

            if (PriorityTarget != null)
            { Target = PriorityTarget; }
            else if (Target != null && (Target.Position - Position).SqrMagnitude >= visionRange * visionRange)
            { Target = null; }
            else
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Player | Tags.Helper, visionRange);
            }

            if (Target != null && Target is IDamageable damageable)
            {
                Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
                if ((Target.Position - Position).SqrMagnitude < (MeleeAttackRange * MeleeAttackRange))
                {
                    if (MeleeAttackTimer <= 0f)
                    {
                        MeleeAttackTimer = MeleeAttackCooldown;
                        damageable.Damage(MeleeAttackDamage, this);
                    }
                }
            }
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case 1:
                    {
                        RpcData_Damage data = message.GetObjectData<RpcData_Damage>();
                        Damage(data.Amount, data.By);
                        break;
                    }
                default:
                    break;
            }
        }

        public void Damage(float amount, GameObject? by)
        {
            LastDamaged = Time.UtcNow;

            if (Game.NetworkMode == NetworkMode.Client) return;

            if (by != null)
            { PriorityTarget = by; }

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            SendRpc(1, new RpcData_Damage(amount, by));
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Game.Instance.Scene.AddObject(new Particles(Position, PredefinedEffects.Death));
        }
    }
}
#endif
