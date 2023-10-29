﻿using System.Diagnostics;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial struct Mesh
    {
        public TriangleEx[] Triangles;
        public Material[] Materials;

        public Mesh(IEnumerable<TriangleEx> triangles)
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
    }
}
