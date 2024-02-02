using System.Numerics;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ObjectDetailsMessage : Message, ISerializable<ObjectDetailsMessage>
    {
        public int NetworkId;
        public Vector2 Position;
        public int ObjectId;
        public ulong OwnerId;

        public ObjectDetailsMessage() : base()
        {
            Type = MessageType.OBJ_DETAILS;
        }

        public ObjectDetailsMessage(NetworkEntityComponent @object) : base()
        {
            Type = MessageType.OBJ_DETAILS;
            NetworkId = @object.NetworkId;
            // Position = @object.Position;
            ObjectId = @object.ObjectId;
            OwnerId = (ulong)@object.Owner;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            NetworkId = deserializer.DeserializeInt32();
            Position = deserializer.DeserializeVector2();
            ObjectId = deserializer.DeserializeInt32();
            OwnerId = deserializer.DeserializeUInt64();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize(NetworkId);
            serializer.Serialize(Position);
            serializer.Serialize(ObjectId);
            serializer.Serialize(OwnerId);
        }
    }
}
