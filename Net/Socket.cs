using System.Diagnostics;
using System.Net;
using System.Text;
using DataUtilities.Serializer;

namespace ConsoleGame.Net
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Socket : ISerializable<Socket>, IEquatable<Socket>
    {
        public IPAddress Address;
        public ushort Port;

        public Socket(string? address, ushort port)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));
            Address = IPAddress.Parse(address);
            Port = port;
        }

        public Socket(IPAddress? address, ushort port)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));
            Address = address;
            Port = port;
        }

        public Socket(EndPoint? endPoint)
        {
            if (endPoint is null) throw new ArgumentNullException(nameof(endPoint));
            if (endPoint is not IPEndPoint ipEndPoint) throw new Exception();
            Address = ipEndPoint.Address;
            Port = (ushort)ipEndPoint.Port;
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Address.GetAddressBytes(), INTEGER_TYPE.INT8);
            serializer.Serialize(Port);
        }

        public void Deserialize(Deserializer deserializer)
        {
            Address = new IPAddress(deserializer.DeserializeArray<byte>(INTEGER_TYPE.INT8));
            Port = deserializer.DeserializeUInt16();
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
        public static explicit operator Socket(IPEndPoint? socket) => new(socket);
        public static explicit operator Socket(EndPoint? socket) => new(socket);

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
    }
}
