﻿namespace ConsoleGame.Behavior;

internal class GrenadeBehavior : Component
{
    public const float Damage = 10f;
    public const float Radius = 7f;
    public const float TimeToExplode = 3f;
    public const float Acceleration = -6f;

    public Vector2 Velocity;
    public Component? Owner;

    readonly float ShotTime;
    readonly RendererComponent? Renderer;
    bool FirstShot = true;
    Entity? ShotBy;

    public GrenadeBehavior(Entity entity) : base(entity)
    {
        Entity.Tags |= Tags.Projectile;
        ShotTime = Time.Now;
        Renderer = Entity.TryGetComponent<RendererComponent>();
    }

    public override void Update()
    {
        float t = Time.Now - ShotTime;

        if (FirstShot)
        {
            FirstShot = false;
            ReadOnlySpan<Entity> entities = Game.Instance.Scene.ObjectsAt(Position, 1f);
            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] == Entity) continue;
                if (!entities[i].IsSolid) continue;
                ShotBy = entities[i];
                break;
            }
        }

        if (Renderer != null)
        {
            float t2 = Math.Clamp(t / TimeToExplode, 0f, 1f) * 4f;
            if ((int)(ShotTime + (t2 * t2)) % 2 == 0)
            {
                Renderer.Color = CharColor.Gray;
            }
            else
            {
                Renderer.Color = CharColor.BrightRed;
            }
        }

        Velocity += Vector2.Normalize(Velocity) * Acceleration * Time.DeltaTime;

        // Vector2 lastPosition = Position;

        {
            ReadOnlySpan<Entity> collided = Game.Instance.Scene.ObjectsAt(Position, 1f);
            for (int i = 0; i < collided.Length; i++)
            {
                Entity other = collided[i];
                if (other == Entity) continue;
                if (other == ShotBy) continue;
                if (!other.IsSolid) continue;

                Entity.DoBounceOff(other, ref Velocity);
            }
        }

        Position += Velocity * Time.DeltaTime;

        if (IsDestroyed) return;

        // bool bounced = WorldBorders.Bounce(Game.Instance.Scene.SizeR, ref Position, ref Velocity);
        // if (bounced)
        // { Velocity = Vector.Zero; }

        if (t >= TimeToExplode)
        {
            IsDestroyed = true;
            return;
        }

        /*
        Vector positionDiff = Position - lastPosition;
        Vector direction = positionDiff.Normalized;
        float distanceTravelled = positionDiff.Magnitude;

        Vector currentPoint = lastPosition;
        for (float i = 0f; i <= distanceTravelled; i++)
        {
            currentPoint += direction;

            Entity? hit = Game.Instance.Scene.FirstObjectAt(Position, Tags.Enemy, 1f);
            if (hit == null) continue;

            // Position = hit.Position - direction;
            // Velocity = Vector.Zero;
            break;
        }
        */
    }

    public override void Destroy()
    {
        base.Destroy();

        List<Entity> entities = Game.Instance.Scene.Entities;
        for (int i = 0; i < entities.Count; i++)
        {
            Entity obj = entities[i];
            float distance = (obj.Position - Position).LengthSquared();
            if (distance >= Radius * Radius) continue;

            if (obj.TryGetComponent(out IDamageable? damageable))
            {
                distance = MathF.Sqrt(distance);
                Game.StartTimer(new Timer(.5f * (distance / Radius), () =>
                {
                    float damage = 1f - (distance / Radius);
                    damage *= Damage;
                    damageable.Damage(damage, Owner);
                }));

                continue;
            }

            if (obj.TryGetComponent(out GrenadeBehavior? otherGrenade) && otherGrenade != this)
            {
                distance = MathF.Sqrt(distance);
                Game.StartTimer(new Timer(.5f * (distance / Radius), () =>
                {
                    if (distance < 1f * 1f)
                    {
                        otherGrenade.IsDestroyed = true;
                        return;
                    }

                    otherGrenade.Velocity += Vector2.Normalize(otherGrenade.Position - Position) * (1f - (distance / Radius)) * 30f;
                }));

                continue;
            }
        }

        Sound.Play(Assets.GetAsset("explosion.wav"));

        Entity effect1 = new("Explosion Particles")
        { Position = Position };
        effect1.SetComponents(new ParticlesRendererComponent(effect1, PredefinedEffects.LargeExplosion) { Priority = Depths.EFFECT });
        Game.Instance.Scene.AddEntity(effect1);

        Entity effect2 = new("Explosion Particles")
        { Position = Position };
        effect2.SetComponents(new ParticlesRendererComponent(effect2, PredefinedEffects.ExplosionTrailStuff) { Priority = Depths.EFFECT });
        Game.Instance.Scene.AddEntity(effect2);

        Entity effect3 = new("Explosion Shockwave")
        { Position = Position };
        effect3.SetComponents(new ShockwaveRendererComponent(effect3)
        {
            Priority = Depths.EFFECT - 1,
            Lifetime = .5f,
            Radius = Radius,
        });
        Game.Instance.Scene.AddEntity(effect3);
    }
}
