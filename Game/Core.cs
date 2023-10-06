using System.Diagnostics.CodeAnalysis;
using ConsoleGame.Net;
using Microsoft.Win32.SafeHandles;
using Win32;

namespace ConsoleGame
{
    public readonly struct GameObjectPrototype
    {
        public const int PLAYER = 1;
        public const int ENEMY = 2;
        public const int HELPER_TURRET = 3;
    }

    struct FpsCounter
    {
        int[] Samples;
        int[] Copy;

        int N;

        public readonly int Value
        {
            get
            {
                if (N == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Samples.Length; i++)
                { sum += Samples[i]; }
                return sum / N;
            }
        }

        public FpsCounter(int sampleCount)
        {
            Samples = new int[sampleCount];
            Copy = new int[sampleCount];
            N = 0;
        }

        public void Sample(int fps)
        {
            Array.Copy(Samples, 0, Copy, 1, Samples.Length - 1);
            Copy[0] = fps;

            int[] temp = Samples;
            Samples = Copy;
            Copy = temp;

            N = Math.Min(N + 1, Samples.Length);
        }
    }

    public partial class Game
    {
        public Scene Scene;
        SafeFileHandle ConsoleHandle;
        ConsoleRenderer renderer;
        DepthBuffer depthBuffer;
        float deltaTime;
        FpsCounter FpsCounter;
        bool isRunning;
        bool ClearOnExit = true;

        [NotNull]
        public static Game? Instance = null;

        MenuBoxed MainMenu;
        InputBox InputBox_HostAddress;
        InputBox InputBox_ConnectAddress;
        Menu Menu_YouDied;
        int CurrentMenu = 1;

        public static float DeltaTime => Instance.deltaTime;
        public static ConsoleRenderer Renderer => Instance.renderer ?? throw new NullReferenceException();
        public static DepthBuffer DepthBuffer => Instance.depthBuffer ?? throw new NullReferenceException();
        public static ObjectOwner LocalOwner
        {
            get
            {
                Game instance = Instance;

                if (instance == null || instance.networkMode == NetworkMode.Offline || instance.connection == null)
                { return new ObjectOwner(); }

                return new ObjectOwner(instance.connection.LocalEndPoint);
            }
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Game()
        {
            Instance = this;
            networkMode = NetworkMode.Offline;
            FpsCounter = new FpsCounter(32);
        }
#pragma warning restore CS8618

        unsafe public void Start()
        {
            Console.Title = $"Game";

            ConsoleHandler.Setup();

            ConsoleListener.KeyEvent += OnKey;
            ConsoleListener.MouseEvent += OnMouse;
            ConsoleListener.WindowBufferSizeEvent += OnBufferSize;

            ConsoleListener.Start();

            fixed (char* fileNamePtr = "CONOUT$")
            { ConsoleHandle = Kernel32.CreateFile(fileNamePtr, 0x40000000, 2, null, (uint)FileMode.Open, 0, IntPtr.Zero); }
            renderer = new ConsoleRenderer(ConsoleHandle, (short)Console.WindowWidth, (short)Console.WindowHeight);
            depthBuffer = new DepthBuffer(renderer);

            double last = DateTime.Now.TimeOfDay.TotalSeconds;
            double now;
            isRunning = true;

            MainMenu = new MenuBoxed(renderer, "Main Menu",
                ("Offline", MainMenuHandler_Offline),
                ("Connect", () => CurrentMenu = 2),
                ("Host", () => CurrentMenu = 3),
                ("Exit", Exit)
                );
            Menu_YouDied = new Menu(renderer,
                ("Respawn", Menu_YouDied_Respawn),
                ("Exit", Exit));
            InputBox_ConnectAddress = new InputBox(renderer, "Connect", "127.0.0.1:7777", 21, InputBox_ConnectAddress_Ok, InputBox_ConnectAddress_Cancel);
            InputBox_HostAddress = new InputBox(renderer, "Host", "0.0.0.0:7777", 21, InputBox_HostAddress_Ok, InputBox_HostAddress_Cancel);

            while (isRunning)
            {
                now = DateTime.Now.TimeOfDay.TotalSeconds;
                deltaTime = (float)(now - last);
                last = now;

                Tick();

                // int overrun = (int)(((1f / 40f) - (DateTime.Now.TimeOfDay.TotalSeconds - now)) * 1000f);
                // if (overrun > 0) Thread.Sleep(overrun);
            }

            if (ClearOnExit)
            {
                renderer.Clear();
                renderer.Render();
            }

            connection?.Close();
            ConsoleListener.Stop();
            ConsoleHandle.Close();
            ConsoleHandle.Dispose();
            ConsoleHandler.Restore();
        }

        void MainMenuHandler_Offline()
        {
            Scene = new Scene();
            networkMode = NetworkMode.Offline;

            connection = null;

            Scene.Load();
            Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), LocalOwner);
            newEntity.Position = new Vector(3, 4);
            Scene.AddEntity(newEntity);
            FollowEntity = newEntity;
        }

        void Menu_YouDied_Respawn()
        {
            bool hasPlayer = false;
            Entity[] players = Scene.ObjectsOfTag(Tags.Player);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<NetworkEntityComponent>().IsOwned)
                {
                    hasPlayer = true;
                    break;
                }
            }

            if (hasPlayer)
            { return; }

            if (networkMode == NetworkMode.Client)
            {
                connection?.SendImmediate(new RespawnRequestMessage()
                {
                    Type = MessageType.REQ_RESPAWN,
                });
                return;
            }

            Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), LocalOwner);
            newEntity.Position = new Vector(3, 4);
            Scene.AddEntity(newEntity);
            FollowEntity = newEntity;
        }

        void InputBox_ConnectAddress_Ok()
        {
            string value = InputBox_ConnectAddress.Value.ToString();
            if (!Socket.TryParse(value, out Socket socket))
            { return; }

            Scene = new Scene();
            networkMode = NetworkMode.Client;

            connection = new UDP(false);
            connection.Client(socket.Address, socket.Port);
            connection.OnReceive += OnDataReceive;
            connection.OnClientConnected += OnClientConnected;
            connection.OnClientDisconnected += OnClientDisconnected;

            InputBox_ConnectAddress.Reset();
        }

        void InputBox_ConnectAddress_Cancel()
        {
            CurrentMenu = 1;
            InputBox_ConnectAddress.Reset();
        }

        void InputBox_HostAddress_Ok()
        {
            string value = InputBox_HostAddress.Value.ToString();
            if (!Socket.TryParse(value, out Socket socket))
            { return; }

            Scene = new Scene();
            networkMode = NetworkMode.Server;

            connection = new UDP(false);
            connection.Server(socket.Address, socket.Port);
            connection.OnReceive += OnDataReceive;
            connection.OnClientConnected += OnClientConnected;
            connection.OnClientDisconnected += OnClientDisconnected;

            Scene.Load();

            Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), LocalOwner);
            newEntity.Position = new Vector(3, 4);
            Scene.AddEntity(newEntity);
            FollowEntity = newEntity;

            InputBox_HostAddress.Reset();
        }

        void InputBox_HostAddress_Cancel()
        {
            CurrentMenu = 1;
            InputBox_HostAddress.Reset();
        }

        void OnRespawnRequest(Socket sender)
        {
            bool hasPlayer = false;
            Entity[] players = Scene.ObjectsOfTag(Tags.Player);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<NetworkEntityComponent>().Owner == new ObjectOwner(sender))
                {
                    hasPlayer = true;
                    break;
                }
            }

            if (hasPlayer)
            { return; }

            if (networkMode == NetworkMode.Client)
            { return; }

            Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), new ObjectOwner(sender));
            newEntity.Position = new Vector(3, 4);
            Scene.AddEntity(newEntity);
        }

        public void Exit()
        {
            isRunning = false;
        }
    }
}
