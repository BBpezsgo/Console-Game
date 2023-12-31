﻿namespace ConsoleGame
{
    public partial struct Mesh
    {
        public static Mesh MakeCube(float size = 1f) => new()
        {
            Triangles = new TriangleEx[]
            {
                new((0,    0,    0   ), (0,    size, 0   ), (size, size, 0   )) { MaterialIndex = 0 },
                new((0,    0,    0   ), (size, size, 0   ), (size, 0,    0   )) { MaterialIndex = 0 },

                new((size, 0,    0   ), (size, size, 0   ), (size, size, size)) { MaterialIndex = 0 },
                new((size, 0,    0   ), (size, size, size), (size, 0,    size)) { MaterialIndex = 0 },

                new((size, 0,    size), (size, size, size), (0,    size, size)) { MaterialIndex = 0 },
                new((size, 0,    size), (0,    size, size), (0,    0,    size)) { MaterialIndex = 0 },

                new((0,    0,    size), (0,    size, size), (0,    size, 0   )) { MaterialIndex = 0 },
                new((0,    0,    size), (0,    size, 0   ), (0,    0,    0   )) { MaterialIndex = 0 },

                new((0,    size, 0   ), (0,    size, size), (size, size, size)) { MaterialIndex = 0 },
                new((0,    size, 0   ), (size, size, size), (size, size, 0   )) { MaterialIndex = 0 },

                new((size, 0,    size), (0,    0,    size), (0,    0,    0   )) { MaterialIndex = 0 },
                new((size, 0,    size), (0,    0,    0   ), (size, 0,    0   )) { MaterialIndex = 0 },
            },
            Materials = new Material[1] { new() },
        };
    }
}
