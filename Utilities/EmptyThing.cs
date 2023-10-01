using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class EmptyThing : ISerializable<EmptyThing>
    {
        public void Deserialize(Deserializer deserializer) { }
        public void Serialize(Serializer serializer) { }
    }
}
