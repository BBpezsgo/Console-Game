using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using ConsoleGame.Net;

namespace ConsoleGame;

public partial class Game : ITimeProvider
{
    public Scene Scene;
    IOnlySetterRenderer<ConsoleChar> renderer;
    Buffer<float> depthBuffer;
    float deltaTime;
    FpsCounter FpsCounter;
    bool isRunning;

    [NotNull]
    public static Game? Instance = null!;

    MenuBoxed MainMenu;
    InputBox InputBox_HostAddress;
    InputBox InputBox_ConnectAddress;
    Menu Menu_YouDied;
    int CurrentMenu = 1;

    readonly List<Timer> Timers = new();

    public PlayerData PlayerData;
    bool Alted;
    public bool HandleInput => !Alted; // && Terminal.Form.IsForeground;

    public float DeltaTime => deltaTime;
    public static IOnlySetterRenderer<ConsoleChar> Renderer => Instance.renderer;
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

    public static void StartTimer(Timer timer) => Instance.Timers.Add(timer);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Game()
    {
        Instance = this;
        networkMode = NetworkMode.Offline;
        FpsCounter = new FpsCounter(8);
        Time.Provider = this;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SupportedOSPlatform("windows")]
    unsafe public void Start()
    {
        Console.Title = "Game";

        // ConsoleUtils.Reset();

        Terminal.Setup();
        // ConsoleHandler.SetFont("Consolas", 8);
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        ConsoleListener.KeyEvent += OnKey;
        ConsoleListener.MouseEvent += ConsoleMouse.Feed;
        ConsoleListener.WindowBufferSizeEvent += OnBufferSize;

        ConsoleListener.Start();

        renderer = new Win32.Console.AnsiRendererTrueColor((short)Console.WindowWidth, (short)Console.WindowHeight);
        depthBuffer = new Buffer<float>(renderer);
        GUI.Renderer = renderer;

        double last = DateTime.Now.TimeOfDay.TotalSeconds;
        double now;
        isRunning = true;

        MainMenu = new MenuBoxed("Main Menu",
            ("Offline", MainMenuHandler_Offline),
            ("Connect", () => CurrentMenu = 2),
            ("Host", () => CurrentMenu = 3),
            ("Exit", Exit)
            );
        Menu_YouDied = new Menu(
            ("Respawn", Menu_YouDied_Respawn),
            ("Exit", Exit));
        InputBox_ConnectAddress = new InputBox("Connect", "127.0.0.1:7777", 21, InputBox_ConnectAddress_Ok, InputBox_ConnectAddress_Cancel);
        InputBox_HostAddress = new InputBox("Host", "0.0.0.0:7777", 21, InputBox_HostAddress_Ok, InputBox_HostAddress_Cancel);

        if (Files.TryLoadAnyDataFile("player_data", out PlayerData savedPlayerData))
        {
            PlayerData = savedPlayerData;
        }

        // MainMenu.Select("Offline");

        while (isRunning)
        {
            now = DateTime.Now.TimeOfDay.TotalSeconds;
            deltaTime = (float)(now - last);
            last = now;

            Tick();

            // int overrun = (int)(((1f / 40f) - (DateTime.Now.TimeOfDay.TotalSeconds - now)) * 1000f);
            // if (overrun > 0) Thread.Sleep(overrun);
        }

        connection?.Close();
        ConsoleListener.Stop();
        Terminal.Restore();
        GUI.Renderer = null;
        Time.Provider = null!;
    }

    void MainMenuHandler_Offline()
    {
        Scene = new Scene();
        networkMode = NetworkMode.Offline;

        connection = null;

        Scene.Load();
        Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), LocalOwner);
        newEntity.Position = new Vector2(3, 4);
        Scene.AddEntity(newEntity);
        FollowEntity = newEntity;
    }

    void Menu_YouDied_Respawn()
    {
        bool hasPlayer = false;
        ReadOnlySpan<Entity> players = Scene.ObjectsOfTag(Tags.Player);
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
        newEntity.Position = new Vector2(3, 4);
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
        newEntity.Position = new Vector2(3, 4);
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
        ReadOnlySpan<Entity> players = Scene.ObjectsOfTag(Tags.Player);
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
        newEntity.Position = new Vector2(3, 4);
        Scene.AddEntity(newEntity);
    }

    public void Exit()
    {
        isRunning = false;
        Files.SaveText("player_data", PlayerData);
    }
}
