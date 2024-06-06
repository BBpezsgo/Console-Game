namespace ConsoleGame;

public class ObjectRequestMessage : Message
{
    public int NetworkId;

    public ObjectRequestMessage()
    {
        Type = MessageType.OBJ_REQUEST;
    }

    public ObjectRequestMessage(int networkId)
    {
        Type = MessageType.OBJ_REQUEST;
        NetworkId = networkId;
    }

    public ObjectRequestMessage(NetworkEntityComponent @object)
    {
        Type = MessageType.OBJ_REQUEST;
        NetworkId = @object.NetworkId;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(NetworkId);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        NetworkId = reader.ReadInt32();
    }
}
