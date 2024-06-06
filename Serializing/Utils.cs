using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ConsoleGame;

public interface ISerializable
{
    public void Serialize(BinaryWriter writer);
    public void Deserialize(BinaryReader reader);
}

public enum BitWidth
{
    _32,
    _16,
    _8,
}

public static class Serializing
{
    public static byte[] Serialize(ISerializable serializable)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);
        serializable.Serialize(writer);
        return memoryStream.ToArray();
    }

    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(byte[] buffer)
        where T : ISerializable
    {
        T result = Activator.CreateInstance<T>();
        using MemoryStream memoryStream = new(buffer);
        using BinaryReader reader = new(memoryStream);
        result.Deserialize(reader);
        return result;
    }

    public static void Write<T>(this BinaryWriter writer, T data)
        where T : ISerializable
        => writer.Write(data);

    public static T Read<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(this BinaryReader reader)
        where T : ISerializable
    {
        T result = Activator.CreateInstance<T>();
        result.Deserialize(reader);
        return result;
    }

    public static void Write<T>(this BinaryWriter writer, T[] data, BitWidth bitWidth = BitWidth._16)
        where T : ISerializable
    {
        switch (bitWidth)
        {
            case BitWidth._32:
                writer.Write(data.Length);
                break;
            case BitWidth._16:
                writer.Write((ushort)data.Length);
                break;
            case BitWidth._8:
                writer.Write((byte)data.Length);
                break;
            default:
                throw new NotImplementedException();
        }

        for (int i = 0; i < data.Length; i++)
        {
            data[i].Serialize(writer);
        }
    }

    public static T[] ReadArray<T>(this BinaryReader reader, BitWidth bitWidth = BitWidth._16)
        where T : ISerializable
    {
        int length = bitWidth switch
        {
            BitWidth._32 => reader.ReadInt32(),
            BitWidth._16 => reader.ReadUInt16(),
            BitWidth._8 => reader.ReadByte(),
            _ => throw new NotImplementedException(),
        };

        T[] result = new T[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = Activator.CreateInstance<T>();
            result[i].Deserialize(reader);
        }
        return result;
    }

    public static void Write<T>(this BinaryWriter writer, T[] data, Action<BinaryWriter, T> serializer, BitWidth bitWidth = BitWidth._16)
    {
        switch (bitWidth)
        {
            case BitWidth._32:
                writer.Write(data.Length);
                break;
            case BitWidth._16:
                writer.Write((ushort)data.Length);
                break;
            case BitWidth._8:
                writer.Write((byte)data.Length);
                break;
            default:
                throw new NotImplementedException();
        }

        for (int i = 0; i < data.Length; i++)
        {
            serializer.Invoke(writer, data[i]);
        }
    }

    public static T[] ReadArray<T>(this BinaryReader reader, Action<BinaryReader, T> deserializer, BitWidth bitWidth = BitWidth._16)
    {
        int length = bitWidth switch
        {
            BitWidth._32 => reader.ReadInt32(),
            BitWidth._16 => reader.ReadUInt16(),
            BitWidth._8 => reader.ReadByte(),
            _ => throw new NotImplementedException(),
        };

        T[] result = new T[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = Activator.CreateInstance<T>();
            deserializer.Invoke(reader, result[i]);
        }
        return result;
    }

    public static T[] ReadArray<T>(this BinaryReader reader, Func<BinaryReader, T> deserializer, BitWidth bitWidth = BitWidth._16)
    {
        int length = bitWidth switch
        {
            BitWidth._32 => reader.ReadInt32(),
            BitWidth._16 => reader.ReadUInt16(),
            BitWidth._8 => reader.ReadByte(),
            _ => throw new NotImplementedException(),
        };

        T[] result = new T[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = deserializer.Invoke(reader);
        }
        return result;
    }

    public static void Write(this BinaryWriter writer, Vector2 vector2)
    {
        writer.Write(vector2.X);
        writer.Write(vector2.Y);
    }

    public static Vector2 ReadVector2(this BinaryReader reader) => new()
    {
        X = reader.ReadSingle(),
        Y = reader.ReadSingle()
    };

    public static void Write(this BinaryWriter writer, IPAddress address)
        => writer.Write(address.GetAddressBytes(), static (v1, v2) => v1.Write(v2), BitWidth._8);

    public static IPAddress ReadIPAddress(this BinaryReader reader)
        => new(reader.ReadArray(static v => v.ReadByte(), BitWidth._8));

    public static void WriteDirection(this BinaryWriter writer, Vector2 vec)
        => writer.Write((byte)MathF.Round(Rotation.ClampAngle(Rotation.ToDeg(vec)) * Rotation.Deg2Byte));

    public static Vector2 ReadDirection(this BinaryReader reader)
        => Rotation.FromDeg(reader.ReadByte() * Rotation.Byte2Deg);
}
