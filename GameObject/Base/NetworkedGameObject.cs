namespace ConsoleGame
{
    using Net;

    public class NetworkedGameObject : GameObject
    {
        public readonly int NetworkId;
        public readonly int ObjectId;
        public readonly ObjectOwner Owner;

        protected Vector NetPosition;

        public bool IsOwned => Game.NetworkMode == NetworkMode.Offline || Owner == new ObjectOwner(Game.Connection?.LocalEndPoint ?? throw new NullReferenceException());

        public NetworkedGameObject(Vector position, int networkId, int objectId, ObjectOwner owner) : base()
        {
            NetworkId = networkId;
            ObjectId = objectId;
            Owner = owner;
            Position = position;
            NetPosition = position;
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
            if (Game.NetworkMode == NetworkMode.Client)
            { Position += Vector.MoveTowards(Position, NetPosition, Game.DeltaTime); }
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
