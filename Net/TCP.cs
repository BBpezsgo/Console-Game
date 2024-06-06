using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ConsoleGame.Net;

public class TCP : Connection
{
    class TcpClient
    {
        public readonly System.Net.Sockets.Socket Socket;
        public readonly Thread ListeningThread;
        public bool IsAlive;
        public readonly ConcurrentQueue<byte[]> IncomingQueue;
        readonly byte[] Buffer;

        public TcpClient(System.Net.Sockets.Socket socket, int bufferSize)
        {
            Socket = socket;
            IsAlive = true;
            Buffer = new byte[bufferSize];
            IncomingQueue = new ConcurrentQueue<byte[]>();
            ListeningThread = new Thread(Listen);
            ListeningThread.Start();
        }

        void Listen()
        {
            Debug.WriteLine($"[Net]: Listening for {Socket.RemoteEndPoint} ...");
            while (IsAlive)
            {
                if (!Socket.Connected) continue;
                try
                {
                    int bytes = Socket.Receive(Buffer, 0, Buffer.Length, SocketFlags.None);
                    Debug.WriteLine($"[Net]: Received {bytes} bytes from {Socket.RemoteEndPoint}");
                    byte[] received = new byte[bytes];
                    Array.Copy(Buffer, 0, received, 0, bytes);
                    IncomingQueue.Enqueue(received);
                }
                catch (SocketException)
                { break; }
            }
            Debug.WriteLine($"[Net]: Listening for {Socket.RemoteEndPoint} aborted");
        }
    }

    readonly System.Net.Sockets.Socket _socket;
    readonly byte[] _buffer;
    readonly ConcurrentQueue<byte[]> _incomingQueue;
    Thread? _listeningThread;
    readonly List<TcpClient?> _clients;
    bool _shouldListen;

    public override ReadOnlySpan<Socket> Clients
    {
        get
        {
            List<Socket> result = new();
            for (int i = 0; i < _clients.Count; i++)
            {
                TcpClient? client = _clients[i];
                if (client == null) continue;
                if (!client.IsAlive) continue;
                result.Add((Socket)client.Socket.RemoteEndPoint);
            }
            return CollectionsMarshal.AsSpan(result);
        }
    }

    public override Socket RemoteEndPoint => (Socket)_socket.RemoteEndPoint;
    public override Socket LocalEndPoint => (Socket)_socket.LocalEndPoint;

    public override string StatusText
    {
        get
        {
            if (_socket == null)
            { return "None"; }

            if (_socket.Connected)
            { return "Connected"; }

            if (_socket.IsBound)
            { return "Bounded"; }

            return "None";
        }
    }
    public override bool IsDone
    {
        get
        {
            if (_socket == null)
            { return false; }

            if (_socket.Connected)
            { return true; }

            if (_socket.IsBound)
            { return false; }

            return false;
        }
    }

    public TCP(bool debugLog = false) : base(debugLog)
    {
        _socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _buffer = new byte[BufferSize];
        _incomingQueue = new ConcurrentQueue<byte[]>();
        _clients = new List<TcpClient?>();
    }

    void Listen()
    {
        while (_shouldListen)
        {
            if (!_socket.Connected) continue;
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                int bytes = _socket.ReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref sender);
                if (DebugLog) Debug.WriteLine($"[Net]: Received {bytes} bytes");
                byte[] received = new byte[bytes];
                Array.Copy(_buffer, 0, received, 0, bytes);
                _incomingQueue.Enqueue(received);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"[Net]: Socket error ({sender}): {ex.SocketErrorCode}");
                break;
            }
        }
        Debug.WriteLine("[Net]: Listening aborted");
    }

    public override void Server(IPAddress address, int port)
    {
        _socket.Bind(new IPEndPoint(address, port));
        _socket.Listen();
        _socket.AcceptAsync().ContinueWith(OnAccept);
    }

    void OnAccept(Task<System.Net.Sockets.Socket> task)
    {
        if (!task.IsCompletedSuccessfully) return;
        Debug.WriteLine($"[Net]: Client {task.Result.RemoteEndPoint} connected");
        _clients.Add(new TcpClient(task.Result, BufferSize));

        OnClientConnectedInternal((Socket)(task.Result.RemoteEndPoint ?? throw new NullReferenceException()));
    }

    public override void Client(IPAddress address, int port)
    {
        _socket.Connect(address, port);
        _shouldListen = true;
        _listeningThread = new Thread(Listen);
        _listeningThread.Start();
    }

    protected override void Send(byte[] data)
    {
        if (DebugLog) Debug.WriteLine($"[Net]: Sending {data.Length} bytes ...");
        for (int i = _clients.Count - 1; i >= 0; i--)
        {
            TcpClient? client = _clients[i];
            if (client == null) continue;
            if (!client.IsAlive) continue;

            if (!client.Socket.Connected)
            {
                Debug.WriteLine($"[Net]: Client {client.Socket.RemoteEndPoint} disconnected");
                OnClientDisconnectedInternal((Socket)(client.Socket.RemoteEndPoint ?? throw new NullReferenceException()));

                client.Socket.Dispose();
                client.IsAlive = false;

                continue;
            }

            try
            {
                int bytes = client.Socket.Send(data, 0, data.Length, SocketFlags.None);
                if (DebugLog) Debug.WriteLine($"[Net]: Sent {bytes} bytes to {client.Socket.RemoteEndPoint}");
            }
            catch (SocketException)
            { continue; }
        }

        if (_socket.Connected)
        {
            int bytes = _socket.Send(data, 0, data.Length, SocketFlags.None);
            if (DebugLog) Debug.WriteLine($"[Net]: Sent {bytes} bytes to {_socket.RemoteEndPoint}");
        }
    }

    protected override void SendTo(byte[] data, Socket destination)
    {
        for (int i = _clients.Count - 1; i >= 0; i--)
        {
            TcpClient? client = _clients[i];
            if (client == null) continue;
            if (!client.IsAlive) continue;
            if (client.Socket.RemoteEndPoint == null ||
                client.Socket.RemoteEndPoint is not IPEndPoint _ipEp ||
                !_ipEp.Equals((IPEndPoint)destination))
            { continue; }

            if (!client.Socket.Connected)
            {
                Debug.WriteLine($"[Net]: Client {client.Socket.RemoteEndPoint} disconnected");
                OnClientDisconnectedInternal((Socket)(client.Socket.RemoteEndPoint ?? throw new NullReferenceException()));

                client.Socket.Dispose();
                client.IsAlive = false;

                continue;
            }

            try
            {
                int bytes = client.Socket.Send(data, 0, data.Length, SocketFlags.None);
                if (DebugLog) Debug.WriteLine($"[Net]: Sent {bytes} bytes to {client.Socket.RemoteEndPoint}");
            }
            catch (SocketException)
            { continue; }
            break;
        }
    }

    public override void Close()
    {
        _shouldListen = false;
        _socket.Close();
        _socket.Dispose();
    }

    public override void Receive()
    {
        while (_incomingQueue.TryDequeue(out byte[]? message))
        {
            OnReceiveInternal((Socket)_socket.RemoteEndPoint, message);
        }

        for (int i = _clients.Count - 1; i >= 0; i--)
        {
            TcpClient? client = _clients[i];
            if (client == null) continue;
            if (!client.IsAlive) continue;

            while (client.IncomingQueue.TryDequeue(out byte[]? message))
            {
                OnReceiveInternal((Socket)client.Socket.RemoteEndPoint, message);
            }
        }
    }

    public override void Dispose()
    {
        _socket?.Dispose();
    }
}
