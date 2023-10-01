using DataUtilities.Serializer;

namespace ConsoleGame
{
    public enum NetControlMessageKind : byte
    {
        HEY_IM_CLIENT,
    }

    public class NetControlMessage : Message, ISerializable<NetControlMessage>
    {
        public NetControlMessageKind Kind;

        public NetControlMessage(NetControlMessageKind kind) : base()
        {
            Type = MessageType.CONTROL;
            Kind = kind;
        }

        public NetControlMessage() : base()
        {
            Type = MessageType.CONTROL;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            Kind = (NetControlMessageKind)deserializer.DeserializeByte();
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            serializer.Serialize((byte)Kind);
        }

        /*
        public IPEndPoint? Sender;

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            byte[] addressBytes = deserializer.DeserializeArray<byte>(INTEGER_TYPE.INT8);
            ushort port = deserializer.DeserializeUInt16();

            IPAddress address = new(addressBytes);

            Sender = new(address, port);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            if (Sender == null) throw new NullReferenceException(nameof(Sender));

            byte[] addressBytes = Sender.Address.GetAddressBytes();

            serializer.Serialize(addressBytes, INTEGER_TYPE.INT8);
            serializer.Serialize((ushort)Sender.Port);
        }
        */
    }
}
