namespace ConsoleGame;

public class ShockwaveRendererComponent : RendererComponent
{
    public float Radius;
    public float Lifetime;
    public readonly float BornTime;

    public ShockwaveRendererComponent(Entity entity) : base(entity)
    {
        BornTime = Time.Now;
    }

    public override void Render()
    {
        float life = (Time.Now - BornTime) / Lifetime;
        if (life >= 1f)
        {
            Entity.IsDestroyed = true;
            return;
        }

        float radius = Radius * MathF.Sqrt(life);
        int points = (int)radius * 8;

        for (int i = 0; i < points; i++)
        {
            Vector2 direction = Rotation.FromDeg((float)i / (float)points);

            Vector2 p = (direction * radius) + Position;

            p += direction * ((Noise.Simplex(p.X, p.Y) * .5f) + .5f) * 1.5f;

            if (!Game.IsVisible(p)) continue;

            Vector2Int conPos = Game.WorldToConsole(p);

            if (Game.IsOnGui(conPos)) continue;

            ref float depth = ref Game.DepthBuffer[conPos];

            if (depth > Priority) continue;

            depth = Priority;

            Game.Renderer.Set(conPos, new ConsoleChar(Ascii.BlockShade[0], CharColor.Silver));
        }
    }
}
