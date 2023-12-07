using DataUtilities.ReadableFileFormat;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public struct PlayerData : ISerializable<PlayerData>, IFullySerializableText
    {
        public int Coins;

        public void Deserialize(Deserializer deserializer)
        {
            Coins = deserializer.DeserializeInt32();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Coins);
        }

        void IDeserializableText.DeserializeText(Value data)
        {
            Coins = data["Score"].Int ?? 0;
        }

        readonly Value ISerializableText.SerializeText()
        {
            Value result = Value.Object();

            result["Score"] = Coins;

            return result;
        }
    }
}
