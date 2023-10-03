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

    public partial class Game
    {
        public Scene Scene;
        SafeFileHandle ConsoleHandle;
        ConsoleRenderer renderer;
        float deltaTime;
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
                ("Exit", Exit),
                ("Respawn", Menu_YouDied_Respawn));
            InputBox_ConnectAddress = new InputBox(renderer, "Connect", "127.0.0.1:7777", 21, InputBox_ConnectAddress_Ok, InputBox_ConnectAddress_Cancel);
            InputBox_HostAddress = new InputBox(renderer, "Host", "0.0.0.0:7777", 21, InputBox_HostAddress_Ok, InputBox_HostAddress_Cancel);

            while (isRunning)
            {
                now = DateTime.Now.TimeOfDay.TotalSeconds;
                deltaTime = (float)(now - last);
                last = now;

                Tick();
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
            Scene = new Scene(this);
            networkMode = NetworkMode.Offline;

            connection = null;

            Scene.Load();
            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, LocalOwner));
        }

        void Menu_YouDied_Respawn()
        {
            bool hasPlayer = false;
            GameObject[] players = Scene.ObjectsOfTag(Tags.Player);
            for (int i = 0; i < players.Length; i++)
            {
                if (((NetworkedGameObject)players[i]).IsOwned)
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

            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, LocalOwner));
        }

        void InputBox_ConnectAddress_Ok()
        {
            string value = InputBox_HostAddress.Value.ToString();
            if (!Socket.TryParse(value, out Socket socket))
            { return; }

            Scene = new Scene(this);
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

            Scene = new Scene(this);
            networkMode = NetworkMode.Server;

            connection = new UDP(false);
            connection.Server(socket.Address, socket.Port);
            connection.OnReceive += OnDataReceive;
            connection.OnClientConnected += OnClientConnected;
            connection.OnClientDisconnected += OnClientDisconnected;

            Scene.Load();
            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, LocalOwner));

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
            GameObject[] players = Scene.ObjectsOfTag(Tags.Player);
            for (int i = 0; i < players.Length; i++)
            {
                if (((NetworkedGameObject)players[i]).Owner == new ObjectOwner(sender))
                {
                    hasPlayer = true;
                    break;
                }
            }

            if (hasPlayer)
            { return; }

            if (networkMode == NetworkMode.Client)
            { return; }

            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, new ObjectOwner(sender)));
        }

        public void Exit()
        {
            isRunning = false;
        }

        public static Vector ConsoleToWorld(VectorInt consolePosition)
            => consolePosition * new Vector(0.5f, 1f);
        public static VectorInt WorldToConsole(Vector worldPosition)
            => (worldPosition * new Vector(2f, 1f)).Round();
    }
}
