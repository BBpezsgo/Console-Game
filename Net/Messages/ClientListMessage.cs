using ConsoleGame.Net;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ClientListMessage : Message, ISerializable<ClientListMessage>
    {
        public Socket[] Clients = Array.Empty<Socket>();

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            Clients = deserializer.DeserializeObjectArray<Socket>(INTEGER_TYPE.INT8);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize(Clients, (s, item) => s.Serialize(item), INTEGER_TYPE.INT8);
        }
    }
}
