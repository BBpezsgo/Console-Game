namespace ConsoleGame
{
    /*
    public class ShockwaveEffect : GameObject
    {
        readonly float Lifetime;
        readonly float MaxRadius;
        float Age;

        public ShockwaveEffect(float lifetime, float maxRadius) : base()
        {
            Lifetime = lifetime;
            MaxRadius = maxRadius;
            Age = 0f;
        }

        public override void Render()
        {
            float life = Math.Clamp(Age / Lifetime, 0f, 1f);
            float radius = MaxRadius * life;
            int points = (int)radius * 8;

            for (int i = 0; i < points; i++)
            {
                float v = (float)i / (float)points * MathF.PI * 2f;
                Vector p = (new Vector(MathF.Cos(v), MathF.Sin(v)) * radius) + Position;
                if (!Game.Instance.Scene.Size.Contains(p)) continue;

                Game.Renderer[Game.WorldToConsole(p)].Background = (byte)(new Color(1f) * (1f - life));
            }
        }

        public override void Tick()
        {
            Age += Game.DeltaTime;
            if (Age >= Lifetime)
            {
                IsDestroyed = true;
                return;
            }
        }
    }
    */
}
