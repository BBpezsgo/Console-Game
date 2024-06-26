namespace ConsoleGame.Behavior;

public class PlayerBehavior : NetworkComponent, IDamageable, ICanPickUpItem
{
    float Reload;
    float GrenadeReload;

    const float MaxSpeed = 3f;

    const float ReloadTime = .1f;
    const float GrenadeReloadTime = .5f;

    const float ProjectileSpeed = 40f;
    const float GrenadeSpeed = 15f;

    public const float MaxHealth = 10f;
    public float Health = MaxHealth;

    float DamageIndicator;

    DamageableRendererComponent3D? DamageableRenderer;

    public PlayerBehavior(Entity entity) : base(entity)
    {
        Entity.Tags |= Tags.Player;
    }

    public override void Make()
    {
        base.Make();
        DamageableRenderer = Entity.TryGetComponent<DamageableRendererComponent3D>();
    }

    public override void Update()
    {
        if (!IsOwned)
        { return; }

        const float blinkPerSec = 2f * 2;
        const float blinkingDuration = 1f;

        float damageIndicatorEffect = Time.Now - DamageIndicator;
        if (damageIndicatorEffect < blinkingDuration && (int)(damageIndicatorEffect * blinkPerSec) % 2 == 0)
        {
            // for (int y = 4; y < Game.Renderer.Height; y++)
            // {
            //     Game.Renderer[0, y].Background = CharColor.Red;
            //     Game.Renderer[Game.Renderer.Width - 1, y].Background = CharColor.Red;
            // }
            // for (int x = 0; x < Game.Renderer.Width; x++)
            // {
            //     Game.Renderer[x, 4].Background = CharColor.Red;
            //     Game.Renderer[x, Game.Renderer.Height - 1].Background = CharColor.Red;
            // }
        }

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Left))
        { this.Position.X += Time.DeltaTime * MaxSpeed; }
        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Right))
        { this.Position.X -= Time.DeltaTime * MaxSpeed; }

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Up))
        { this.Position.Y += Time.DeltaTime * MaxSpeed; }
        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Down))
        { this.Position.Y -= Time.DeltaTime * MaxSpeed; }

        Vector2 lookDir = Vector2.Normalize(new Vector2(Game.Instance.Scene.Camera.LookDirection.X, Game.Instance.Scene.Camera.LookDirection.Z));
        Vector3 lookDir2 = Vector3.Cross(Vector3.Normalize(Game.Instance.Scene.Camera.LookDirection), new Vector3(0f, 1f, 0f));

        if (DamageableRenderer is not null)
        {
            DamageableRenderer.Rotation = Maths.Matrix4x4.CreateRotationY(-Game.Instance.Scene.Camera.Yaw);
        }

        if (ConsoleKeyboard.IsKeyPressed('W'))
        { this.Position += lookDir * MaxSpeed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('S'))
        { this.Position -= lookDir * MaxSpeed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('A'))
        { this.Position -= new Vector2(lookDir2.X, lookDir2.Z) * MaxSpeed * Time.DeltaTime; }

        if (ConsoleKeyboard.IsKeyPressed('D'))
        { this.Position += new Vector2(lookDir2.X, lookDir2.Z) * MaxSpeed * Time.DeltaTime; }

        // if (Keyboard.IsKeyPressed('W') || Keyboard.IsKeyPressed(VirtualKeyCode.UP))
        // {
        //     Position.Y -= Time.DeltaTime * MaxSpeed;
        // }
        // 
        // if (Keyboard.IsKeyPressed('A') || Keyboard.IsKeyPressed(VirtualKeyCode.LEFT))
        // {
        //     Position.X -= Time.DeltaTime * MaxSpeed;
        // }
        // 
        // if (Keyboard.IsKeyPressed('S') || Keyboard.IsKeyPressed(VirtualKeyCode.DOWN))
        // {
        //     Position.Y += Time.DeltaTime * MaxSpeed;
        // }
        // 
        // if (Keyboard.IsKeyPressed('D') || Keyboard.IsKeyPressed(VirtualKeyCode.RIGHT))
        // {
        //     Position.X += Time.DeltaTime * MaxSpeed;
        // }

        WorldBorders.Clamp(Game.Instance.Scene.SizeR, ref Position);

        if (Reload <= 0f &&
            (ConsoleMouse.IsPressed(MouseButton.Left) || ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Space)) &&
            Game.Instance.HandleInput)
        {
            Shoot(Position, lookDir); // Rotation.RotateByDeg(Vector2.Normalize(Game.ConsoleToWorld(Mouse.RecordedConsolePosition) - Position), Random.Float(-2f, 2f)));
        }

        // if (Keyboard.IsKeyDown('X'))
        // {
        //     Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.HELPER_TURRET](Game.Instance.Scene.GenerateNetworkId(), Owner);
        //     newEntity.Position = Game.ConsoleToWorld(Mouse.RecordedConsolePosition);
        //     Game.Instance.Scene.AddEntity(newEntity);
        // }
        // 
        // if (Keyboard.IsKeyDown('V'))
        // {
        //     Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.HELPER_TURRET2](Game.Instance.Scene.GenerateNetworkId(), Owner);
        //     newEntity.Position = Game.ConsoleToWorld(Mouse.RecordedConsolePosition);
        //     Game.Instance.Scene.AddEntity(newEntity);
        // }
        // 
        // if (Keyboard.IsKeyDown('O'))
        // {
        //     Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.HELPER_THINGY](Game.Instance.Scene.GenerateNetworkId(), Owner);
        //     newEntity.Position = Game.ConsoleToWorld(Mouse.RecordedConsolePosition);
        //     Game.Instance.Scene.AddEntity(newEntity);
        // }

        // if (GrenadeReload <= 0f && Keyboard.IsKeyPressed('G'))
        // {
        //     Vector2 diff = Game.ConsoleToWorld(Mouse.RecordedConsolePosition) - Position;
        //     float speed = Math.Min(GrenadeSpeed, Acceleration.RequiredSpeedToReachDistance(GrenadeBehavior.Acceleration, diff.Length()) ?? GrenadeSpeed);
        //     Vector2 direction = Vector2.Normalize(diff);
        //     Rotation.RotateByDeg(ref direction, Random.Float(-1f, 1f));
        //     ShootGrenade(Position, direction, speed);
        // }

        if (ConsoleKeyboard.IsKeyDown('T'))
        {
            Damage(1f, null);
        }

        if (Game.NetworkMode != NetworkMode.Client)
        {
            ReadOnlySpan<Entity> coinsNearby = Game.Instance.Scene.ObjectsAt(Position, Tags.Item, .5f);
            for (int i = 0; i < coinsNearby.Length; i++)
            {
                if (!coinsNearby[i].TryGetComponent(out CoinItemBehavior? coin)) continue;
                coin.PickUp(this);
            }
        }

        if (Reload > 0f)
        { Reload -= Time.DeltaTime; }

        if (GrenadeReload > 0f)
        { GrenadeReload -= Time.DeltaTime; }
    }

    void Shoot(Vector2 origin, Vector2 direction)
    {
        if (NetworkEntity.IsOwned) SendRpcImmediate(1, new RpcMessages.Shoot(origin, direction));

        Sound.Play(Assets.GetAsset("laserShoot.wav"));

        Entity projectile = new("Player Projectile");
        projectile.SetComponents(
                // new RendererComponent(projectile)
                // {
                //     Color = CharColor.BrightYellow,
                //     Character = '.',
                //     Priority = Depths.PROJECTILE,
                // },
                new RendererComponent3D(projectile,
                    material =>
                    {
                        material.DiffuseColor = CharColor.To24bitColor(CharColor.BrightYellow);
                        material.AmbientColor = CharColor.To24bitColor(CharColor.BrightYellow) * .6f;
                    },
                    mesh => mesh.Scale(.5f)),
                new ProjectileBehavior(projectile)
                {
                    Velocity = direction * ProjectileSpeed,
                    Owner = this,
                }
            );
        projectile.Position = Position;
        Game.Instance.Scene.AddEntity(projectile);

        Entity effect = new("Shoot Effect");
        effect.SetComponents(new ParticlesRendererComponent3D(effect, new ParticlesConfig(PredefinedEffects.Shoot) { Direction = direction }));
        effect.Position = Position + direction;
        Game.Instance.Scene.AddEntity(effect);

        Reload = ReloadTime;
    }

    void ShootGrenade(Vector2 origin, Vector2 direction, float speed)
    {
        if (!NetworkEntity.IsOwned) return;

        Entity projectile = new("Player Grenade");
        projectile.AddComponent(new RendererComponent(projectile)
        {
            Color = CharColor.Silver,
            Character = '�',
            Priority = Depths.PROJECTILE,
        });
        projectile.AddComponent(new GrenadeBehavior(projectile)
        {
            Velocity = direction * speed,
            Owner = this,
        });
        projectile.Position = origin;
        Game.Instance.Scene.AddEntity(projectile);

        GrenadeReload = GrenadeReloadTime;
    }

    public override void OnRpc(MessageRpc message)
    {
        switch (message.RpcKind)
        {
            case RpcMessages.Kind.Shoot:
            {
                if (!NetworkEntity.IsOwned)
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
            default: break;
        }
    }

    public void Damage(float amount, Component? by)
    {
        DamageableRenderer?.OnDamage();
        DamageIndicator = Time.Now;

        if (Game.NetworkMode == NetworkMode.Client) return;

        SendRpc(RpcMessages.Kind.Damage, new RpcMessages.Damaged(amount, by));

        Health -= amount;
        if (Health <= 0f)
        {
            IsDestroyed = true;
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        Entity newEntity = new("Death Particles")
        { Position = Position };
        newEntity.SetComponents(new ParticlesRendererComponent3D(newEntity, PredefinedEffects.Death));
        Game.Instance.Scene.AddEntity(newEntity);
    }

    public void OnItemPickedUp(ItemBehavior.ItemKind kind, float amount)
    {
        switch (kind)
        {
            case ItemBehavior.ItemKind.Health:
                Health = Math.Min(Health + amount, MaxHealth);
                break;
            case ItemBehavior.ItemKind.Coin:
                if (IsOwned)
                { Game.Instance.PlayerData.Coins += (int)MathF.Round(amount); }
                break;
            default:
                break;
        }
    }
}
