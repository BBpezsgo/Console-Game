namespace ConsoleGame.Behavior;

public class HelperTurretBehavior : NetworkComponent, IDamageable, ICanDrawEntityHoverPopup
{
    float Reload;

    const float ReloadTime = .7f;
    const float ProjectileSpeed = 40f;

    const float VisionRadius = 15f;

    public const float MaxHealth = 10f;
    public float Health = MaxHealth;

    const int MaxAmmo = 20;

    int Ammo = MaxAmmo;

    Entity? Target;

    readonly DamageableRendererComponent? DamageableRenderer;
    readonly RendererComponent? Renderer;

    public HelperTurretBehavior(Entity entity) : base(entity)
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
                    RpcMessages.Shoot data = message.GetObjectData<RpcMessages.Shoot>();
                    Shoot(data.Origin, data.Direction);
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
                Target = Game.Instance.Scene.ClosestObject(Position, Tags.Enemy, VisionRadius, entity =>
                {
                    if (!entity.TryGetComponent(out IncomingProjectileCounter? incomingProjectileCounter) ||
                        !entity.TryGetComponent(out EnemyBehavior? enemy))
                    { return true; }

                    if (incomingProjectileCounter.EstimatedDamage <= enemy.Health + 0.1f)
                    { return true; }

                    return false;
                });
            }
            else
            {
                Target.TryGetComponent(out IncomingProjectileCounter? incomingProjectileCounter);
                Target.TryGetComponent(out EnemyBehavior? enemy);

                if (incomingProjectileCounter != null &&
                    enemy != null &&
                    incomingProjectileCounter.EstimatedDamage > enemy.Health)
                {
                    Target = null;
                    return;
                }

                Vector2 direction = Vector2.Normalize(Target.Position - Position);

                incomingProjectileCounter?.OnShot(new IncomingProjectileCounter.IncomingProjectile(Entity, Position, Time.Now, ProjectileBehavior.Damage, ProjectileSpeed));

                SendRpcImmediate(1, new RpcMessages.Shoot(Position, direction));

                Shoot(Position, direction);
            }
        }

        if (Ammo <= 0 && Renderer != null) Renderer.Color = CharColor.Silver;

        if (Reload > 0f)
        { Reload -= Time.DeltaTime; }
    }

    void Shoot(Vector2 origin, Vector2 direction)
    {
        if (NetworkEntity.IsOwned) SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot(origin, direction));

        Sound.Play(Assets.GetAsset("laserShoot.wav"));

        Entity projectile = new("Turret Projectile");
        projectile.SetComponents(
                new RendererComponent(projectile)
                {
                    Color = CharColor.BrightYellow,
                    Character = '.',
                    Priority = Depths.PROJECTILE,
                },
                new ProjectileBehavior(projectile)
                {
                    Velocity = direction * ProjectileSpeed,
                    Owner = this,
                }
            );
        projectile.Position = Position;
        Game.Instance.Scene.AddEntity(projectile);

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
        Game.Renderer.Text(content.X, content.Y, "Turret", CharColor.Silver);
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
