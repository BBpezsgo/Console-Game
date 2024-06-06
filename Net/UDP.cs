using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ConsoleGame.Net;

public class UDP : Connection
{
    class UdpClient
    {
        public readonly ConcurrentQueue<byte[]> IncomingQueue;
        public readonly IPEndPoint RemoteEndPoint;
        public bool IsAlive;

        public UdpClient(IPEndPoint endPoint)
        {
            RemoteEndPoint = endPoint;
            IsAlive = true;
            IncomingQueue = new ConcurrentQueue<byte[]>();
        }
    }

    IPEndPoint? _localEndPoint;
    readonly ConcurrentQueue<UdpReceiveResult> _incomingQueue = new();
    [NotNull] System.Net.Sockets.UdpClient? _udpSocket = null;
    readonly Thread _listeningThread;
    bool _isServer;
    bool _shouldListen;
    readonly ConcurrentDictionary<string, UdpClient> _connections = new();
    bool _connectedToServer;

    public override ReadOnlySpan<Socket> Clients
    {
        get
        {
            List<Socket> result = new();
            foreach (KeyValuePair<string, UdpClient> client in _connections)
            {
                if (client.Value == null) continue;
                result.Add((Socket)client.Value.RemoteEndPoint);
            }
            return CollectionsMarshal.AsSpan(result);
        }
    }

    public override Socket RemoteEndPoint => (Socket)_udpSocket.Client.RemoteEndPoint;
    public override Socket LocalEndPoint => (Socket)_localEndPoint;

    public override string StatusText
    {
        get
        {
            if (_udpSocket == null || _udpSocket.Client == null)
            { return "None"; }

            if (_connectedToServer)
            { return "Connected"; }

            if (_udpSocket.Client.IsBound)
            {
                if (_isServer)
                {
                    return "Listening";
                }
                else
                {
                    return "Connecting ...";
                }
            }

            return "Starting ...";
        }
    }
    public override bool IsDone
    {
        get
        {
            if (_udpSocket == null || _udpSocket.Client == null)
            { return false; }

            if (_connectedToServer)
            { return true; }

            if (_udpSocket.Client.IsBound)
            {
                if (_isServer)
                { return true; }
                else
                { return false; }
            }

            return false;
        }
    }

    public UDP(bool debugLog = false) : base(debugLog)
    {
        _listeningThread = new Thread(Listen);
    }

    public override void Client(IPAddress address, int port)
    {
        _isServer = false;
        _udpSocket = new System.Net.Sockets.UdpClient();
        _udpSocket.Connect(address, port);
        _localEndPoint = (IPEndPoint?)_udpSocket.Client.LocalEndPoint;
        _shouldListen = true;
        _listeningThread.Start();
        Send(new NetControlMessage(NetControlMessageKind.HEY_IM_CLIENT_PLS_REPLY));
    }

    public override void Server(IPAddress address, int port)
    {
        _isServer = true;
        _localEndPoint = new IPEndPoint(address, port);
        _udpSocket = new System.Net.Sockets.UdpClient(_localEndPoint);
        _shouldListen = true;
        _listeningThread.Start();
    }

    async void Listen()
    {
        if (_udpSocket == null) return;

        while (_shouldListen)
        {
            try
            {
                UdpReceiveResult result = await _udpSocket.ReceiveAsync();

                if (!_shouldListen) break;

                if (DebugLog) Debug.WriteLine($"[Net]: Received {result.Buffer.Length} bytes");

                if (_isServer)
                {
                    if (_connections.TryGetValue(result.RemoteEndPoint.ToString(), out UdpClient? client))
                    {
                        client.IncomingQueue.Enqueue(result.Buffer);
                    }
                    else
                    {
                        Debug.WriteLine($"[Net]: Client {result.RemoteEndPoint} connected");

                        UdpClient newClient = new(result.RemoteEndPoint);
                        _connections.TryAdd(result.RemoteEndPoint.ToString(), newClient);
                        newClient.IncomingQueue.Enqueue(result.Buffer);

                        OnClientConnectedInternal((Socket)result.RemoteEndPoint);
                    }
                }
                else
                {
                    _incomingQueue.Enqueue(result);
                    _connectedToServer = true;
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionReset)
                {
                    Debug.WriteLine($"[Net]: Socket error: {ex.SocketErrorCode}");
                    break;
                }
            }
        }
        Debug.WriteLine("[Net]: Listening aborted");
    }

    public override void Close()
    {
        _shouldListen = false;
        _connections.Clear();
        _udpSocket?.Close();
        _udpSocket?.Dispose();
    }

    public override void Receive()
    {
        while (_incomingQueue.TryDequeue(out UdpReceiveResult message))
        {
            OnReceiveInternal((Socket)message.RemoteEndPoint, message.Buffer);
        }

        List<string> shouldRemove = new();

        foreach (KeyValuePair<string, UdpClient> client in _connections)
        {
            if (!client.Value.IsAlive)
            {
                shouldRemove.Add(client.Key);
                continue;
            }

            while (client.Value.IncomingQueue.TryDequeue(out byte[]? _message))
            {
                OnReceiveInternal((Socket)client.Value.RemoteEndPoint, _message);
            }
        }

        for (int i = shouldRemove.Count - 1; i >= 0; i--)
        {
            if (_connections.TryRemove(shouldRemove[i], out UdpClient? removedClient))
            {
                OnClientDisconnectedInternal((Socket)removedClient.RemoteEndPoint);
            }
        }
    }

    protected override void Send(byte[] data)
    {
        if (_udpSocket == null) return;

        if (DebugLog) Debug.WriteLine($"[Net]: Sending {data.Length} bytes ...");
        if (_isServer)
        {
            foreach (KeyValuePair<string, UdpClient> client in _connections)
            {
                int sent = _udpSocket.Send(data, data.Length, client.Value.RemoteEndPoint);
                if (DebugLog) Debug.WriteLine($"[Net]: Sent {sent} bytes to {client.Value.RemoteEndPoint}");
            }
        }
        else
        {
            _udpSocket.Send(data, data.Length);
        }
    }

    protected override void SendTo(byte[] data, Socket destination)
    {
        if (_udpSocket == null) return;
        if (!_isServer) return;

        if (DebugLog) Debug.WriteLine($"[Net]: Sending {data.Length} bytes to {destination} ...");

        foreach (KeyValuePair<string, UdpClient> client in _connections)
        {
            if (!client.Value.RemoteEndPoint.Equals((IPEndPoint)destination)) continue;

            int sent = _udpSocket.Send(data, data.Length, client.Value.RemoteEndPoint);

            if (DebugLog) Debug.WriteLine($"[Net]: Sent {sent} bytes to {client.Value.RemoteEndPoint}");

            break;
        }
    }

    public override void FeedControlMessage(Socket sender, NetControlMessage netControlMessage)
    {
        if (_udpSocket == null) return;

        if (netControlMessage.Type == MessageType.CONTROL)
        {
            switch (netControlMessage.Kind)
            {
                case NetControlMessageKind.HEY_IM_CLIENT_PLS_REPLY:
                {
                    Debug.WriteLine("[Net]: Someone connected");
                    SendTo<NetControlMessage>(new NetControlMessage(NetControlMessageKind.HEY_CLIENT_IM_SERVER), sender);
                    return;
                }
                case NetControlMessageKind.HEY_CLIENT_IM_SERVER:
                {
                    Debug.WriteLine("[Net]: Connected to server");
                    _connectedToServer = true;
                    return;
                }
                default: return;
            }
        }
    }

    public override void Dispose()
    {
        _udpSocket?.Dispose();
    }
}
