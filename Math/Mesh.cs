using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Mesh
    {
        public List<Triangle> Triangles;

        public Mesh(IEnumerable<Triangle> triangles)
        {
            Triangles = new List<Triangle>(triangles);
        }

        public override readonly string ToString() => $"Mesh ({Triangles.Count} tris)";
        string GetDebuggerDisplay() => ToString();
    }
}
