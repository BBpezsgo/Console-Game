using System.Net;

namespace ConsoleGame.Net;

public delegate void ReceivedEvent(Socket sender, byte[] data);
public delegate void ClientConnectedEvent(Socket client);
public delegate void ClientDisconnectedEvent(Socket client);

public abstract class Connection : IDisposable
{
    public const int BufferSize = 8 * 1024;

    public event ReceivedEvent? OnReceive;
    public event ClientConnectedEvent? OnClientConnected;
    public event ClientDisconnectedEvent? OnClientDisconnected;

    protected readonly Queue<Message> OutgoingQueue;

    protected readonly bool DebugLog;

    public abstract ReadOnlySpan<Socket> Clients { get; }
    public abstract Socket LocalEndPoint { get; }
    public abstract Socket RemoteEndPoint { get; }

    public abstract string StatusText { get; }
    public abstract bool IsDone { get; }

    protected Connection(bool debugLog = false)
    {
        OutgoingQueue = new Queue<Message>();
        DebugLog = debugLog;
    }

    public abstract void Server(IPAddress address, int port);

    public abstract void Client(IPAddress address, int port);

    protected void Send<T>(T data) where T : ISerializable
        => Send(Serializing.Serialize(data));
    protected abstract void Send(byte[] data);

    protected void SendTo<T>(T data, Socket destination) where T : ISerializable
        => SendTo(Serializing.Serialize(data), destination);
    protected abstract void SendTo(byte[] data, Socket destination);

    public void Send(Message message)
    {
        OutgoingQueue.Enqueue(message);
    }

    public void SendImmediate(Message message)
    {
        Send<Message>(message);
    }

    uint GuidCounter;
    public void Flush()
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);
        while (OutgoingQueue.TryDequeue(out Message? message))
        {
            GuidCounter++;
            if (GuidCounter == 0) GuidCounter++;
            message.GUID = GuidCounter; // unchecked((uint)Guid.NewGuid().GetHashCode());
            writer.Write(message);
        }
        byte[] data = memoryStream.ToArray();
        if (data.Length > 0) Send(data);
    }

    public abstract void Close();

    public abstract void Receive();

    protected void OnReceiveInternal(Socket sender, byte[] data) => OnReceive?.Invoke(sender, data);
    protected void OnClientConnectedInternal(Socket client) => OnClientConnected?.Invoke(client);
    protected void OnClientDisconnectedInternal(Socket client) => OnClientDisconnected?.Invoke(client);

    public virtual void FeedControlMessage(Socket sender, NetControlMessage netControlMessage) { }

    public abstract void Dispose();
}
