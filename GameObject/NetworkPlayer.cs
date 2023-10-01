using System.Diagnostics;
using System.Net;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly struct NetworkPlayer : IEquatable<NetworkPlayer>
    {
        readonly ulong Id;

        public NetworkPlayer(IPEndPoint? endPoint)
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

        public NetworkPlayer(Net.Socket endPoint)
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

        NetworkPlayer(ulong v)
        {
            Id = v;
        }

        public override bool Equals(object? obj) =>
            obj is NetworkPlayer player &&
            Equals(player);
        public bool Equals(NetworkPlayer other) => Id == other.Id;
        public override int GetHashCode() => HashCode.Combine(Id);

        public static bool operator ==(NetworkPlayer left, NetworkPlayer right) => left.Equals(right);
        public static bool operator !=(NetworkPlayer left, NetworkPlayer right) => !left.Equals(right);

        public override string ToString() => Id.ToString();
        private string GetDebuggerDisplay() => ToString();

        public static explicit operator ulong(NetworkPlayer v) => v.Id;
        public static explicit operator NetworkPlayer(ulong v) => new(v);
    }
}
