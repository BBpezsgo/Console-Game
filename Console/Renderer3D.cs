namespace ConsoleGame
{
    public class Renderer3D
    {
        readonly ConsoleRenderer Renderer;
        VectorInt ScreenSize => Renderer.Rect;

        public Renderer3D(ConsoleRenderer renderer)
        {
            this.Renderer = renderer;
        }

        unsafe public void Render(Mesh mesh, Camera camera, Image? image)
        {
            List<TriangleEx> trianglesToDraw = new();

            TriangleEx* clipped = stackalloc TriangleEx[2];

            this.DoMathWithTriangles(mesh.Triangles, camera, trianglesToDraw, clipped);

            this.ClipAndDrawTriangles(trianglesToDraw, clipped, image);
        }

        unsafe void DoMathWithTriangles(List<TriangleEx> triangles, Camera camera, List<TriangleEx> trianglesToDraw, TriangleEx* clipped)
        {
            for (int i = 0; i < triangles.Count; i++)
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

                float dp = Math.Clamp(Vector3.Dot(sunDirection, normal), 0.3f, 1f);
                tri.Color = tri.DiffuseColor * dp;

                /*
                const float AmbientIntensity = 1.0f;
                const float DiffuseIntensity = 1.0f;
                const float SpecularIntensity = 2.0f;

                // Calculate the ambient term: 
                Color ambient = AmbientIntensity * tri.AmbientColor;
                // Calculate the diffuse term: 
                Color diffuse = DiffuseIntensity * dp * Color.White;
                // Calculate the reflection vector: 
                Vector3 r = (2 * Vector3.Dot(normal, -sunDirection) * normal + sunDirection).Normalized;
                // Calculate the speculate component: 
                Color specular = tri.SpecularExponent > float.Epsilon ? (MathF.Pow(Vector3.Dot(r, cameraRay.Normalized), Math.Clamp(tri.SpecularExponent, 1f, 50f)) * SpecularIntensity) * Color.White : Color.Black;
                // Calculate final color: 
                tri.Color = (ambient + diffuse + specular) * tri.DiffuseColor;
                */

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

        unsafe void ClipAndDrawTriangles(List<TriangleEx> trianglesToDraw, TriangleEx* clipped, Image? image)
        {
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
                            case 1: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, ScreenSize.Y - 1, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 2: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            case 3: trisToAdd = Triangle.ClipAgainstPlane(new Vector3(ScreenSize.X - 1, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), test, out clipped[0], out clipped[1]); break;
                            default: break;
                        }

                        for (int w = 0; w < trisToAdd; w++)
                            triangles.Enqueue(clipped[w]);
                    }
                    newTriangles = triangles.Count;
                }

                this.DrawTriangles(triangles.ToArray(), image);
            }
        }

        static readonly Image SolidImage = new(new Color[1], 1, 1);

        unsafe void DrawTriangles(TriangleEx[] triangles, Image? image)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                if (image.HasValue)
                {
                    Renderer.FillTriangle(
                        ((Vector.One - (Vector)triangles[i].PointA) * ScreenSize).Round(), triangles[i].TexA,
                        ((Vector.One - (Vector)triangles[i].PointB) * ScreenSize).Round(), triangles[i].TexB,
                        ((Vector.One - (Vector)triangles[i].PointC) * ScreenSize).Round(), triangles[i].TexC,
                        image.Value);
                }
                else
                {
                    SolidImage.Data[0] = triangles[i].Color;
                    Renderer.FillTriangle(
                        ((Vector.One - (Vector)triangles[i].PointA) * ScreenSize).Round(), triangles[i].TexA,
                        ((Vector.One - (Vector)triangles[i].PointB) * ScreenSize).Round(), triangles[i].TexB,
                        ((Vector.One - (Vector)triangles[i].PointC) * ScreenSize).Round(), triangles[i].TexC,
                        SolidImage);
                }

                /*
                Renderer.FillTriangle(
                    ((Vector.One - (Vector)triangles_[j].PointA) * ScreenSize).Round(),
                    ((Vector.One - (Vector)triangles_[j].PointB) * ScreenSize).Round(),
                    ((Vector.One - (Vector)triangles_[j].PointC) * ScreenSize).Round(),
                    Color.ToCharacterShaded(triangles_[j].Color));
                */

                /*
                renderer.DrawLines(new VectorInt[]
                {
                    ((Vector.One - (Vector)tri.A) * screenSize).Round(),
                    ((Vector.One - (Vector)tri.B) * screenSize).Round(),
                    ((Vector.One - (Vector)tri.C) * screenSize).Round(),
                }, ByteColor.Magenta << 4, ' ', true);
                */
            }
        }
    }
}
