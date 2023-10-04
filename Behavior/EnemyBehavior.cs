using ConsoleGame.Net;

namespace ConsoleGame
{
    unsafe public class EnemyBehavior : NetworkComponent, IDamageable
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

        Entity? PriorityTarget;
        Entity? Target;

        DamageableRendererComponent? DamageableRenderer;

        unsafe public EnemyBehavior(Entity entity) : base(entity)
        {
            Health = MaxHealth;
            Entity.Tags |= Tags.Enemy;
            DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new()
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.Death));
            Game.Instance.Scene.AddEntity(newEntity);
        }

        public override void Update()
        {
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
            else if (Target != null && (Target.Position - Position).SqrMagnitude >= visionRange * visionRange)
            { Target = null; }
            else
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Player | Tags.Helper, visionRange);
            }

            if (Target != null)
            {
                IDamageable? damageableTarget = Target.GetComponent<IDamageable>();
                if (damageableTarget != null)
                {
                    Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
                    if ((Target.Position - Position).SqrMagnitude < (MeleeAttackRange * MeleeAttackRange))
                    {
                        if (MeleeAttackTimer <= 0f)
                        {
                            MeleeAttackTimer = MeleeAttackCooldown;
                            damageableTarget.Damage(MeleeAttackDamage, this);
                        }
                    }
                }
            }
        }

        public void Damage(float amount, Component? by)
        {
            DamageableRenderer?.OnDamage();
            LastDamaged = Time.UtcNow;

            if (Game.NetworkMode == NetworkMode.Client) return;

            if (by != null)
            { PriorityTarget = by.Entity; }

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            SendRpc(1, new RpcMessages.Damaged(amount, by.Entity.GetComponent<NetworkEntityComponent>().NetworkId));
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case 1:
                    {
                        RpcMessages.Damaged data = message.GetObjectData<RpcMessages.Damaged>();
                        Damage(data.Amount, data.By);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
