namespace ConsoleGame
{
    internal class DamageableRendererComponent : RendererComponent
    {
        float LastDamaged;

        const float BlinkPerSec = 4f * 2;
        const float BlinkingDuration = 1f;

        public DamageableRendererComponent(Entity entity) : base(entity) { }

        public override void Render()
        {
            if (!Game.IsVisible(Position)) return;

            VectorInt p = Game.WorldToConsole(Position);

            ref byte depth = ref Game.DepthBuffer[p];

            if (depth > Priority) return;

            depth = Priority;

            ref Win32.CharInfo pixel = ref Game.Renderer[p];
            pixel.Char = Character;

            float lastDamagedInterval = Time.UtcNow - LastDamaged;
            if (lastDamagedInterval < BlinkingDuration && (int)(lastDamagedInterval * BlinkPerSec) % 2 == 0)
            {
                pixel.Foreground = ByteColor.White;
            }
            else
            {
                pixel.Foreground = Color;
            }
        }

        public void OnDamage() => LastDamaged = Time.UtcNow;
    }
}
