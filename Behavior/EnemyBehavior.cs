using System.Numerics;
using ConsoleGame.Behavior;
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

            if (Random.Float() <= .5f)
            { Game.Instance.SpawnCoin(Position, Random.Integer(1, 4)); }
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
            { MeleeAttackTimer -= Time.DeltaTime; }

            Targeting(out bool canLoseTarget);
            TargetHandling(canLoseTarget);
            Entity.DoCollisions();
            Entity.ClampIntoWord();
        }

        void Targeting(out bool canLoseTarget)
        {
            canLoseTarget = true;

            if (Health < MaxHealth / 3f)
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Item, visionRange * 10f);
                canLoseTarget = false;
                return;
            }

            if (Target != null) return;

            if (PriorityTarget != null)
            {
                if (PriorityTarget.IsDestroyed)
                {
                    PriorityTarget = null;
                }
                else
                {
                    Target = PriorityTarget;
                    canLoseTarget = false;
                    return;
                }
            }

            Target = Game.Instance.Scene.ClosestObject(Position, Tags.Player | Tags.Helper, visionRange);
            return;
        }

        void TargetHandling(bool canLoseTarget)
        {
            if (Target == null) return;

            if (Target.IsDestroyed ||
                (canLoseTarget && ((Target.Position - Position).LengthSquared() >= visionRange * visionRange)))
            {
                Target = null;
                return;
            }

            if (Target.TryGetComponent(out IDamageable? damageableTarget))
            {
                Vector2 diff = Target.Position - Position;
                float sqrMag = diff.LengthSquared();
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

                        Entity newEntity = new();
                        newEntity.AddComponent(new ParticlesRendererComponent(newEntity, PredefinedEffects.Stuff));
                        newEntity.Position = Position + (diff * .5f);
                        Game.Instance.Scene.AddEntity(newEntity);
                    }
                }
            }
            else if (Target.TryGetComponent(out ItemBehavior? item))
            {
                float sqrMag = (Target.Position - Position).LengthSquared();
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

            SendRpc(RpcMessages.Kind.Damage, new RpcMessages.Damaged(amount, by));
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case RpcMessages.Kind.Damage:
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
