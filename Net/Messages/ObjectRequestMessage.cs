using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ObjectRequestMessage : Message, ISerializable<ObjectRequestMessage>
    {
        public int NetworkId;

        public ObjectRequestMessage() : base()
        {
            Type = MessageType.OBJ_REQUEST;
        }

        public ObjectRequestMessage(int networkId) : base()
        {
            Type = MessageType.OBJ_REQUEST;
            NetworkId = networkId;
        }

        public ObjectRequestMessage(NetworkEntityComponent @object) : base()
        {
            Type = MessageType.OBJ_REQUEST;
            NetworkId = @object.NetworkId;
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
