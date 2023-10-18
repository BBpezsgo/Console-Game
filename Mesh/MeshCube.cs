namespace ConsoleGame
{
    public partial struct Mesh
    {
        public static Mesh MakeCube(float size = 1f) => new()
        {
            Triangles = new List<TriangleEx>()
            {
                new TriangleEx((0, 0, 0),         (0, size, 0),       (size, size, 0)     ),
                new TriangleEx((0, 0, 0),         (size, size, 0),    (size, 0, 0)        ),

                new TriangleEx((size, 0, 0),      (size, size, 0),    (size, size, size)  ),
                new TriangleEx((size, 0, 0),      (size, size, size), (size, 0, size)     ),

                new TriangleEx((size, 0, size),   (size, size, size), (0, size, size)     ),
                new TriangleEx((size, 0, size),   (0, size, size),    (0, 0, size)        ),

                new TriangleEx((0, 0, size),      (0, size, size),    (0, size, 0)        ),
                new TriangleEx((0, 0, size),      (0, size, 0),       (0, 0, 0)           ),

                new TriangleEx((0, size, 0),      (0, size, size),    (size, size, size)  ),
                new TriangleEx((0, size, 0),      (size, size, size), (size, size, 0)     ),

                new TriangleEx((size, 0, size),   (0, 0, size),       (0, 0, 0)           ),
                new TriangleEx((size, 0, size),   (0, 0, 0),          (size, 0, 0)        ),
            },
        };
    }
}
