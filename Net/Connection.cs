using System.Net;
using System.Text;
using DataUtilities.Serializer;

namespace ConsoleGame.Net
{
    public delegate void ReceivedEvent(Socket sender, byte[] data);
    public delegate void ClientConnectedEvent(Socket client);
    public delegate void ClientDisconnectedEvent(Socket client);

    public abstract class Connection
    {
        public const int BufferSize = 8 * 1024;

        public event ReceivedEvent? OnReceive;
        public event ClientConnectedEvent? OnClientConnected;
        public event ClientDisconnectedEvent? OnClientDisconnected;

        protected readonly Queue<Message> OutgoingQueue;

        protected readonly bool DebugLog;

        public abstract Socket[] Clients { get; }
        public abstract Socket LocalEndPoint { get; }
        public abstract Socket ServerEndPoint { get; }

        public Connection(bool debugLog = false)
        {
            OutgoingQueue = new Queue<Message>();
            DebugLog = debugLog;
        }

        public abstract void Server(IPAddress address, int port);
        public void Server(string address, int port)
            => Server(IPAddress.Parse(address), port);

        public abstract void Client(IPAddress address, int port);
        public void Client(string address, int port)
            => Client(IPAddress.Parse(address), port);

        public void Send(string text) => Send(Encoding.ASCII.GetBytes(text));
        public void Send<T>(ISerializable<T> data) where T : ISerializable<T> => Send(SerializerStatic.Serialize(data));
        public abstract void Send(byte[] data);

        public void Send(Message message)
        {
            OutgoingQueue.Enqueue(message);
        }

        uint GuidCounter;
        public void Flush()
        {
            Serializer serializer = new();
            while (OutgoingQueue.TryDequeue(out Message? message))
            {
                GuidCounter++;
                if (GuidCounter == 0) GuidCounter++;
                message.GUID = GuidCounter; // unchecked((uint)Guid.NewGuid().GetHashCode());
                serializer.Serialize(message);
            }
            if (serializer.Result.Length > 0) Send(serializer.Result);
        }

        public abstract void Close();

        public abstract void Receive();

        protected void OnReceiveInternal(Socket sender, byte[] data) => OnReceive?.Invoke(sender, data);
        protected void OnClientConnectedInternal(Socket client) => OnClientConnected?.Invoke(client);
        protected void OnClientDisconnectedInternal(Socket client) => OnClientDisconnected?.Invoke(client);

        public virtual void FeedControlMessage(NetControlMessage netControlMessage) { }
    }
}