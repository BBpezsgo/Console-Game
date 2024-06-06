using System.Net;
using System.Text;

namespace ConsoleGame.Net;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct Socket : ISerializable, IEquatable<Socket>
{
    public IPAddress Address;
    public ushort Port;

    public Socket(IPAddress address, ushort port)
    {
        Address = address;
        Port = port;
    }

    public Socket(EndPoint endPoint)
    {
        if (endPoint is not IPEndPoint ipEndPoint) throw new Exception();
        Address = ipEndPoint.Address;
        Port = (ushort)ipEndPoint.Port;
    }

    public readonly void Serialize(BinaryWriter serializer)
    {
        serializer.Write(Address);
        serializer.Write(Port);
    }

    public void Deserialize(BinaryReader deserializer)
    {
        Address = deserializer.ReadIPAddress();
        Port = deserializer.ReadUInt16();
    }

    public override readonly bool Equals(object? obj) =>
        obj is Socket socket &&
        Equals(socket);
    public readonly bool Equals(Socket other) =>
        Address.Equals(other.Address) &&
        Port == other.Port;

    public override readonly int GetHashCode() => HashCode.Combine(Address, Port);

    public static bool operator ==(Socket left, Socket right) => left.Equals(right);
    public static bool operator !=(Socket left, Socket right) => !(left == right);

    public override readonly string ToString() => $"{Address}:{Port}";
    readonly string GetDebuggerDisplay() => ToString();

    public static explicit operator IPEndPoint(Socket socket) => new(socket.Address, socket.Port);
    public static explicit operator Socket(IPEndPoint? socket) => new(socket ?? throw new ArgumentNullException(nameof(socket)));
    public static explicit operator Socket(EndPoint? socket) => new(socket ?? throw new ArgumentNullException(nameof(socket)));

    public readonly string Simplify()
    {
        StringBuilder result = new();
        if (Address.Equals(IPAddress.Any))
        { result.Append('*'); }
        else
        { result.Append(Address.ToString()); }
        result.Append(':');
        result.Append(Port);
        return result.ToString();
    }

    public static bool TryParse(string text, out Socket socket)
    {
        socket = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (!text.Contains(':')) return false;
        string addressString = text.Split(':')[0];
        string portString = text.Split(':')[1];
        IPAddress? address;
        if (addressString == "*")
        {
            address = IPAddress.Any;
        }
        else if (!IPAddress.TryParse(addressString, out address))
        {
            return false;
        }

        if (!ushort.TryParse(portString, out ushort port))
        {
            return false;
        }

        socket = new Socket(address, port);
        return true;
    }
}
