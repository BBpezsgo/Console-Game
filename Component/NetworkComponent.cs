using ConsoleGame.Net;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public abstract class NetworkComponent : Component
    {
        protected NetworkEntityComponent NetworkEntity;

        public bool IsOwned => NetworkEntity.IsOwned;

        public int NetworkId => NetworkEntity.NetworkId;
        public int ObjectId => NetworkEntity.ObjectId;
        public ObjectOwner Owner => NetworkEntity.Owner;

        public NetworkComponent(Entity entity) : base(entity)
        {
            NetworkEntity = entity.GetComponent<NetworkEntityComponent>();
        }

        public virtual void Synchronize(NetworkMode mode, Connection socket) { }
        public virtual void OnMessage(ObjectMessage message) { }
        public virtual void OnRpc(MessageRpc message) { }


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
                NetworkId = NetworkEntity.NetworkId,
                RpcKind = rpcKind,
                Data = data,
                ComponentIndex = ComponentIndex,
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
                NetworkId = NetworkEntity.NetworkId,
                RpcKind = rpcKind,
                Data = data,
                ComponentIndex = ComponentIndex,
            });
        }
    }
}
