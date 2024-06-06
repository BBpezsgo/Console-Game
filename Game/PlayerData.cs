using DataUtilities.ReadableFileFormat;

namespace ConsoleGame;

public struct PlayerData : ISerializable, IFullySerializableText
{
    public int Coins;

    public readonly void Serialize(BinaryWriter serializer)
    {
        serializer.Write(Coins);
    }

    public void Deserialize(BinaryReader deserializer)
    {
        Coins = deserializer.ReadInt32();
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
