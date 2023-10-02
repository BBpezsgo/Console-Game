using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.SafeHandles;
using Win32;

namespace ConsoleGame
{
    public readonly struct GameObjectPrototype
    {
        public const int PLAYER = 1;
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

        Menu MainMenu;

        public static float DeltaTime => Instance.deltaTime;
        public static ConsoleRenderer Renderer => Instance.renderer ?? throw new NullReferenceException();

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

            MainMenu = new Menu(renderer, "Main Menu",
                ("Offline", MainMenuHandler_Offline),
                ("Connect", MainMenuHandler_Connect),
                ("Host", MainMenuHandler_Host),
                ("Exit", Exit)
                );

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

            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, new NetworkPlayer()));

            connection = null;
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

            Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, new NetworkPlayer(connection?.LocalEndPoint ?? throw new NullReferenceException())));
        }

        public void Exit()
        {
            isRunning = false;
        }

        public static Vector ConsoleToWorld(Vector consolePosition)
            => consolePosition * new Vector(0.5f, 1f);
        public static Vector WorldToConsole(Vector consolePosition)
            => consolePosition * new Vector(2f, 1f);
    }
}
