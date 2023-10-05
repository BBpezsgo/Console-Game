using System.Diagnostics;
using System.Net;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct ObjectOwner : IEquatable<ObjectOwner>
    {
        readonly ulong Id;

        public ObjectOwner(IPEndPoint? endPoint)
        {
            if (endPoint == null)
            {
                Id = 0;
                return;
            }
            IPAddress address = endPoint.Address;
            byte[] addressBytes = address.GetAddressBytes();
            if (addressBytes.Length != 4)
            { throw new NotImplementedException(); }
            ushort port = (ushort)endPoint.Port;
            byte[] portBytes = BitConverter.GetBytes(port);
            byte[] result = new byte[8];
            Array.Copy(addressBytes, 0, result, 2, 4);
            Array.Copy(portBytes, 0, result, 6, 2);
            Id = BitConverter.ToUInt64(result, 0);
        }

        public ObjectOwner(Net.Socket endPoint)
        {
            IPAddress address = endPoint.Address;
            byte[] addressBytes = address.GetAddressBytes();
            if (addressBytes.Length != 4)
            { throw new NotImplementedException(); }
            ushort port = endPoint.Port;
            byte[] portBytes = BitConverter.GetBytes(port);
            byte[] result = new byte[8];
            Array.Copy(addressBytes, 0, result, 2, 4);
            Array.Copy(portBytes, 0, result, 6, 2);
            Id = BitConverter.ToUInt64(result, 0);
        }

        ObjectOwner(ulong v)
        {
            Id = v;
        }

        public override bool Equals(object? obj) =>
            obj is ObjectOwner player &&
            Equals(player);
        public bool Equals(ObjectOwner other) => Id == other.Id;
        public override int GetHashCode() => HashCode.Combine(Id);

        public static bool operator ==(ObjectOwner left, ObjectOwner right) => left.Equals(right);
        public static bool operator !=(ObjectOwner left, ObjectOwner right) => !left.Equals(right);

        public override string ToString() => Id.ToString();
        private string GetDebuggerDisplay() => ToString();

        public static explicit operator ulong(ObjectOwner v) => v.Id;
        public static explicit operator ObjectOwner(ulong v) => new(v);
    }
}
