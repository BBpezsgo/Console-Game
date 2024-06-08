using System.Runtime.InteropServices;

namespace ConsoleGame;

public static class Renderer3D
{
    static readonly bool SimpleLightning = false;

    public static void Render<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<TransformedMesh> meshes, Camera camera, Image? image, Func<ColorF, TPixel> converter)
    {
        List<Triangle4Ex> trianglesToDraw = new();

        for (int i = 0; i < meshes.Length; i++)
        {
            TransformedMesh mesh = meshes[i];
            Project(mesh.Mesh.Triangles, mesh.Mesh.Materials, camera, trianglesToDraw, mesh.Transformation);
        }

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image, converter);
    }

    public static void Render(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<TransformedMesh> meshes, Camera camera, Image? image)
    {
        List<Triangle4Ex> trianglesToDraw = new();

        for (int i = 0; i < meshes.Length; i++)
        {
            TransformedMesh mesh = meshes[i];
            Project(mesh.Mesh.Triangles, mesh.Mesh.Materials, camera, trianglesToDraw, mesh.Transformation);
        }

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image);
    }

    public static void Render<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<Mesh> meshes, Camera camera, Image? image, Func<ColorF, TPixel> converter)
    {
        List<Triangle4Ex> trianglesToDraw = new();

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            Project(mesh.Triangles, mesh.Materials, camera, trianglesToDraw, default);
        }

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image, converter);
    }

    public static void Render(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<Mesh> meshes, Camera camera, Image? image)
    {
        List<Triangle4Ex> trianglesToDraw = new();

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            Project(mesh.Triangles, mesh.Materials, camera, trianglesToDraw, default);
        }

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image);
    }

    public static void Render<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, TransformedMesh mesh, Camera camera, Image? image, Func<ColorF, TPixel> converter)
    {
        List<Triangle4Ex> trianglesToDraw = new(mesh.Mesh.Triangles.Length / 2);

        Project(mesh.Mesh.Triangles, mesh.Mesh.Materials, camera, trianglesToDraw, mesh.Transformation);

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image, converter);
    }

    public static void Render(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, TransformedMesh mesh, Camera camera, Image? image)
    {
        List<Triangle4Ex> trianglesToDraw = new(mesh.Mesh.Triangles.Length / 2);

        Project(mesh.Mesh.Triangles, mesh.Mesh.Materials, camera, trianglesToDraw, mesh.Transformation);

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image);
    }

    public static void Render<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, Mesh mesh, Camera camera, Image? image, Func<ColorF, TPixel> converter)
    {
        List<Triangle4Ex> trianglesToDraw = new(mesh.Triangles.Length / 2);

        Project(mesh.Triangles, mesh.Materials, camera, trianglesToDraw, default);

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image, converter);
    }

    public static void Render(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, Mesh mesh, Camera camera, Image? image)
    {
        List<Triangle4Ex> trianglesToDraw = new(mesh.Triangles.Length / 2);

        Project(mesh.Triangles, mesh.Materials, camera, trianglesToDraw, default);

        ClipAndDrawTriangles(renderer, depth, CollectionsMarshal.AsSpan(trianglesToDraw), image);
    }

    static void Project(
        ReadOnlySpan<Triangle3Ex> triangles,
        ReadOnlySpan<Material> materials,
        Camera camera,
        List<Triangle4Ex> projected,
        MeshTransformation transformation)
    {
        for (int i = 0; i < triangles.Length; i++)
        {
            Project(triangles[i], materials, camera, projected, transformation);
        }

        /*
        trianglesToDraw.Sort(new Comparison<(TriangleEx, Color)>((a, b) =>
        {
            float midA = (a.Item1.PointA.Z + a.Item1.PointB.Z + a.Item1.PointC.Z) / 3;
            float midB = (b.Item1.PointA.Z + b.Item1.PointB.Z + b.Item1.PointC.Z) / 3;
            return -midA.CompareTo(midB);
        }));
        */
    }

    static void Project(
        Triangle3Ex tri,
        ReadOnlySpan<Material> materials,
        Camera camera,
        List<Triangle4Ex> projected,
        MeshTransformation transformation)
    {
        Span<Triangle3Ex> clipped = stackalloc Triangle3Ex[2];

        tri.A -= new Vector3(.5f, 0f, .5f);
        tri.B -= new Vector3(.5f, 0f, .5f);
        tri.C -= new Vector3(.5f, 0f, .5f);

        tri.A = Matrix.Multiply(tri.A.To4(), in transformation.Rotation).To3();
        tri.B = Matrix.Multiply(tri.B.To4(), in transformation.Rotation).To3();
        tri.C = Matrix.Multiply(tri.C.To4(), in transformation.Rotation).To3();

        tri.A += new Vector3(.5f, 0f, .5f);
        tri.B += new Vector3(.5f, 0f, .5f);
        tri.C += new Vector3(.5f, 0f, .5f);

        tri.A -= camera.Position;
        tri.B -= camera.Position;
        tri.C -= camera.Position;

        tri.A += transformation.Offset;
        tri.B += transformation.Offset;
        tri.C += transformation.Offset;

        Vector3 line1 = tri.B - tri.A;
        Vector3 line2 = tri.C - tri.A;

        Vector3 normal = Vector3.Cross(line1, line2);
        normal = Vector3.Normalize(normal);

        Vector3 cameraRay = tri.A - camera.Position;

        if (Vector3.Dot(normal, cameraRay) >= float.Epsilon) return;

        Vector3 sunDirection = Vector3.Normalize(new Vector3(4f, 5f, -3f));

        Material material = materials[tri.MaterialIndex];

        if (SimpleLightning)
        {
            float dp = Math.Clamp(Vector3.Dot(sunDirection, normal), 0.3f, 1f);
            tri.Color = material.DiffuseColor * dp;
        }
        else
        {
            const float AmbientIntensity = 0.4f;
            const float DiffuseIntensity = 1.0f;
            const float SpecularIntensity = 5.0f;

            ColorF ambientComponent = AmbientIntensity * material.AmbientColor;

            ColorF diffuse = DiffuseIntensity * Math.Clamp(Vector3.Dot(sunDirection, normal), 0f, 1f) * material.DiffuseColor;

            ColorF specular = ColorF.Black;
            if (material.SpecularExponent > float.Epsilon)
            {
                Vector3 reflected = Vector3.Normalize(Vector3.Reflect(-sunDirection, normal));
                float specularConstant = MathF.Pow(Math.Max(Vector3.Dot(Vector3.Normalize(-cameraRay), reflected), 0f), material.SpecularExponent);
                specular = material.SpecularColor * specularConstant * SpecularIntensity;
            }

            tri.Color = (ambientComponent + diffuse + specular) * 1.4f;
        }

        tri.A = Matrix.Multiply(tri.A.To4(), in camera.ViewMatrix).To3();
        tri.B = Matrix.Multiply(tri.B.To4(), in camera.ViewMatrix).To3();
        tri.C = Matrix.Multiply(tri.C.To4(), in camera.ViewMatrix).To3();

        int clippedTriangles = Triangle3Ex.ClipAgainstPlane(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), tri, out clipped[0], out clipped[1]);

        for (int n = 0; n < clippedTriangles; n++)
        {
            Triangle4Ex clippedTriangle = (Triangle4Ex)clipped[n];

            clippedTriangle.A = Matrix.Multiply(clippedTriangle.A, in camera.ProjectionMatrix);
            clippedTriangle.B = Matrix.Multiply(clippedTriangle.B, in camera.ProjectionMatrix);
            clippedTriangle.C = Matrix.Multiply(clippedTriangle.C, in camera.ProjectionMatrix);

            clippedTriangle.TexA.X /= clippedTriangle.A.W;
            clippedTriangle.TexB.X /= clippedTriangle.B.W;
            clippedTriangle.TexC.X /= clippedTriangle.C.W;

            clippedTriangle.TexA.Y /= clippedTriangle.A.W;
            clippedTriangle.TexB.Y /= clippedTriangle.B.W;
            clippedTriangle.TexC.Y /= clippedTriangle.C.W;

            clippedTriangle.TexA.Z = 1f / clippedTriangle.A.W;
            clippedTriangle.TexB.Z = 1f / clippedTriangle.B.W;
            clippedTriangle.TexC.Z = 1f / clippedTriangle.C.W;

            if (clippedTriangle.A.W != 0f)
            { clippedTriangle.A /= clippedTriangle.A.W; }

            if (clippedTriangle.B.W != 0f)
            { clippedTriangle.B /= clippedTriangle.B.W; }

            if (clippedTriangle.C.W != 0f)
            { clippedTriangle.C /= clippedTriangle.C.W; }

            Vector4 viewOffset = new(1f, 1f, 0f, 1f);

            clippedTriangle.A += viewOffset;
            clippedTriangle.B += viewOffset;
            clippedTriangle.C += viewOffset;

            clippedTriangle.A *= 0.5f;
            clippedTriangle.B *= 0.5f;
            clippedTriangle.C *= 0.5f;

            projected.Add(clippedTriangle);
        }
    }

    public static Vector2 Project(
        IRenderer renderer,
        Vector3 point,
        Camera camera,
        out float depth)
    {
        point -= camera.Position;

        point = Matrix.Multiply(point.To4(), in camera.ViewMatrix).To3();

        Vector4 p4 = point.To4();
        p4 = Matrix.Multiply(p4, in camera.ProjectionMatrix);

        if (p4.W != 0f)
        { p4 /= p4.W; }

        Vector4 viewOffset = new(1f, 1f, 0f, 1f);

        p4 += viewOffset;

        p4 *= .5f;

        depth = 1f / p4.W;

        return (Vector2.One - p4.To2()) * new Vector2(renderer.Width, renderer.Height);
    }

    static void ClipAndDrawTriangles<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> trianglesToDraw, Image? image, Func<ColorF, TPixel> converter)
    {
        Span<Triangle4Ex> clipped = stackalloc Triangle4Ex[2];
        ValueList<Triangle4Ex> triangles = new(stackalloc Triangle4Ex[6]);

        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < trianglesToDraw.Length; i++)
        {
            Triangle4Ex tri = trianglesToDraw[i];

            triangles.Clear();
            triangles.Add(tri);

            int newTriangles = 1;

            for (int p = 0; p < 4; p++)
            {
                int trisToAdd = 0;
                while (newTriangles > 0)
                {
                    Triangle4Ex test = triangles[0];
                    triangles.RemoveAt(0);
                    newTriangles--;

                    switch (p)
                    {
                        case 0: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 1: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, screenSize.Height - 1, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 2: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 3: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(screenSize.Width - 1, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        default: break;
                    }

                    for (int w = 0; w < trisToAdd; w++)
                    { triangles.Add(clipped[w]); }
                }
                newTriangles = triangles.Count;
            }

            DrawTriangles(renderer, depth, triangles.AsSpan(), image, converter);
        }
    }

    static void ClipAndDrawTriangles(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> trianglesToDraw, Image? image)
    {
        Span<Triangle4Ex> clipped = stackalloc Triangle4Ex[2];
        ValueList<Triangle4Ex> triangles = new(stackalloc Triangle4Ex[4], 0);

        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < trianglesToDraw.Length; i++)
        {
            Triangle4Ex tri = trianglesToDraw[i];

            triangles.Clear();

            triangles.Add(tri);

            int newTriangles = 1;

            for (int p = 0; p < 4; p++)
            {
                int trisToAdd = 0;
                while (newTriangles > 0)
                {
                    Triangle4Ex test = triangles[0];
                    triangles.RemoveAt(0);
                    newTriangles--;

                    switch (p)
                    {
                        case 0: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 1: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, screenSize.Height - 1, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 2: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        case 3: trisToAdd = Triangle4Ex.ClipAgainstPlane(new Vector3(screenSize.Width - 1, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                        default: break;
                    }

                    for (int w = 0; w < trisToAdd; w++)
                    { triangles.Add(clipped[w]); }
                }
                newTriangles = triangles.Count;
            }

            DrawTriangles(renderer, depth, triangles.AsSpan(), image);
        }
    }

    static void DrawTriangles<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles, Image? image, Func<ColorF, TPixel> converter)
    {
        if (image.HasValue)
        { DrawTriangles(renderer, depth, triangles, image.Value, converter); }
        else
        { DrawTriangles(renderer, depth, triangles, converter); }
    }

    static void DrawTriangles<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles, Image image, Func<ColorF, TPixel> converter)
    {
        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < triangles.Length; i++)
        {
            renderer.FillTriangle<TPixel, ColorF>(
                (Span<float>)depth,
                (Coord)((Vector2.One - triangles[i].A.To2()) * screenSize).Round(), triangles[i].TexA.To3(),
                (Coord)((Vector2.One - triangles[i].B.To2()) * screenSize).Round(), triangles[i].TexB.To3(),
                (Coord)((Vector2.One - triangles[i].C.To2()) * screenSize).Round(), triangles[i].TexC.To3(),
                new ReadOnlySpan2D<ColorF>(image.Data.AsSpan(), image.Width, image.Height), converter);
        }
    }

    static void DrawTriangles<TPixel>(IOnlySetterRenderer<TPixel> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles, Func<ColorF, TPixel> converter)
    {
        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < triangles.Length; i++)
        {
            TPixel pixel = converter.Invoke(triangles[i].Color);
            renderer.Triangle(
                (Span<float>)depth,
                (Coord)((Vector2.One - triangles[i].A.To2()) * screenSize).Round(), triangles[i].TexA.Z,
                (Coord)((Vector2.One - triangles[i].B.To2()) * screenSize).Round(), triangles[i].TexB.Z,
                (Coord)((Vector2.One - triangles[i].C.To2()) * screenSize).Round(), triangles[i].TexC.Z,
                pixel);
        }
    }

    static void DrawTriangles(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles, Image? image)
    {
        if (image.HasValue)
        { DrawTriangles(renderer, depth, triangles, image.Value); }
        else
        { DrawTriangles(renderer, depth, triangles); }
    }

    static void DrawTriangles(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles, Image image)
    {
        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < triangles.Length; i++)
        {
            renderer.FillTriangle(
                (Span<float>)depth,
                (Coord)((Vector2.One - triangles[i].A.To2()) * screenSize).Round(), triangles[i].TexA.To3(),
                (Coord)((Vector2.One - triangles[i].B.To2()) * screenSize).Round(), triangles[i].TexB.To3(),
                (Coord)((Vector2.One - triangles[i].C.To2()) * screenSize).Round(), triangles[i].TexC.To3(),
                new ReadOnlySpan2D<ColorF>(image.Data.AsSpan(), image.Width, image.Height), v => v);
        }
    }

    static void DrawTriangles(IOnlySetterRenderer<ColorF> renderer, Buffer<float>? depth, ReadOnlySpan<Triangle4Ex> triangles)
    {
        Size screenSize = new(renderer.Width, renderer.Height);
        for (int i = 0; i < triangles.Length; i++)
        {
            renderer.Triangle(
                (Span<float>)depth,
                (Coord)((Vector2.One - triangles[i].A.To2()) * screenSize).Round(), triangles[i].TexA.Z,
                (Coord)((Vector2.One - triangles[i].B.To2()) * screenSize).Round(), triangles[i].TexB.Z,
                (Coord)((Vector2.One - triangles[i].C.To2()) * screenSize).Round(), triangles[i].TexC.Z,
                triangles[i].Color);
        }
    }
}
