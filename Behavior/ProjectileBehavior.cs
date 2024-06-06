namespace ConsoleGame.Behavior;

public class Projectile2Behavior : Component
{
    public Vector2 Velocity;
    public Component? Owner;

    public const float Damage = 1f;

    public Projectile2Behavior(Entity entity) : base(entity)
    {
        Entity.Tags |= Tags.Projectile;
    }

    public override void Update()
    {
        Vector2 lastPosition = Position;
        Position += Velocity * Time.DeltaTime;

        bool bounced = WorldBorders.Bounce(Game.Instance.Scene.SizeR, ref Position, ref Velocity);
        if (bounced)
        { IsDestroyed = true; }

        if (IsDestroyed) return;

        Vector2 positionDiff = Position - lastPosition;
        Vector2 direction = Vector2.Normalize(positionDiff);
        float distanceTravelled = positionDiff.Length();

        Vector2 currentPoint = lastPosition;
        for (float i = 0f; i <= distanceTravelled; i++)
        {
            currentPoint += direction;

            Entity? hitEntity = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy, 1f);
            if (hitEntity != null)
            {
                IDamageable? hit = hitEntity?.TryGetComponent<IDamageable>();

                if (hit != null)
                {
                    Position = ((Component)hit).Position;
                    IsDestroyed = true;
                    hit.Damage(Damage, Owner);
                    return;
                }
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        Entity newEntity = new("Projectile2 Impact Particles")
        { Position = Position };
        newEntity.SetComponents(new ParticlesRendererComponent3D(newEntity, PredefinedEffects.SmallExplosion));
        Game.Instance.Scene.AddEntity(newEntity);
    }
}
