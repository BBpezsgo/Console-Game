namespace ConsoleGame;

public class RendererComponent3D : Component
{
    public Mesh Mesh;
    public Material Material;
    public Maths.Matrix4x4 Rotation;

    public RendererComponent3D(Entity entity) : base(entity)
    {
        Mesh = Mesh.MakeCube();
        Material = Mesh.Materials[0];
        Rotation = Maths.Matrix4x4.CreateRotationY(0f);
    }

    public RendererComponent3D(Entity entity, Action<Material>? materializer, Action<Mesh>? meshizer = null) : this(entity)
    {
        materializer?.Invoke(Material);
        meshizer?.Invoke(Mesh);
    }

    public override void Make()
    {
        base.Make();
        Game.Instance.Scene.RendererComponent3Ds.Register(this);
    }

    public override void Destroy()
    {
        base.Destroy();
        Game.Instance.Scene.RendererComponent3Ds.Deregister(this);
    }

    public virtual void Render(List<TransformedMesh> meshBuffer)
    {
        meshBuffer.Add(Mesh.ToTransformed(new Vector3(Position.X * 2f, 0f, Position.Y * 2f), Rotation));
    }
}
