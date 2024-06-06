namespace ConsoleGame;

public class Message : ISerializable
{
    public MessageType Type;
    public uint GUID;

    public virtual void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(GUID);
    }

    public virtual void Deserialize(BinaryReader reader)
    {
        Type = (MessageType)reader.ReadByte();
        GUID = reader.ReadUInt32();
    }

    public static Message DeserializeMessage(BinaryReader reader)
    {
        MessageType type = (MessageType)reader.ReadByte();
        reader.BaseStream.Position--;
        return type switch
        {
            MessageType.OBJ_POSITION => reader.Read<ObjectPositionMessage>(),
            MessageType.OBJ_REQUEST => reader.Read<ObjectRequestMessage>(),
            MessageType.OBJ_DETAILS => reader.Read<ObjectDetailsMessage>(),
            MessageType.OBJ_RPC => reader.Read<MessageRpc>(),
            MessageType.CONTROL => reader.Read<NetControlMessage>(),
            MessageType.OBJ_SPAWN => reader.Read<ObjectSpawnMessage>(),
            MessageType.CLIENT_LIST_REQUEST => reader.Read<ClientListRequestMessage>(),
            MessageType.CLIENT_LIST => reader.Read<ClientListMessage>(),
            MessageType.OBJ_DESTROY => reader.Read<ObjectDestroyMessage>(),
            MessageType.REQ_RESPAWN => reader.Read<RespawnRequestMessage>(),
            _ => throw new NotImplementedException(),
        };
    }
}
