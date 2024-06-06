namespace ConsoleGame;

public class ComponentMessage : ObjectMessage
{
    public int ComponentIndex;

    public ComponentMessage()
    {
        ComponentIndex = -1;
    }

    public ComponentMessage(NetworkComponent sender) : base(sender.Entity.GetComponent<NetworkEntityComponent>())
    {
        ComponentIndex = sender.ComponentIndex;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write((byte)ComponentIndex);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        ComponentIndex = reader.ReadByte();
    }
}
