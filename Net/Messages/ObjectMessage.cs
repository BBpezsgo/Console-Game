namespace ConsoleGame;

public class ObjectMessage : Message
{
    public int NetworkId;

    public ObjectMessage()
    {
        NetworkId = -1;
    }

    public ObjectMessage(NetworkEntityComponent sender)
    {
        NetworkId = sender.NetworkId;
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
