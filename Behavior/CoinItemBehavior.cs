namespace ConsoleGame
{
    public class CoinItemBehavior : Component
    {
        public int Amount;
        public const float MagnetRange = 5f;
        public const float MaxSpeed = 5f;

        public CoinItemBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Item;
        }

        public override void Update()
        {
            Entity[] otherItems = Game.Instance.Scene.ObjectsAt(Position, Tags.Item | Tags.Player, MagnetRange);

            for (int i = 0; i < otherItems.Length; i++)
            {
                if (otherItems[i] == Entity) continue;

                if (!otherItems[i].TryGetComponent(out CoinItemBehavior? otherCoin)&&
                    !otherItems[i].TryGetComponent<PlayerBehavior>(out _))
                { continue; }

                Vector diff = otherItems[i].Position - Position;
                float distance = diff.SqrMagnitude;

                if (otherCoin != null && distance <= 1f)
                {
                    otherCoin.Amount += this.Amount;
                    this.IsDestroyed = true;
                    continue;
                }

                Position += diff.Normalized * (1f - (MathF.Sqrt(distance) / MagnetRange)) * Time.DeltaTime;

            }

            WorldBorders.Clamp(Game.Instance.Scene.SizeR, ref Position);
        }

        public void PickUp(ICanPickUpItem by)
        {
            if (IsDestroyed) return;

            by.OnItemPickedUp(ItemBehavior.ItemKind.Coin, Amount);
            IsDestroyed = true;

            Entity effect = new("Coin Pickup Effect");

            effect.AddComponent(new ParticlesRendererComponent(effect, new ParticlesConfig()
            {
                Gradients = new Gradient[1] { new Gradient(Color.White, Color.Yellow) },
                Characters = new char[] { '○', '°', '⁰', '˚' },
                ParticleCount = (3, 5),
                ParticleLifetime = (.2f, .3f),
                ParticleSpeed = (3f, 6f),
            }));
            effect.Position = Position;

            Game.Instance.Scene.AddEntity(effect);
        }
    }
}
