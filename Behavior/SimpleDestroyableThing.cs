namespace ConsoleGame
{
    public class SimpleDestroyableThing : NetworkComponent, IDamageable
    {
        public float Health;
        public readonly float MaxHealth;
        readonly DamageableRendererComponent? DamageableRenderer;

        public SimpleDestroyableThing(Entity entity, float maxHealth) : base(entity)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
        }

        public void Damage(float amount, Component? by)
        {
            DamageableRenderer?.OnDamage();

            if (Game.NetworkMode == NetworkMode.Client) return;

            Health -= amount;

            if (Health <= 0f)
            {
                IsDestroyed = true;
                return;
            }

            SendRpc(RpcMessages.Kind.Damage, new RpcMessages.Damaged(amount, by));
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
    }
}
