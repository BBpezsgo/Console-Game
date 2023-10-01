using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace ConsoleGame.Net
{
    public class UDP : Connection
    {
        class UdpClient
        {
            public readonly Pipe Pipe;
            public readonly ConcurrentQueue<byte[]> IncomingQueue;
            public readonly IPEndPoint RemoteEndPoint;
            public bool IsAlive;
            public readonly bool DebugLog;

            readonly Thread ListeningThread;

            public UdpClient(UdpReceiveResult udpReceiveResult, bool debugLog = false)
            {
                Pipe = new Pipe();
                RemoteEndPoint = udpReceiveResult.RemoteEndPoint;
                IsAlive = true;
                IncomingQueue = new ConcurrentQueue<byte[]>();
                ListeningThread = new Thread(Listen);
                ListeningThread.Start();
                DebugLog = debugLog;
            }

            async void Listen()
            {
                if (DebugLog) Debug.WriteLine($"[Net]: Listening for {RemoteEndPoint} ...");
                while (IsAlive)
                {
                    ReadResult readResult = await Pipe.Reader.ReadAsync();
                    if (!IsAlive) break;
                    IncomingQueue.Enqueue(readResult.Buffer.FirstSpan.ToArray());
                    Pipe.Reader.AdvanceTo(readResult.Buffer.End);
                }
                if (DebugLog) Debug.WriteLine($"[Net]: Listening for {RemoteEndPoint} aborted");
            }

            public void Dispose()
            {
                IsAlive = false;
            }
        }

        public override Socket LocalEndPoint => (Socket)localEndPoint;
        IPEndPoint? localEndPoint;
        readonly ConcurrentQueue<UdpReceiveResult> IncomingQueue = new();

        [NotNull]
        System.Net.Sockets.UdpClient? UdpSocket = null;
        readonly Thread ListeningThread;
        bool isServer;

        bool ShouldListen;

        readonly ConcurrentDictionary<string, UdpClient> Connections = new();

        public override Socket[] Clients
        {
            get
            {
                List<Socket> result = new();
                foreach (KeyValuePair<string, UdpClient> client in Connections)
                {
                    if (client.Value == null) continue;
                    result.Add((Socket)client.Value.RemoteEndPoint);
                }
                return result.ToArray();
            }
        }

        public override Socket ServerEndPoint => (Socket)UdpSocket.Client.RemoteEndPoint;

        public UDP(bool debugLog = false) : base(debugLog)
        {
            ListeningThread = new Thread(Listen);
        }

        public override void Client(IPAddress address, int port)
        {
            isServer = false;
            UdpSocket = new System.Net.Sockets.UdpClient();
            UdpSocket.Connect(address, port);
            localEndPoint = (IPEndPoint?)UdpSocket.Client.LocalEndPoint;
            ShouldListen = true;
            ListeningThread.Start();
            Send(new NetControlMessage(NetControlMessageKind.HEY_IM_CLIENT));
        }

        public override void Server(IPAddress address, int port)
        {
            isServer = true;
            localEndPoint = new IPEndPoint(address, port);
            UdpSocket = new System.Net.Sockets.UdpClient(localEndPoint);
            ShouldListen = true;
            ListeningThread.Start();
        }

        async void Listen()
        {
            if (UdpSocket == null) return;

            while (ShouldListen)
            {
                try
                {
                    UdpReceiveResult result = await UdpSocket.ReceiveAsync();

                    if (!ShouldListen) break;

                    if (DebugLog) Debug.WriteLine($"[Net]: Received {result.Buffer.Length} bytes");

                    if (isServer)
                    {
                        if (Connections.TryGetValue(result.RemoteEndPoint.ToString(), out var client))
                        {
                            await client.Pipe.Writer.WriteAsync(result.Buffer);
                        }
                        else
                        {
                            Debug.WriteLine($"[Net]: Client {result.RemoteEndPoint} connected");

                            UdpClient newClient = new(result);
                            Connections.TryAdd(result.RemoteEndPoint.ToString(), new UdpClient(result));
                            await newClient.Pipe.Writer.WriteAsync(result.Buffer);

                            OnClientConnectedInternal((Socket)result.RemoteEndPoint);
                        }
                    }
                    else
                    {
                        IncomingQueue.Enqueue(result);
                    }
                }
                catch (SocketException)
                { break; }
            }
            Debug.WriteLine($"[Net]: Listening aborted");
        }

        public override void Close()
        {
            ShouldListen = false;
            foreach (KeyValuePair<string, UdpClient> client in Connections)
            {
                client.Value.Dispose();
            }
            UdpSocket?.Close();
            UdpSocket?.Dispose();
        }

        public override void Receive()
        {
            while (IncomingQueue.TryDequeue(out var message))
            {
                OnReceiveInternal((Socket)message.RemoteEndPoint, message.Buffer);
            }

            List<string> shouldRemove = new();

            foreach (KeyValuePair<string, UdpClient> client in Connections)
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
                if (Connections.TryRemove(shouldRemove[i], out UdpClient? removedClient))
                { removedClient?.Dispose(); }
            }
        }

        public override void Send(byte[] data)
        {
            if (UdpSocket == null) return;

            if (DebugLog) Debug.WriteLine($"[Net]: Sending {data.Length} bytes ...");
            if (isServer)
            {
                foreach (KeyValuePair<string, UdpClient> client in Connections)
                {
                    int sent = UdpSocket.Send(data, data.Length, client.Value.RemoteEndPoint);
                    if (DebugLog) Debug.WriteLine($"[Net]: Sent {sent} bytes to {client.Value.RemoteEndPoint}");
                }
            }
            else
            {
                UdpSocket.Send(data, data.Length);
            }
        }

        public override void FeedControlMessage(NetControlMessage netControlMessage)
        {
            if (UdpSocket == null) return;

            if (netControlMessage.Type == MessageType.CONTROL)
            {
                if (!isServer) return;

                Debug.WriteLine($"[Net]: Someone connected");

                return;
            }
        }
    }
}
