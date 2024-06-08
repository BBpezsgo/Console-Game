using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace ConsoleGame;

public partial class MeshRenderer : ITimeProvider
{
    Win32.Console.ConsoleRenderer? renderer;
    Buffer<float>? depthBuffer;
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
        depthBuffer = null;
    }

    public MeshRenderer(string objFile, string? imgFile) : this()
    {
        MeshToRender = Obj.LoadFile(objFile).Scale(3);
        if (imgFile is not null)
        {
            if (imgFile.EndsWith(".png"))
            { ImageToRender = Png.LoadFile(imgFile, ColorF.Black); }
            else if (imgFile.EndsWith(".ppm"))
            { ImageToRender = Ppm.LoadFile(imgFile); }
        }
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

        renderer = new Win32.Console.ConsoleRenderer();
        depthBuffer = new Buffer<float>(renderer);
        GUI.Renderer = renderer;

        double last = DateTime.Now.TimeOfDay.TotalSeconds;
        double now;
        isRunning = true;

        while (isRunning)
        {
            now = DateTime.Now.TimeOfDay.TotalSeconds;
            DeltaTime = (float)(now - last);
            last = now;

            if (shouldResizeRenderer)
            {
                renderer.RefreshBufferSize();
                depthBuffer.RefreshBufferSize();
            }

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
