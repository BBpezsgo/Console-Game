using ConsoleGame.Net;

namespace ConsoleGame.Behavior;

public class HelperTurret2Behavior : NetworkComponent, IDamageable, ICanDrawEntityHoverPopup
{
    public const float MaxHealth = 10f;
    const float ReloadTime = .7f;
    const float ProjectileSpeed = 15f;
    const float VisionRadius = 15f;
    const int MaxAmmo = 20;

    public float Health = MaxHealth;

    float Reload;
    int Ammo = MaxAmmo;
    Entity? Target;

    readonly DamageableRendererComponent? DamageableRenderer;
    readonly RendererComponent? Renderer;

    public HelperTurret2Behavior(Entity entity) : base(entity)
    {
        Entity.Tags |= Tags.Helper;
        DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent>();
        Renderer = DamageableRenderer ?? Entity.TryGetComponent<RendererComponent>();
    }

    public override void Destroy()
    {
        base.Destroy();

        Sound.Play(Assets.GetAsset("explosion.wav"));

        Entity newEntity = new("Death Explosion Particles")
        { Position = Position };
        newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.SmallExplosion) { Priority = Depths.EFFECT });
        Game.Instance.Scene.AddEntity(newEntity);
    }

    public override void OnRpc(MessageRpc message)
    {
        switch (message.RpcKind)
        {
            case RpcMessages.Kind.Shoot:
            {
                if (!IsOwned)
                {
                    RpcMessages.Shoot2 data = message.GetObjectData<RpcMessages.Shoot2>();
                    Shoot(data.Origin, data.Direction, data.Speed);
                }
                break;
            }
            case RpcMessages.Kind.Damage:
            {
                RpcMessages.Damaged data = message.GetObjectData<RpcMessages.Damaged>();
                Damage(data.Amount, data.By);
                break;
            }
        }
    }

    public override void Update()
    {
        if (!IsOwned) return;

        if (Ammo > 0 && Reload <= 0f)
        {
            if (Target == null || Target.IsDestroyed)
            {
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Enemy, VisionRadius);
            }
            else
            {
                Vector2 diff = Target.Position - Position;
                Vector2 direction = Vector2.Normalize(diff);
                float speed = Math.Min(ProjectileSpeed, Acceleration.RequiredSpeedToReachDistance(GrenadeBehavior.Acceleration, (float)diff.Length()) ?? ProjectileSpeed);

                Rotation.RotateByDeg(ref direction, Random.Float(-1f, 1f));

                SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot2(Position, direction, speed));

                Shoot(Position, direction, speed);
            }
        }

        if (Ammo <= 0 && Renderer != null) Renderer.Color = CharColor.Silver;

        if (Reload > 0f)
        { Reload -= Time.DeltaTime; }
    }

    void Shoot(Vector2 origin, Vector2 direction, float speed)
    {
        if (NetworkEntity.IsOwned) SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot2(origin, direction, speed));

        Entity grenade = new("Grenade");
        grenade.SetComponents(
                new RendererComponent(grenade)
                {
                    Color = CharColor.Silver,
                    Character = '§',
                    Priority = Depths.PROJECTILE,
                },
                new GrenadeBehavior(grenade)
                {
                    Velocity = direction * speed,
                    Owner = this,
                }
            );
        grenade.Position = Position;
        Game.Instance.Scene.AddEntity(grenade);

        Entity effect = new("Shoot Particles");
        effect.SetComponents(new ParticlesRendererComponent(effect, new ParticlesConfig(PredefinedEffects.Shoot) { Direction = direction }) { Priority = Depths.EFFECT });
        effect.Position = Position + direction;
        Game.Instance.Scene.AddEntity(effect);

        Reload = ReloadTime;
        Ammo--;
    }

    public void Damage(float amount, Component? by)
    {
        DamageableRenderer?.OnDamage();

        if (Game.NetworkMode == NetworkMode.Client) return;

        SendRpc(RpcMessages.Kind.Damage, new RpcMessages.Damaged(amount, by));

        Health -= amount;
        if (Health <= 0f)
        {
            IsDestroyed = true;
        }
    }

    public void RenderHoverPopup(RectInt content)
    {
        Game.Renderer.Text(content.X, content.Y, "Turret2", CharColor.Silver);
        Game.Renderer.Text(content.X, content.Y + 1, "♥:", CharColor.Silver);
        float health = Health / MaxHealth * (content.Width - 4);

        for (int x = 0; x < content.Width - 4; x++)
        {
            if (health > x)
            {
                Game.Renderer.Set(x + content.X + 3, content.Y + 1, new ConsoleChar(Ascii.Blocks.Full, CharColor.BrightRed, CharColor.Gray));
            }
            else if (health <= x)
            {
                Game.Renderer.Set(x + content.X + 3, content.Y + 1, new ConsoleChar(' ', CharColor.BrightRed, CharColor.Gray));
            }
        }

        Game.Renderer.Text(content.X, content.Y + 2, "∆:", CharColor.Silver);

        Game.Renderer.Text(content.X + 3, content.Y + 2, Ammo.ToString(), CharColor.White);
    }
}
