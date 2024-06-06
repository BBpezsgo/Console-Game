namespace ConsoleGame;

internal class ObjectPositionMessage : ComponentMessage
{
    public Vector2 Position;

    public override void Serialize(BinaryWriter serializer)
    {
        base.Serialize(serializer);
        serializer.Write(Position);
    }

    public override void Deserialize(BinaryReader deserializer)
    {
        base.Deserialize(deserializer);
        Position = deserializer.ReadVector2();
    }
}
