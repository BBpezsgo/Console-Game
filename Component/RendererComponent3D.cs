namespace ConsoleGame;

public abstract class PostRendererComponent3D : Component
{
    protected PostRendererComponent3D(Entity entity) : base(entity) { }

    public sealed override void Make()
    {
        base.Make();
        Game.Instance.Scene.PostRendererComponent3Ds.Register(this);
    }

    public sealed override void Destroy()
    {
        base.Destroy();
        Game.Instance.Scene.PostRendererComponent3Ds.Deregister(this);
    }

    public abstract void Render();
}
