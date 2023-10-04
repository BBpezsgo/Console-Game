using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public class EnemyBehaviour : Component, IDamageable
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

        public EnemyBehaviour(Entity entity) : base(entity)
        {
            Health = MaxHealth;
            Entity.Tags |= Tags.Enemy;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            base.Update();
        }

        public void Damage(float amount, GameObject? by)
        {

        }
    }
}
