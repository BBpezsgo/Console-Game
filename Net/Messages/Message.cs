using ConsoleGame.Net;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class Message : ISerializable<Message>
    {
        public MessageType Type;
        public uint GUID;

        public Message()
        {

        }

        public virtual void Deserialize(Deserializer deserializer)
        {
            Type = (MessageType)deserializer.DeserializeByte();
            GUID = deserializer.DeserializeUInt32();
        }

        public virtual void Serialize(Serializer serializer)
        {
            serializer.Serialize((byte)Type);
            serializer.Serialize(GUID);
        }

        public static Message DeserializeMessage(Deserializer deserializer)
        {
            MessageType type = (MessageType)deserializer.Peek();
            return type switch
            {
                MessageType.OBJ_POSITION => deserializer.DeserializeObject<ObjectPositionMessage>(),
                MessageType.OBJ_REQUEST => deserializer.DeserializeObject<ObjectRequestMessage>(),
                MessageType.OBJ_DETAILS => deserializer.DeserializeObject<ObjectDetailsMessage>(),
                MessageType.OBJ_RPC => deserializer.DeserializeObject<MessageRpc>(),
                MessageType.CONTROL => deserializer.DeserializeObject<NetControlMessage>(),
                MessageType.OBJ_SPAWN => deserializer.DeserializeObject<ObjectSpawnMessage>(),
                MessageType.CLIENT_LIST_REQUEST => deserializer.DeserializeObject<ClientListRequestMessage>(),
                MessageType.CLIENT_LIST => deserializer.DeserializeObject<ClientListMessage>(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
