using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ObjectDestroyMessage : ObjectMessage, ISerializable<ObjectDestroyMessage>
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
