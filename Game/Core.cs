using System.Diagnostics.CodeAnalysis;
using ConsoleGame.Net;
using Microsoft.Win32.SafeHandles;
using Win32;
using Win32.Utilities;

namespace ConsoleGame
{
    public partial class Game
    {
        public Scene Scene;
        SafeFileHandle ConsoleHandle;
        ConsoleRenderer renderer;
        Buffer<float> depthBuffer;
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

        List<ITimer> Timers = new();

        public PlayerData PlayerData;

        public static float DeltaTime => Instance.deltaTime;
        public static ConsoleRenderer Renderer => Instance.renderer;
        public static Buffer<float> DepthBuffer => Instance.depthBuffer;
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

        public static void StartTimer(ITimer timer) => Instance.Timers.Add(timer);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Game()
        {
            Instance = this;
            networkMode = NetworkMode.Offline;
            FpsCounter = new FpsCounter(32);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        unsafe public void Start()
        {
            Console.Title = $"Game";

            // ConsoleUtils.Reset();

            ConsoleHandler.Setup();
            ConsoleHandler.SetFont("Consolas", 8);

            ConsoleListener.KeyEvent += OnKey;
            ConsoleListener.MouseEvent += OnMouse;
            ConsoleListener.WindowBufferSizeEvent += OnBufferSize;

            ConsoleListener.Start();

            fixed (char* fileNamePtr = "CONOUT$")
            { ConsoleHandle = Kernel32.CreateFile(fileNamePtr, 0x40000000, 2, null, (uint)FileMode.Open, 0, IntPtr.Zero); }
            renderer = new ConsoleRenderer(ConsoleHandle, (short)Console.WindowWidth, (short)Console.WindowHeight);
            depthBuffer = new Buffer<float>(renderer);

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

            if (Files.TryLoadAnyDataFile("player_data", out PlayerData savedPlayerData))
            {
                PlayerData = savedPlayerData;
            }

            MainMenu.Select("Offline");

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
                renderer.ClearBuffer();
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
            Files.SaveText("player_data", PlayerData);
        }
    }
}
