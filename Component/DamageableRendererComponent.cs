namespace ConsoleGame;

internal class DamageableRendererComponent : RendererComponent
{
    float LastDamaged;

    const float BlinkPerSec = 4f * 2;
    const float BlinkingDuration = 1f;

    public DamageableRendererComponent(Entity entity) : base(entity) { }

    public override void Render()
    {
        if (!Game.IsVisible(Position)) return;

        Vector2Int p = Game.WorldToConsole(Position);

        ref float depth = ref Game.DepthBuffer[p];

        if (depth > Priority) return;

        depth = Priority;

        float lastDamagedInterval = Time.Now - LastDamaged;
        if (lastDamagedInterval < BlinkingDuration && (int)(lastDamagedInterval * BlinkPerSec) % 2 == 0)
        {
            Game.Renderer.Set(p.X, p.Y, new ConsoleChar(Character, CharColor.White));
        }
        else
        {
            Game.Renderer.Set(p.X, p.Y, new ConsoleChar(Character, Color));
        }
    }

    public void OnDamage() => LastDamaged = Time.Now;
}
