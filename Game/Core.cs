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
        InputBox InputBox_Address;
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
                ("Connect", () => { CurrentMenu = 2; }),
                ("Host", () => { CurrentMenu = 2; }),
                ("Exit", Exit)
                );
            Menu_YouDied = new Menu(renderer,
                ("Exit", Exit),
                ("Respawn", Menu_YouDied_Respawn));
            InputBox_Address = new InputBox(renderer, "Socket", "127.0.0.1:7777");

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

        void MainMenuHandler_Connect()
        {
            Scene = new Scene(this);
            networkMode = NetworkMode.Client;

            connection = new Net.UDP(false);
            connection.Client("127.0.0.1", 7777);
            connection.OnReceive += OnDataReceive;
            connection.OnClientConnected += OnClientConnected;
            connection.OnClientDisconnected += OnClientDisconnected;
        }

        void MainMenuHandler_Host()
        {
            Scene = new Scene(this);
            networkMode = NetworkMode.Server;

            connection = new Net.UDP(false);
            connection.Server(System.Net.IPAddress.Any, 7777);
            connection.OnReceive += OnDataReceive;
            connection.OnClientConnected += OnClientConnected;
            connection.OnClientDisconnected += OnClientDisconnected;

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
