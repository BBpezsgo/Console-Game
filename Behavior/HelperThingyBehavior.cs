namespace ConsoleGame.Behavior;

internal class HelperThingyBehavior : NetworkComponent, IDamageable, ICanDrawEntityHoverPopup, ICanPickUpItem
{
    float Reload;

    const float ReloadTime = 1.5f;
    const float ProjectileSpeed = 40f;

    const float FleeRadius = 5f;
    const float VisionRadius = 15f;
    const float ShootRadius = 10f;

    public const float MaxHealth = 5f;
    public float Health = MaxHealth;

    const float MaxSpeed = 1.5f;

    const int MaxAmmo = 10;
    int Ammo = MaxAmmo;

    Entity? PriorityTarget;
    Entity? Target;

    readonly DamageableRendererComponent? DamageableRenderer;
    readonly RendererComponent? Renderer;

    public HelperThingyBehavior(Entity entity) : base(entity)
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
        if (Game.NetworkMode == NetworkMode.Client)
        { return; }

        // if (!Entity.HandleCollisions())
        // Position += Vector.MoveTowards(Position, Mouse.WorldPosition, MaxSpeed * Time.DeltaTime);

        if (Ammo <= 0 && Renderer != null) Renderer.Color = CharColor.Silver;

        if (Reload > 0f)
        { Reload -= Time.DeltaTime; }

        Targeting(out bool canLoseTarget);
        TargetHandling(canLoseTarget);
        Entity.DoCollisions();
        Entity.ClampIntoWord();
    }

    void Targeting(out bool canLoseTarget)
    {
        canLoseTarget = true;

        if (Ammo <= 0)
        {
            Target = null;
            return;
        }

        if (Health < MaxHealth / 3f)
        {
            Target = Game.Instance.Scene.ClosestObject(Position, Tags.Item, VisionRadius * 10f);
            canLoseTarget = false;
            return;
        }

        if (Target != null) return;

        if (PriorityTarget != null)
        {
            if (PriorityTarget.IsDestroyed)
            {
                PriorityTarget = null;
            }
            else
            {
                Target = PriorityTarget;
                canLoseTarget = false;
                return;
            }
        }

        Target = Game.Instance.Scene.ClosestObject(Position, Tags.Enemy, VisionRadius);
    }

    void TargetHandling(bool canLoseTarget)
    {
        if (Target == null) return;

        if (Target.IsDestroyed ||
            (canLoseTarget && ((Target.Position - Position).LengthSquared() >= VisionRadius * VisionRadius)))
        {
            Target = null;
            return;
        }

        if (Target.TryGetComponent(out IDamageable? _))
        {
            float sqrMag = (Target.Position - Position).LengthSquared();
            if (sqrMag > ShootRadius * ShootRadius)
            {
                Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
            }
            else
            {
                if (Ammo > 0 && Reload <= 0f)
                {
                    Vector2 direction = Vector2.Normalize(Target.Position - Position);

                    SendRpcImmediate(1, new RpcMessages.Shoot(Position, direction));

                    Shoot(Position, direction);
                }

                if (sqrMag <= FleeRadius * FleeRadius)
                {
                    Position -= Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
                }
            }
        }
        else if (Target.TryGetComponent(out ItemBehavior? item))
        {
            float sqrMag = (Target.Position - Position).LengthSquared();
            if (sqrMag > 1f * 1f)
            {
                Position += Vector.MoveTowards(Position, Target.Position, MaxSpeed * Time.DeltaTime);
            }
            else
            {
                item.PickUp(this);
            }
        }
    }

    void Shoot(Vector2 origin, Vector2 direction)
    {
        if (NetworkEntity.IsOwned) SendRpcImmediate(RpcMessages.Kind.Shoot, new RpcMessages.Shoot(origin, direction));

        Sound.Play(Assets.GetAsset("laserShoot.wav"));

        Entity projectile = new("Helper Thingy Projectile");
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
        Game.Renderer.Text(content.X, content.Y, "Helper", CharColor.Silver);
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

    public void OnItemPickedUp(ItemBehavior.ItemKind kind, float amount)
    {
        switch (kind)
        {
            case ItemBehavior.ItemKind.Health:
                Health = Math.Min(Health + amount, MaxHealth);
                break;
        }
    }
}
