using System.Numerics;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    internal class ObjectPositionMessage : ComponentMessage, ISerializable<ObjectPositionMessage>
    {
        public Vector2 Position;

        public ObjectPositionMessage() : base() { }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            Position = deserializer.DeserializeVector2();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize(Position);
        }
    }
}
