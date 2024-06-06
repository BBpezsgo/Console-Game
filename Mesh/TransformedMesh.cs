namespace ConsoleGame;

public struct TransformedMesh
{
    public readonly Mesh Mesh;
    public MeshTransformation Transformation;

    public TransformedMesh(Mesh mesh)
    {
        Mesh = mesh;
        Transformation = default;
    }

    public static TransformedMesh operator +(TransformedMesh mesh, Vector3 vec)
    {
        mesh.Transformation.Offset += vec;
        return mesh;
    }

    public static TransformedMesh operator -(TransformedMesh mesh, Vector3 vec)
    {
        mesh.Transformation.Offset -= vec;
        return mesh;
    }
}
