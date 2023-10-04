using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ObjectMessage : Message, ISerializable<ObjectMessage>
    {
        public int NetworkId;

        public ObjectMessage() : base() { }

        public ObjectMessage(NetworkEntityComponent sender) : base()
        {
            NetworkId = sender.NetworkId;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            NetworkId = deserializer.DeserializeInt32();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize(NetworkId);
        }
    }
}
