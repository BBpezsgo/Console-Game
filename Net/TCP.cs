using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ConsoleGame.Net
{
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

        class TcpState
        {
            public byte[] buffer;

            public TcpState(int bufferSize)
            {
                buffer = new byte[bufferSize];
            }
        }

        readonly System.Net.Sockets.Socket Socket;
        readonly TcpState State;
        EndPoint EndPoint;

        readonly ConcurrentQueue<byte[]> IncomingQueue;
        readonly Thread ListeningThread;
        readonly List<TcpClient?> clients;
        public override Socket[] Clients
        {
            get
            {
                List<Socket> result = new();
                for (int i = 0; i < clients.Count; i++)
                {
                    TcpClient? client = clients[i];
                    if (client == null) continue;
                    if (!client.IsAlive) continue;
                    result.Add((Socket)client.Socket.RemoteEndPoint);
                }
                return result.ToArray();
            }
        }

        public override Socket ServerEndPoint => (Socket)Socket.RemoteEndPoint;

        bool ShouldListen;

        public override Socket LocalEndPoint => (Socket)Socket.LocalEndPoint;

        public override string StatusText
        {
            get
            {
                if (Socket == null)
                { return "None"; }

                if (Socket.Connected)
                { return "Connected"; }

                if (Socket.IsBound)
                { return "Bounded"; }

                return "None";
            }
        }
        public override bool IsDone
        {
            get
            {
                if (Socket == null)
                { return false; }

                if (Socket.Connected)
                { return true; }

                if (Socket.IsBound)
                { return false; }

                return false;
            }
        }

        public TCP(bool debugLog = false) : base(debugLog)
        {
            Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            EndPoint = new IPEndPoint(IPAddress.Any, 0);
            State = new TcpState(BufferSize);
            IncomingQueue = new ConcurrentQueue<byte[]>();
            ListeningThread = new Thread(Listen);
            clients = new List<TcpClient?>();
        }

        void Listen()
        {
            while (ShouldListen)
            {
                if (!Socket.Connected) continue;
                try
                {
                    int bytes = Socket.ReceiveFrom(State.buffer, 0, BufferSize, SocketFlags.None, ref EndPoint);
                    if (DebugLog) Debug.WriteLine($"[Net]: Received {bytes} bytes");
                    byte[] received = new byte[bytes];
                    Array.Copy(State.buffer, 0, received, 0, bytes);
                    IncomingQueue.Enqueue(received);
                }
                catch (SocketException)
                { break; }
            }
            Debug.WriteLine($"[Net]: Listening aborted");
        }

        public override void Server(IPAddress address, int port)
        {
            Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            Socket.Bind(new IPEndPoint(address, port));
            Socket.Listen();
            Socket.AcceptAsync().ContinueWith(OnAccept);
        }

        void OnAccept(Task<System.Net.Sockets.Socket> task)
        {
            if (!task.IsCompletedSuccessfully) return;
            Debug.WriteLine($"[Net]: Client {task.Result.RemoteEndPoint} connected");
            clients.Add(new TcpClient(task.Result, BufferSize));

            OnClientConnectedInternal((Socket)(task.Result.RemoteEndPoint ?? throw new NullReferenceException()));
        }

        public override void Client(IPAddress address, int port)
        {
            Socket.Connect(address, port);
            ShouldListen = true;
            ListeningThread.Start();
        }

        protected override void Send(byte[] data)
        {
            if (DebugLog) Debug.WriteLine($"[Net]: Sending {data.Length} bytes ...");
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                TcpClient? client = clients[i];
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

            if (Socket.Connected)
            {
                int bytes = Socket.Send(data, 0, data.Length, SocketFlags.None);
                if (DebugLog) Debug.WriteLine($"[Net]: Sent {bytes} bytes to {Socket.RemoteEndPoint}");
            }
        }

        protected override void SendTo(byte[] data, Socket destination)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                TcpClient? client = clients[i];
                if (client == null) continue;
                if (!client.IsAlive) continue;
                if (client.Socket.RemoteEndPoint == null ||
                    client.Socket.RemoteEndPoint is not IPEndPoint _ipEp ||
                    !_ipEp.Equals((IPEndPoint)destination)) continue;

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
            ShouldListen = false;
            Socket.Close();
            Socket.Dispose();
        }

        public override void Receive()
        {
            while (IncomingQueue.TryDequeue(out byte[]? message))
            {
                OnReceiveInternal((Socket)Socket.RemoteEndPoint, message);
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                TcpClient? client = clients[i];
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
            Socket?.Dispose();
        }
    }
}
