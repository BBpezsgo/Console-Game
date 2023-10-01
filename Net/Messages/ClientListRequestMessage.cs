using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ClientListRequestMessage : Message, ISerializable<ClientListRequestMessage>
    {
        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
        }
    }
}
