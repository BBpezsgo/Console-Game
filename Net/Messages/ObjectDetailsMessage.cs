namespace ConsoleGame;

public class ObjectSpawnMessage : Message
{
    public int NetworkId;
    public Vector2 Position;
    public int ObjectId;
    public ulong OwnerId;

    public ObjectSpawnMessage()
    {
        Type = MessageType.OBJ_SPAWN;
    }

    public ObjectSpawnMessage(NetworkEntityComponent networkEntity)
    {
        Type = MessageType.OBJ_SPAWN;

        NetworkId = networkEntity.NetworkId;
        OwnerId = (ulong)networkEntity.Owner;
        ObjectId = networkEntity.ObjectId;
        Position = networkEntity.Position;
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
