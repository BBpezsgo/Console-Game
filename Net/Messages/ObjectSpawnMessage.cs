namespace ConsoleGame;

public class ObjectDetailsMessage : Message
{
    public int NetworkId;
    public Vector2 Position;
    public int ObjectId;
    public ulong OwnerId;

    public ObjectDetailsMessage()
    {
        Type = MessageType.OBJ_DETAILS;
    }

    public ObjectDetailsMessage(NetworkEntityComponent @object)
    {
        Type = MessageType.OBJ_DETAILS;
        NetworkId = @object.NetworkId;
        // Position = @object.Position;
        ObjectId = @object.ObjectId;
        OwnerId = (ulong)@object.Owner;
    }

    public override void Serialize(BinaryWriter serializer)
    {
        base.Serialize(serializer);
        serializer.Write(NetworkId);
        serializer.Write(Position);
        serializer.Write(ObjectId);
        serializer.Write(OwnerId);
    }

    public override void Deserialize(BinaryReader deserializer)
    {
        base.Deserialize(deserializer);
        NetworkId = deserializer.ReadInt32();
        Position = deserializer.ReadVector2();
        ObjectId = deserializer.ReadInt32();
        OwnerId = deserializer.ReadUInt64();
    }
}
