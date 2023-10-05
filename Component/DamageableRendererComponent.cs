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
            Vector position = Position;
            if (!Game.Instance.Scene.Size.Contains(position)) return;
            ref Win32.CharInfo pixel = ref Game.Renderer[Game.WorldToConsole(position)];
            pixel.Char = Character;

            float lastDamagedInterval = Time.UtcNow - LastDamaged;
            if (lastDamagedInterval < BlinkingDuration && (int)(lastDamagedInterval * BlinkPerSec) % 2 == 0)
            {
                pixel.Attributes = ByteColor.White;
            }
            else
            {
                pixel.Attributes = Color;
            }
        }

        public void OnDamage() => LastDamaged = Time.UtcNow;
    }
}
