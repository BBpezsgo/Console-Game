using ConsoleGame.Net;

namespace ConsoleGame;

public class ClientListMessage : Message
{
    public Socket[] Clients = Array.Empty<Socket>();

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Clients, BitWidth._8);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        Clients = reader.ReadArray<Socket>(BitWidth._8);
    }
}
