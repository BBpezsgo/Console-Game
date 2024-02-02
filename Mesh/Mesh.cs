using System.Diagnostics;
using System.Numerics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Mesh
    {
        public Triangle3Ex[] Triangles;
        public Material[] Materials;

        public Mesh(IEnumerable<Triangle3Ex> triangles)
        {
            Triangles = triangles.ToArray();
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
                Triangles[i].PointA *= scale;
                Triangles[i].PointB *= scale;
                Triangles[i].PointC *= scale;
            }
            return this;
        }
        public Mesh Scale(Vector3 scale)
        {
            for (int i = 0; i < Triangles.Length; i++)
            {
                Triangles[i].PointA *= scale;
                Triangles[i].PointB *= scale;
                Triangles[i].PointC *= scale;
            }
            return this;
        }
    }
}
