namespace ConsoleGame
{
    using Net;

    public class Player : NetworkedGameObject
    {
        float Reload = 0f;

        const float MaxSpeed = 3f;
        const float ReloadTime = .2f;
        const float ProjectileSpeed = 40f;

        public Player(Vector position, int networkId, int objectId, NetworkPlayer owner)
            : base(position, networkId, objectId, owner)
        {

        }

        public Player(Vector position, Vector speed, int networkId, int objectId, NetworkPlayer owner)
            : base(position, speed, networkId, objectId, owner)
        {

        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Offline) return;
            if (!IsOwned) return;

            socket.Send(new ObjectPositionMessage()
            {
                Type = MessageType.OBJ_POSITION,
                Position = Position,
                NetworkId = NetworkId,
            });
        }

        public override void Render()
        {
            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = ByteColor.BrightMagenta;
            Game.Renderer[Game.WorldToConsole(Position)].Char = 'O';
        }

        public override void Tick()
        {
            if (!IsOwned)
            {
                Position += Vector.MoveTowards(Position, NetPosition, Game.DeltaTime);

                if ((Position - NetPosition).SqrMagnitude > MaxSpeed * Game.NetworkTickRate + .5f)
                { Position = NetPosition; }
            }
            else
            {
                if (Keyboard.IsKeyPressed('W'))
                {
                    Position.Y -= Game.DeltaTime * MaxSpeed;
                }

                if (Keyboard.IsKeyPressed('A'))
                {
                    Position.X -= Game.DeltaTime * MaxSpeed;
                }

                if (Keyboard.IsKeyPressed('S'))
                {
                    Position.Y += Game.DeltaTime * MaxSpeed;
                }

                if (Keyboard.IsKeyPressed('D'))
                {
                    Position.X += Game.DeltaTime * MaxSpeed;
                }

                WorldBorders.Clamp(Game.Instance.Scene.Size, ref Position);

                if (Reload <= 0f && Mouse.IsLeftDown)
                {
                    Vector direction = (Mouse.WorldPosition - Position).Normalized;

                    if (Game.NetworkMode != NetworkMode.Offline)
                    { Game.Connection?.Send(MessageRpc.Make(this, 1, direction)); }

                    Game.Instance.Scene.AddObject(new Projectile(Position, direction * ProjectileSpeed));
                    Reload = ReloadTime;
                }

                if (Reload > 0f)
                { Reload -= Game.DeltaTime; }
            }
        }

        public override void OnRpc(MessageRpc message)
        {
            switch (message.RpcKind)
            {
                case 1:
                    {
                        if (!IsOwned)
                        {
                            Vector direction = message.GetObjectData<Vector>();
                            Game.Instance.Scene.AddObject(new Projectile(NetPosition, direction * ProjectileSpeed));
                        }
                        break;
                    }
                default:

                    break;
            }
        }
    }
}
