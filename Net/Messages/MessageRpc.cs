using DataUtilities.Serializer;

namespace ConsoleGame
{
    public class MessageRpc : ComponentMessage, ISerializable<MessageRpc>
    {
        public int RpcKind;
        public byte[] Data = Array.Empty<byte>();

        public MessageRpc() : base()
        {

        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            RpcKind = deserializer.DeserializeInt32();
            Data = deserializer.DeserializeArray<byte>();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize(RpcKind);
            serializer.Serialize(Data);
        }

        public T GetObjectData<T>() where T : ISerializable<T>
        {
            Deserializer deserializer = new(Data);
            return deserializer.DeserializeObject<T>();
        }

        public T GetObjectData<T>(Func<Deserializer, T> deserializer)
        {
            Deserializer _deserializer = new(Data);
            return deserializer.Invoke(_deserializer);
        }

        public T GetData<T>()
        {
            Deserializer deserializer = new(Data);
            return deserializer.Deserialize<T>();
        }

        public static MessageRpc Make<T>(NetworkComponent sender, int kind, T data) where T : ISerializable<T>
        {
            Serializer serializer = new();
            serializer.Serialize(data);
            return new MessageRpc()
            {
                Type = MessageType.OBJ_RPC,
                NetworkId = sender.NetworkId,
                RpcKind = kind,
                Data = serializer.Result,
            };
        }

        public static MessageRpc Make<T>(NetworkComponent sender, int kind, T data, Action<T, Serializer> serializer)
        {
            Serializer _serializer = new();
            serializer.Invoke(data, _serializer);
            return new MessageRpc()
            {
                Type = MessageType.OBJ_RPC,
                NetworkId = sender.NetworkId,
                RpcKind = kind,
                Data = _serializer.Result,
            };
        }
    }
}
