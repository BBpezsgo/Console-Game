using System.Diagnostics.CodeAnalysis;
using DataUtilities.ReadableFileFormat;

namespace ConsoleGame;

public static class Files
{
    public static bool TryLoadAnyDataFile<T>(string name, [NotNullWhen(true)] out T? data) where T : ISerializable, IDeserializableText
    {
        if (Files.TryLoadBinaryFile<T>(name, out data))
        { return true; }

        if (Files.TryLoadTextFile<T>(name, out data))
        { return true; }

        return false;
    }

    public static void SaveBinary<T>(string name, T data) where T : ISerializable
    {
        File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, $"{name}.bin"), Serializing.Serialize(data));
    }
    public static void SaveText<T>(string name, T data) where T : ISerializableText
    {
        Value serialized = ((ISerializableText)data).SerializeText();
        string txt = serialized.ToSDF(false);
        File.WriteAllText(Path.Combine(Environment.CurrentDirectory, $"{name}.dat"), txt);
    }
    public static void SaveJson<T>(string name, T data) where T : ISerializableText
    {
        Value serialized = ((ISerializableText)data).SerializeText();
        string txt = serialized.ToJSON(false);
        File.WriteAllText(Path.Combine(Environment.CurrentDirectory, $"{name}.json"), txt);
    }

    public static bool TryLoadBinaryFile<T>(string name, [NotNullWhen(true)] out T? data) where T : ISerializable
    {
        data = default;
        if (!TryLoadBinaryFile(name, out byte[]? bin)) return false;

        try
        {
            data = Serializing.Deserialize<T>(bin);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public static bool TryLoadBinaryFile(string name, [NotNullWhen(true)] out byte[]? data)
    {
        string path = Path.Combine(Environment.CurrentDirectory, $"{name}.bin");

        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        data = File.ReadAllBytes(path);
        return true;
    }

    public static bool TryLoadTextFile<T>(string name, [NotNullWhen(true)] out T? data) where T : IDeserializableText
    {
        data = default;
        if (!TryLoadTextFile(name, out string? txt)) return false;

        try
        {
            data = Parser.Parse(txt).Object<T>();
            return data is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public static bool TryLoadTextFile(string name, [NotNullWhen(true)] out string? data)
    {
        string path = Path.Combine(Environment.CurrentDirectory, $"{name}.dat");

        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        data = File.ReadAllText(path);
        return true;
    }

    public static bool TryLoadJsonFile<T>(string name, [NotNullWhen(true)] out T? data) where T : IDeserializableText
    {
        data = default;
        if (!TryLoadJsonFile(name, out string? txt)) return false;

        try
        {
            data = DataUtilities.Json.Parser.Parse(txt).Object<T>();
            return data is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public static bool TryLoadJsonFile(string name, [NotNullWhen(true)] out string? data)
    {
        string path = Path.Combine(Environment.CurrentDirectory, $"{name}.json");

        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        data = File.ReadAllText(path);
        return true;
    }
}
