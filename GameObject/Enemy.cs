using ConsoleGame.Net;

namespace ConsoleGame
{
    public class Enemy : NetworkedGameObject, IDamageable
    {
        public float Health;
        public const float MaxHealth = 5f;

        const float MaxSpeed = 1f;

        float lastDamaged = 0f;
        const float blinkPerSec = 3f * 2;
        const float blinkingDuration = 1f;
        const float visionRange = 10f;
        const float MeleeAttackCooldown = 1f;
        float MeleeAttackTimer = 0f;
        const float MeleeAttackRange = 2f;
        const float MeleeAttackDamage = 1f;

        GameObject? priorityTarget;

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

            float damageEffect = Time.Now - lastDamaged;
            if (damageEffect < blinkingDuration && (int)(damageEffect * blinkPerSec) % 2 == 0)
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

            GameObject? target;

            if (priorityTarget == null)
            { target = Game.Instance.Scene.ClosestObject(Position, Tags.Player, visionRange); }
            else
            { target = priorityTarget; }

            if (target != null && target is IDamageable damageable)
            {
                Position += Vector.MoveTowards(Position, target.Position, MaxSpeed * Time.DeltaTime);
                if ((target.Position - Position).SqrMagnitude < (MeleeAttackRange * 2f))
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

        }

        public void Damage(float amount, GameObject? by)
        {
            lastDamaged = Time.Now;

            if (by != null)
            {
                priorityTarget = by;
            }

            if (Game.NetworkMode == NetworkMode.Client) return;

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }
        }

        public override void OnDestroy()
        {
            Game.Instance.Scene.AddObject(new Particles(Position, PredefinedEffects.Death));
        }
    }
}
