using ConsoleGame.Net;

namespace ConsoleGame;

public abstract class NetworkComponent : Component
{
    protected NetworkEntityComponent NetworkEntity;

    public bool IsOwned => NetworkEntity.IsOwned;

    public int NetworkId => NetworkEntity.NetworkId;
    public int ObjectId => NetworkEntity.ObjectId;
    public ObjectOwner Owner => NetworkEntity.Owner;

#pragma warning disable CS8618
    protected NetworkComponent(Entity entity) : base(entity) { }
#pragma warning restore CS8618

    public override void Make()
    {
        NetworkEntity = Entity.GetComponent<NetworkEntityComponent>();
        base.Make();
    }

    public virtual void Synchronize(NetworkMode mode, Connection socket) { }
    public virtual void OnMessage(ObjectMessage message) { }
    public virtual void OnRpc(MessageRpc message) { }

    protected void SendRpc<T>(int rpcKind, T data, Action<BinaryWriter, T> serializer)
    {
        if (Game.NetworkMode == NetworkMode.Offline) return;

        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        serializer.Invoke(writer, data);

        SendRpc(rpcKind, memoryStream.ToArray());
    }
    protected void SendRpc<T>(int rpcKind, T data)
        where T : ISerializable
    {
        if (Game.NetworkMode == NetworkMode.Offline) return;

        SendRpc(rpcKind, Serializing.Serialize(data));
    }
    protected void SendRpc(int rpcKind, byte[] data)
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

    protected void SendRpcImmediate<T>(int rpcKind, T data, Action<BinaryWriter, T> serializer)
    {
        if (Game.NetworkMode == NetworkMode.Offline) return;

        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        serializer.Invoke(writer, data);

        SendRpcImmediate(rpcKind, memoryStream.ToArray());
    }
    protected void SendRpcImmediate<T>(int rpcKind, T data)
        where T : ISerializable
    {
        if (Game.NetworkMode == NetworkMode.Offline) return;

        SendRpcImmediate(rpcKind, Serializing.Serialize(data));
    }
    protected void SendRpcImmediate(int rpcKind, byte[] data)
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
