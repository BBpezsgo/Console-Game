using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ComponentMessage : ObjectMessage, ISerializable<ComponentMessage>
    {
        public int ComponentIndex;

        public ComponentMessage() : base()
        {
            ComponentIndex = -1;
        }

        public ComponentMessage(NetworkComponent sender) : base(sender.Entity.GetComponent<NetworkEntityComponent>())
        {
            ComponentIndex = sender.ComponentIndex;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            ComponentIndex = deserializer.DeserializeByte();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize((byte)ComponentIndex);
        }
    }
}
