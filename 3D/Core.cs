using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace ConsoleGame;

public partial class MeshRenderer : ITimeProvider
{
    ConsoleRenderer? renderer;
    FpsCounter FpsCounter;
    bool isRunning;

    [NotNull] public static MeshRenderer? Instance = null!;

    readonly Mesh MeshToRender;
    readonly Image? ImageToRender;

    public float DeltaTime { get; private set; }

    MeshRenderer()
    {
        Instance = this;
        FpsCounter = new FpsCounter(8);
        Time.Provider = this;
        renderer = null;
    }

    public MeshRenderer(string objFile, string? imgFile) : this()
    {
        MeshToRender = Obj.LoadFile(objFile).Scale(3);
        ImageToRender = Image.LoadFile(imgFile, ColorF.Black);
    }

    public MeshRenderer(Mesh mesh, Image? image) : this()
    {
        MeshToRender = mesh;
        ImageToRender = image;
    }

    [SupportedOSPlatform("windows")]
    unsafe public void Start()
    {
        Console.Title = "Mesh Renderer";

        // ConsoleUtils.Reset();

        Terminal.Setup();
        // ConsoleHandler.SetFont("Consolas", 8);

        ConsoleListener.KeyEvent += OnKey;
        ConsoleListener.MouseEvent += OnMouse;
        ConsoleListener.WindowBufferSizeEvent += OnBufferSize;

        ConsoleListener.Start();

        renderer = new ConsoleRenderer((short)Console.WindowWidth, (short)Console.WindowHeight);
        GUI.Renderer = renderer;

        double last = DateTime.Now.TimeOfDay.TotalSeconds;
        double now;
        isRunning = true;

        while (isRunning)
        {
            now = DateTime.Now.TimeOfDay.TotalSeconds;
            DeltaTime = (float)(now - last);
            last = now;

            Tick();

            // int overrun = (int)(((1f / 40f) - (DateTime.Now.TimeOfDay.TotalSeconds - now)) * 1000f);
            // if (overrun > 0) Thread.Sleep(overrun);
        }

        ConsoleListener.Stop();
        Terminal.Restore();
        GUI.Renderer = null;
        Time.Provider = null!;
        Console.Write(Ansi.Reset);
        Console.Clear();
    }

    public void Exit()
    {
        isRunning = false;
    }
}
