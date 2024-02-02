using System.Numerics;

namespace ConsoleGame
{
    public partial struct Mesh
    {
        public static Mesh MakeCube(float size = 1f) => new()
        {
            Triangles = new Triangle3Ex[]
            {
                new(new Vector3(0,    0,    0   ), new Vector3(0,    size, 0   ), new Vector3(size, size, 0   )) { MaterialIndex = 0 },
                new(new Vector3(0,    0,    0   ), new Vector3(size, size, 0   ), new Vector3(size, 0,    0   )) { MaterialIndex = 0 },

                new(new Vector3(size, 0,    0   ), new Vector3(size, size, 0   ), new Vector3(size, size, size)) { MaterialIndex = 0 },
                new(new Vector3(size, 0,    0   ), new Vector3(size, size, size), new Vector3(size, 0,    size)) { MaterialIndex = 0 },

                new(new Vector3(size, 0,    size), new Vector3(size, size, size), new Vector3(0,    size, size)) { MaterialIndex = 0 },
                new(new Vector3(size, 0,    size), new Vector3(0,    size, size), new Vector3(0,    0,    size)) { MaterialIndex = 0 },

                new(new Vector3(0,    0,    size), new Vector3(0,    size, size), new Vector3(0,    size, 0   )) { MaterialIndex = 0 },
                new(new Vector3(0,    0,    size), new Vector3(0,    size, 0   ), new Vector3(0,    0,    0   )) { MaterialIndex = 0 },

                new(new Vector3(0,    size, 0   ), new Vector3(0,    size, size), new Vector3(size, size, size)) { MaterialIndex = 0 },
                new(new Vector3(0,    size, 0   ), new Vector3(size, size, size), new Vector3(size, size, 0   )) { MaterialIndex = 0 },

                new(new Vector3(size, 0,    size), new Vector3(0,    0,    size), new Vector3(0,    0,    0   )) { MaterialIndex = 0 },
                new(new Vector3(size, 0,    size), new Vector3(0,    0,    0   ), new Vector3(size, 0,    0   )) { MaterialIndex = 0 },
            },
            Materials = new Material[1] { new() },
        };
    }
}
