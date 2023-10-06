using ConsoleGame.Net;

namespace ConsoleGame
{
    unsafe public class EnemyBehavior : NetworkComponent, IDamageable, ICanPickUpItem
    {
        public float Health;
        public const float MaxHealth = 5f;

        const float MaxSpeed = 2.5f;

        const float visionRange = 10f;

        const float MeleeAttackCooldown = 1f;
        float MeleeAttackTimer;
        const float MeleeAttackRange = 1.2f;
        const float MeleeAttackDamage = 1f;

        Entity? PriorityTarget;
        Entity? Target;

        readonly DamageableRendererComponent? DamageableRenderer;

        unsafe public EnemyBehavior(Entity entity) : base(entity)
        {
            Health = MaxHealth;
            Entity.Tags |= Tags.Enemy;
            DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
        }

        public override void Destroy()
        {
            base.Destroy();
            Entity newEntity = new("Death Particles")
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.Death) { Priority = Depths.EFFECT });
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

            if (Health < MaxHealth / 3f)
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Item, visionRange * 10f);
            }

            Targeting();

            TargetHandling();
        }

        void Targeting()
        {
            if (Target != null) return;

            if (PriorityTarget != null)
            {
                Target = PriorityTarget;
                if (PriorityTarget.IsDestroyed)
                {
                    PriorityTarget = null;
                }
            }
            else
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Player | Tags.Helper, visionRange);
            }
        }

        void TargetHandling()
        {
            if (Target == null) return;

            if (Target.IsDestroyed ||
                ((Target.Position - Position).SqrMagnitude >= visionRange * visionRange))
            {
                Target = null;
                return;
            }

            if (Target.TryGetComponent(out IDamageable? damageableTarget))
            {
                float sqrMag = (Target.Position - Position).SqrMagnitude;
                if (sqrMag > MeleeAttackRange * MeleeAttackRange)
                {
                    Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
                }
                else
                {
                    if (MeleeAttackTimer <= 0f)
                    {
                        MeleeAttackTimer = MeleeAttackCooldown;
                        damageableTarget.Damage(MeleeAttackDamage, this);
                    }
                }
            }
            else if (Target.TryGetComponent(out ItemBehavior? item))
            {
                float sqrMag = (Target.Position - Position).SqrMagnitude;
                if (sqrMag > MeleeAttackRange * MeleeAttackRange)
                {
                    Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
                }
                else
                {
                    item.PickUp(this);
                }
            }
        }

        public void Damage(float amount, Component? by)
        {
            DamageableRenderer?.OnDamage();

            if (Game.NetworkMode == NetworkMode.Client) return;

            if (by != null)
            { PriorityTarget = by.Entity; }

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            SendRpc(1, new RpcMessages.Damaged(amount, by));
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
