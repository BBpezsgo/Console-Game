namespace ConsoleGame.Behavior
{
    public class CoinItemRendererComponent : RendererComponent
    {
        readonly CoinItemBehavior Item;
        float SpawnedTime;

        public CoinItemRendererComponent(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Item;
            Item = Entity.GetComponent<CoinItemBehavior>();
        }

        public override void Make()
        {
            base.Make();
            SpawnedTime = Time.UtcNow;
        }

        public override void Render()
        {
            if (!Game.IsVisible(Position)) return;

            Vector2Int p = Game.WorldToConsole(Position);

            ref float depth = ref Game.DepthBuffer[p];

            if (depth > Priority) return;

            depth = Priority;

            int i1 = (int)((Time.UtcNow - SpawnedTime) * 3f) % 2;
            int i2 = Math.Clamp(Item.Amount, 0, 20);

            Game.Renderer[p] = new Win32.ConsoleChar((i1 == 0) ? Ascii.CircleNumbersFilled[i2] : Ascii.CircleNumbersOutline[i2], Color);
        }
    }
}
