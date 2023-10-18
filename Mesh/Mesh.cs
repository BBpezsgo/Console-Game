using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Mesh
    {
        public List<TriangleEx> Triangles;

        public Mesh(IEnumerable<TriangleEx> triangles)
        {
            Triangles = new List<TriangleEx>(triangles);
        }

        public override readonly string ToString() => $"Mesh ({Triangles.Count} tris)";
        readonly string GetDebuggerDisplay() => ToString();
    }
}
