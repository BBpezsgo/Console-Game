namespace ConsoleGame
{
    using Net;

    public class NetworkedGameObject : GameObject
    {
        public readonly int NetworkId;
        public readonly int ObjectId;
        public readonly NetworkPlayer Owner;

        protected Vector NetPosition;

        public bool IsOwned => Game.NetworkMode == NetworkMode.Offline || Owner == new NetworkPlayer(Game.Connection?.LocalEndPoint ?? throw new NullReferenceException());

        public NetworkedGameObject(Vector position, int networkId, int objectId, NetworkPlayer owner) : base(position)
        {
            NetworkId = networkId;
            ObjectId = objectId;
            NetPosition = position;
            Owner = owner;
        }

        public NetworkedGameObject(Vector position, Vector speed, int networkId, int objectId, NetworkPlayer owner) : base(position, speed)
        {
            NetworkId = networkId;
            ObjectId = objectId;
            NetPosition = position;
            Owner = owner;
        }

        public virtual void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Server)
            {
                socket.Send(new ObjectPositionMessage()
                {
                    Type = MessageType.OBJ_POSITION,
                    Position = Position,
                    NetworkId = NetworkId,
                });
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (Game.NetworkMode != NetworkMode.Client) return;

            Position += Vector.MoveTowards(Position, NetPosition, Game.DeltaTime);
        }

        public void OnMessageReceived(ObjectMessage message)
        {
            if (message is ObjectPositionMessage positionMessage)
            {
                NetPosition = positionMessage.Position;
            }
        }

        public virtual void OnRpc(MessageRpc message) { }
    }
}
