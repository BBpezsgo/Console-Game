using Win32;
using Win32.Common;
using System.Numerics;

namespace ConsoleGame
{
    public static class Renderer3D
    {
        static readonly bool SimpleLightning = false;

        public static unsafe void Render<TPixel>(Renderer<TPixel> renderer, Buffer<float>? depth, Mesh mesh, Camera camera, Image? image, Func<Color, TPixel> converter)
        {
            List<TriangleEx> trianglesToDraw = new();

            TriangleEx* clipped = stackalloc TriangleEx[2];

            DoMathWithTriangles(mesh.Triangles.ToArray(), mesh.Materials, camera, trianglesToDraw, clipped);

            ClipAndDrawTriangles(renderer, depth, trianglesToDraw, clipped, image, converter);
        }

        public static unsafe void Render(Renderer<Color> renderer, Buffer<float>? depth, Mesh mesh, Camera camera, Image? image)
        {
            List<TriangleEx> trianglesToDraw = new();

            TriangleEx* clipped = stackalloc TriangleEx[2];

            DoMathWithTriangles(mesh.Triangles.ToArray(), mesh.Materials, camera, trianglesToDraw, clipped);

            ClipAndDrawTriangles(renderer, depth, trianglesToDraw, clipped, image);
        }

        static unsafe void DoMathWithTriangles(TriangleEx[] triangles, Material[] materials, Camera camera, List<TriangleEx> trianglesToDraw, TriangleEx* clipped)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                TriangleEx tri = triangles[i];

                tri.PointA -= camera.CameraPosition;
                tri.PointB -= camera.CameraPosition;
                tri.PointC -= camera.CameraPosition;

                Vector3 normal, line1, line2;

                line1 = tri.PointB - tri.PointA;
                line2 = tri.PointC - tri.PointA;

                normal = Vector3.Cross(line1, line2);

                normal.Normalize();

                Vector3 cameraRay = tri.PointA - camera.CameraPosition;

                if (Vector3.Dot(normal, cameraRay) >= float.Epsilon) continue;

                Vector3 sunDirection = (0f, .7f, -1f);

                Material material = materials[tri.MaterialIndex];

                if (SimpleLightning)
                {
                    float dp = Math.Clamp(Vector3.Dot(sunDirection, normal), 0.3f, 1f);
                    tri.Color = material.DiffuseColor * dp;
                }
                else
                {
                    const float AmbientIntensity = 0.2f;
                    const float DiffuseIntensity = 1.0f;
                    const float SpecularIntensity = 5.0f;

                    Color ambientComponent = AmbientIntensity * material.AmbientColor;

                    Color diffuse = DiffuseIntensity * Math.Clamp(Vector3.Dot(sunDirection, normal), 0f, 1f) * material.DiffuseColor;

                    Color specular = Color.Black;
                    if (material.SpecularExponent > float.Epsilon)
                    {
                        Vector3 reflected = Vector3.Reflect(-sunDirection, normal).Normalized;
                        float specularConstant = MathF.Pow(Math.Max(Vector3.Dot(-cameraRay.Normalized, reflected), 0f), material.SpecularExponent);
                        specular = material.SpecularColor * specularConstant * SpecularIntensity;
                    }

                    tri.Color = (ambientComponent + diffuse + specular);
                }

                tri.PointA *= camera.ViewMatrix;
                tri.PointB *= camera.ViewMatrix;
                tri.PointC *= camera.ViewMatrix;

                int clippedTriangles = Triangle.ClipAgainstPlane(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), tri, out clipped[0], out clipped[1]);

                for (int n = 0; n < clippedTriangles; n++)
                {
                    TriangleEx clippedTriangle = clipped[n];

                    clippedTriangle.PointA *= camera.ProjectionMatrix;
                    clippedTriangle.PointB *= camera.ProjectionMatrix;
                    clippedTriangle.PointC *= camera.ProjectionMatrix;

                    clippedTriangle.TexA.X /= clippedTriangle.PointA.W;
                    clippedTriangle.TexB.X /= clippedTriangle.PointB.W;
                    clippedTriangle.TexC.X /= clippedTriangle.PointC.W;

                    clippedTriangle.TexA.Y /= clippedTriangle.PointA.W;
                    clippedTriangle.TexB.Y /= clippedTriangle.PointB.W;
                    clippedTriangle.TexC.Y /= clippedTriangle.PointC.W;

                    clippedTriangle.TexA.Z = 1f / clippedTriangle.PointA.W;
                    clippedTriangle.TexB.Z = 1f / clippedTriangle.PointB.W;
                    clippedTriangle.TexC.Z = 1f / clippedTriangle.PointC.W;

                    if (clippedTriangle.PointA.W != 0f)
                    { clippedTriangle.PointA /= clippedTriangle.PointA.W; }

                    if (clippedTriangle.PointB.W != 0f)
                    { clippedTriangle.PointB /= clippedTriangle.PointB.W; }

                    if (clippedTriangle.PointC.W != 0f)
                    { clippedTriangle.PointC /= clippedTriangle.PointC.W; }

                    Vector3 viewOffset = new(1f, 1f, 0f);

                    clippedTriangle.PointA += viewOffset;
                    clippedTriangle.PointB += viewOffset;
                    clippedTriangle.PointC += viewOffset;

                    clippedTriangle.PointA *= 0.5f;
                    clippedTriangle.PointB *= 0.5f;
                    clippedTriangle.PointC *= 0.5f;

                    trianglesToDraw.Add(clippedTriangle);
                }
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

        static unsafe void ClipAndDrawTriangles<TPixel>(Renderer<TPixel> renderer, Buffer<float>? depth, List<TriangleEx> trianglesToDraw, TriangleEx* clipped, Image? image, Func<Color, TPixel> converter)
        {
            SmallSize screenSize = renderer.Size;
            for (int i = 0; i < trianglesToDraw.Count; i++)
            {
                TriangleEx tri = trianglesToDraw[i];

                Queue<TriangleEx> triangles = new();
                triangles.Enqueue(tri);
                int newTriangles = 1;

                for (int p = 0; p < 4; p++)
                {
                    int trisToAdd = 0;
                    while (newTriangles > 0)
                    {
                        TriangleEx test = triangles.Dequeue();
                        newTriangles--;

                        switch (p)
                        {
                            case 0: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 1: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, screenSize.Height - 1, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 2: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 3: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(screenSize.Width - 1, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            default: break;
                        }

                        for (int w = 0; w < trisToAdd; w++)
                            triangles.Enqueue(clipped[w]);
                    }
                    newTriangles = triangles.Count;
                }

                DrawTriangles(renderer, depth, triangles.ToArray(), image, converter);
            }
        }

        static unsafe void ClipAndDrawTriangles(Renderer<Color> renderer, Buffer<float>? depth, List<TriangleEx> trianglesToDraw, TriangleEx* clipped, Image? image)
        {
            SmallSize screenSize = renderer.Size;
            for (int i = 0; i < trianglesToDraw.Count; i++)
            {
                TriangleEx tri = trianglesToDraw[i];

                Queue<TriangleEx> triangles = new();
                triangles.Enqueue(tri);
                int newTriangles = 1;

                for (int p = 0; p < 4; p++)
                {
                    int trisToAdd = 0;
                    while (newTriangles > 0)
                    {
                        TriangleEx test = triangles.Dequeue();
                        newTriangles--;

                        switch (p)
                        {
                            case 0: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 1: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, screenSize.Height - 1, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 2: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 3: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(screenSize.Width - 1, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            default: break;
                        }

                        for (int w = 0; w < trisToAdd; w++)
                            triangles.Enqueue(clipped[w]);
                    }
                    newTriangles = triangles.Count;
                }

                DrawTriangles(renderer, depth, triangles.ToArray(), image);
            }
        }

        static unsafe void DrawTriangles<TPixel>(Renderer<TPixel> renderer, Buffer<float>? depth, TriangleEx[] triangles, Image? image, Func<Color, TPixel> converter)
        {
            Coord screenSize = (Coord)renderer.Size;
            for (int i = 0; i < triangles.Length; i++)
            {
                if (image.HasValue)
                {
                    renderer.FillTriangle(
                        depth,
                        ((Vector2.One - (Vector2)triangles[i].PointA) * screenSize).Round(), triangles[i].TexA,
                        ((Vector2.One - (Vector2)triangles[i].PointB) * screenSize).Round(), triangles[i].TexB,
                        ((Vector2.One - (Vector2)triangles[i].PointC) * screenSize).Round(), triangles[i].TexC,
                        image.Value, converter);
                }
                else
                {
                    TPixel pixel = converter.Invoke(triangles[i].Color);
                    renderer.FillTriangle(
                        depth,
                        ((Vector2.One - (Vector2)triangles[i].PointA) * screenSize).Round(), triangles[i].TexA.Z,
                        ((Vector2.One - (Vector2)triangles[i].PointB) * screenSize).Round(), triangles[i].TexB.Z,
                        ((Vector2.One - (Vector2)triangles[i].PointC) * screenSize).Round(), triangles[i].TexC.Z,
                        pixel);
                }
            }
        }

        static unsafe void DrawTriangles(Renderer<Color> renderer, Buffer<float>? depth, TriangleEx[] triangles, Image? image)
        {
            Vector2Int screenSize = (Coord)renderer.Size;
            for (int i = 0; i < triangles.Length; i++)
            {
                if (image.HasValue)
                {
                    renderer.FillTriangle(
                        depth,
                        ((Vector2.One - (Vector2)triangles[i].PointA) * screenSize).Round(), triangles[i].TexA,
                        ((Vector2.One - (Vector2)triangles[i].PointB) * screenSize).Round(), triangles[i].TexB,
                        ((Vector2.One - (Vector2)triangles[i].PointC) * screenSize).Round(), triangles[i].TexC,
                        image.Value, v => v);
                }
                else
                {
                    renderer.FillTriangle(
                        depth,
                        ((Vector2.One - (Vector2)triangles[i].PointA) * screenSize).Round(), triangles[i].TexA.Z,
                        ((Vector2.One - (Vector2)triangles[i].PointB) * screenSize).Round(), triangles[i].TexB.Z,
                        ((Vector2.One - (Vector2)triangles[i].PointC) * screenSize).Round(), triangles[i].TexC.Z,
                        triangles[i].Color);
                }
            }
        }
    }
}
