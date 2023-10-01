﻿using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class ObjectSpawnMessage : Message, ISerializable<ObjectSpawnMessage>
    {
        public int NetworkId;
        public Vector Position;
        public int ObjectId;
        public ulong OwnerId;

        public ObjectSpawnMessage() : base()
        {
            Type = MessageType.OBJ_SPAWN;
        }

        public ObjectSpawnMessage(NetworkedGameObject @object) : base()
        {
            Type = MessageType.OBJ_SPAWN;
            NetworkId = @object.NetworkId;
            Position = @object.Position;
            ObjectId = @object.ObjectId;
            OwnerId = (ulong)@object.Owner;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            NetworkId = deserializer.DeserializeInt32();
            Position = deserializer.DeserializeObject<Vector>();
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