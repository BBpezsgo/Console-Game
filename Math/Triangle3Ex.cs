using System.Numerics;

namespace ConsoleGame
{
    public struct Triangle3Ex
    {
        public Vector3 PointA, PointB, PointC;
        public Vector2 TexA, TexB, TexC;
        public Color Color;

        public int MaterialIndex;

        public Triangle3Ex(Vector3 a, Vector3 b, Vector3 c)
        {
            PointA = a;
            PointB = b;
            PointC = c;

            TexA = a.To2();
            TexB = b.To2();
            TexC = c.To2();

            Color = Color.Magenta;
            MaterialIndex = -1;
        }

        public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle3Ex triangleIn, out Triangle3Ex triangleOut1, out Triangle3Ex triangleOut2)
        {
            triangleOut1 = default;
            triangleOut2 = default;

            planeNormal = Vector3.Normalize(planeNormal);

            // Return signed shortest distance from point to plane, plane normal must be normalized
            float dist(Vector3 p)
            {
                return planeNormal.X * p.X + planeNormal.Y * p.Y + planeNormal.Z * p.Z - Vector3.Dot(planeNormal, planePoint);
            };

            Span<Vector3> insidePoints = stackalloc Vector3[3];
            int insidePointCount = 0;

            Span<Vector3> outsidePoints = stackalloc Vector3[3];
            int outsidePointCount = 0;

            Span<Vector2> insidePointsTex = stackalloc Vector2[3];
            int insidePointCountTex = 0;

            Span<Vector2> outsidePointsTex = stackalloc Vector2[3];
            int outsidePointCountTex = 0;

            float d0 = dist(triangleIn.PointA);
            float d1 = dist(triangleIn.PointB);
            float d2 = dist(triangleIn.PointC);

            if (d0 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointA;
                insidePointsTex[insidePointCountTex++] = triangleIn.TexA;
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointA;
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexA;
            }

            if (d1 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointB;
                insidePointsTex[insidePointCountTex++] = triangleIn.TexB;
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointB;
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexB;
            }

            if (d2 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointC;
                insidePointsTex[insidePointCountTex++] = triangleIn.TexC;
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointC;
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexC;
            }

            if (insidePointCount == 0)
            {
                return 0;
            }

            if (insidePointCount == 3)
            {
                triangleOut1 = triangleIn;
                return 1;
            }

            float t;

            if (insidePointCount == 1 && outsidePointCount == 2)
            {
                triangleOut1 = triangleIn;

                triangleOut1.PointA = insidePoints[0];
                triangleOut1.TexA = insidePointsTex[0];

                triangleOut1.PointB = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
                triangleOut1.TexB = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

                triangleOut1.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1], out t);
                triangleOut1.TexC = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                triangleOut1 = triangleIn;
                triangleOut2 = triangleIn;

                triangleOut1.PointA = insidePoints[0];
                triangleOut1.TexA = insidePointsTex[0];

                triangleOut1.PointB = insidePoints[1];
                triangleOut1.TexB = insidePointsTex[1];

                triangleOut1.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
                triangleOut1.TexC = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

                triangleOut2.PointA = insidePoints[1];
                triangleOut2.TexA = insidePointsTex[1];

                triangleOut2.PointB = triangleOut1.PointC;
                triangleOut2.TexB = triangleOut1.TexC;

                triangleOut2.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0], out t);
                triangleOut2.TexC = (t * (outsidePointsTex[0] - insidePointsTex[1])) + insidePointsTex[1];

                return 2;
            }

            return 0;
        }

        public static implicit operator Triangle3Ex(Triangle3 tri) => new(tri.A, tri.B, tri.C);
        public static implicit operator Triangle3(Triangle3Ex tri) => new(tri.PointA, tri.PointB, tri.PointC);

        public static implicit operator Triangle3Ex(Triangle4Ex tri) => new()
        {
            PointA = tri.PointA.To3(),
            PointB = tri.PointB.To3(),
            PointC = tri.PointC.To3(),
                                  
            TexA = tri.TexA.To2(),
            TexB = tri.TexB.To2(),
            TexC = tri.TexC.To2(),

            MaterialIndex = tri.MaterialIndex,
            Color = tri.Color,
        };
        public static implicit operator Triangle4Ex(Triangle3Ex tri) => new()
        {
            PointA = tri.PointA.To4(),
            PointB = tri.PointB.To4(),
            PointC = tri.PointC.To4(),

            TexA = tri.TexA.To4(),
            TexB = tri.TexB.To4(),
            TexC = tri.TexC.To4(),

            MaterialIndex = tri.MaterialIndex,
            Color = tri.Color,
        };
    }
}
