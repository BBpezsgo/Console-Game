namespace ConsoleGame;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public partial struct Mesh
{
    public Triangle3Ex[] Triangles;
    public Material[] Materials;

    public Mesh(Triangle3Ex[] triangles)
    {
        Triangles = triangles;
        Materials = Array.Empty<Material>();
    }

    public override readonly string ToString() => $"Mesh ({Triangles.Length} tris)";
    readonly string GetDebuggerDisplay() => ToString();

    public Mesh SetMaterial(Material material)
    {
        Materials = new Material[1] { material };
        for (int i = 0; i < Triangles.Length; i++)
        { Triangles[i].MaterialIndex = 0; }
        return this;
    }

    public Mesh Scale(float scale)
    {
        for (int i = 0; i < Triangles.Length; i++)
        {
            Triangles[i].A *= scale;
            Triangles[i].B *= scale;
            Triangles[i].C *= scale;
        }
        return this;
    }

    public Mesh Scale(Vector3 scale)
    {
        for (int i = 0; i < Triangles.Length; i++)
        {
            Triangles[i].A *= scale;
            Triangles[i].B *= scale;
            Triangles[i].C *= scale;
        }
        return this;
    }

    public readonly TransformedMesh ToTransformed() => new(this);

    public readonly TransformedMesh ToTransformed(Vector3 offset, Maths.Matrix4x4 rotation) => new(this)
    {
        Transformation = new MeshTransformation()
        {
            Offset = offset,
            Rotation = rotation,
        }
    };
}
