using ConsoleGame.Net;

namespace ConsoleGame
{
    unsafe public class EnemyBehaviour : NetworkComponent, IDamageable
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

        TransformComponent? PriorityTarget;
        TransformComponent? Target;

        readonly TransformComponent Transform;

        unsafe public EnemyBehaviour(Entity entity) : base(entity)
        {
            Health = MaxHealth;
            Entity.Tags |= Tags.Enemy;
            Transform = entity.GetComponentOfType<TransformComponent>();
        }

        public override void Destroy()
        {
            // Game.Instance.Scene.AddEntity(new Particles(Transform.Position, PredefinedEffects.Death));
            base.Destroy();
        }

        public override void Update()
        {
            base.Update();
        
            if (Game.NetworkMode == NetworkMode.Client)
            { return; }

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
            else if (Target != null && (Target.Position - Transform.Position).SqrMagnitude >= visionRange * visionRange)
            { Target = null; }
            else
            {
                Target = Game.Instance.Scene.ClosestObject(Transform.Position, Tags.Player | Tags.Helper, visionRange)?.GetComponentOfType<TransformComponent>();
            }

            if (Target != null && Target is IDamageable damageable)
            {
                Transform.Position += Vector.MoveTowards(Transform.Position, Target.Position, MaxSpeed * Time.DeltaTime);
                if ((Target.Position - Transform.Position).SqrMagnitude < (MeleeAttackRange * MeleeAttackRange))
                {
                    if (MeleeAttackTimer <= 0f)
                    {
                        MeleeAttackTimer = MeleeAttackCooldown;
                        damageable.Damage(MeleeAttackDamage, this);
                    }
                }
            }
        }

        public void Damage(float amount, Component? by)
        {
            LastDamaged = Time.UtcNow;

            if (Game.NetworkMode == NetworkMode.Client) return;

            if (by != null)
            { PriorityTarget = by.Entity.GetComponentOfType<TransformComponent>(); }

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            SendRpc(1, new RpcData_Damage(amount, by.Entity.GetComponentOfType<NetworkEntityComponent>().NetworkId));
        }

        public override void OnMessageReceived(ObjectMessage message)
        {
            
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

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            
        }
    }
}
