namespace ConsoleGame;

public class MessageRpc : ComponentMessage
{
    public int RpcKind;
    public byte[] Data = Array.Empty<byte>();

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(RpcKind);
        writer.Write(Data);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        RpcKind = reader.ReadInt32();
        Data = reader.ReadArray(static v => v.ReadByte());
    }

    public T GetObjectData<T>() where T : ISerializable => Serializing.Deserialize<T>(Data);

    public static MessageRpc Make<T>(NetworkComponent sender, int kind, T data) where T : ISerializable => new()
    {
        Type = MessageType.OBJ_RPC,
        NetworkId = sender.NetworkId,
        RpcKind = kind,
        Data = Serializing.Serialize(data),
    };

    public static MessageRpc Make<T>(NetworkComponent sender, int kind, T data, Action<BinaryWriter, T> serializer)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);
        serializer.Invoke(writer, data);
        return new MessageRpc()
        {
            Type = MessageType.OBJ_RPC,
            NetworkId = sender.NetworkId,
            RpcKind = kind,
            Data = memoryStream.ToArray(),
        };
    }
}
