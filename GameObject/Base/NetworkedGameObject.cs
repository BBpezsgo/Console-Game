namespace ConsoleGame
{
    using DataUtilities.Serializer;
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

        public override void OnDestroy()
        {
            if (Game.NetworkMode == NetworkMode.Client) return;
            Game.Connection?.Send(new ObjectDestroyMessage()
            {
                Type = MessageType.OBJ_DESTROY,
                NetworkId = NetworkId,
            });
        }

        protected void SendRpc<T>(int rpcKind, T data, Action<T, Serializer> serializer)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Serializer _serializer = new();
            serializer.Invoke(data, _serializer);
            SendRpc(rpcKind, _serializer.Result);
        }
        protected void SendRpc<T>(int rpcKind, ISerializable<T> data)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Serializer serializer = new();
            data.Serialize(serializer);
            SendRpc(rpcKind, serializer.Result);
        }
        protected void SendRpc(int rpcKind, params byte[] data)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Game.Connection?.Send(new MessageRpc()
            {
                Type = MessageType.OBJ_RPC,
                NetworkId = NetworkId,
                RpcKind = rpcKind,
                Data = data,
            });
        }

        protected void SendRpcImmediate<T>(int rpcKind, T data, Action<Serializer, T> serializer)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Serializer _serializer = new();
            serializer.Invoke(_serializer, data);
            SendRpcImmediate(rpcKind, _serializer.Result);
        }
        protected void SendRpcImmediate<T>(int rpcKind, ISerializable<T> data)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Serializer serializer = new();
            data.Serialize(serializer);
            SendRpcImmediate(rpcKind, serializer.Result);
        }
        protected void SendRpcImmediate(int rpcKind, params byte[] data)
        {
            if (Game.NetworkMode == NetworkMode.Offline) return;

            Game.Connection?.SendImmediate(new MessageRpc()
            {
                Type = MessageType.OBJ_RPC,
                NetworkId = NetworkId,
                RpcKind = rpcKind,
                Data = data,
            });
        }
    }
}
