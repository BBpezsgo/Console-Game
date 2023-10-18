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

            VectorInt p = Game.WorldToConsole(Position);

            ref float depth = ref Game.DepthBuffer[p];

            if (depth > Priority) return;

            depth = Priority;

            ref Win32.CharInfo pixel = ref Game.Renderer[p];

            int i1 = (int)((Time.UtcNow - SpawnedTime) * 3f) % 2;
            int i2 = Math.Clamp(Item.Amount, 0, 20);

            pixel.Foreground = Color;
            pixel.Char = (i1 == 0) ? Ascii.CircleNumbersFilled[i2] : Ascii.CircleNumbersOutline[i2];
        }
    }
}
