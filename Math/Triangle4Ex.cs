using System.Numerics;

namespace ConsoleGame
{
    public struct Triangle4Ex
    {
        public Vector4 PointA, PointB, PointC;
        public Vector4 TexA, TexB, TexC;
        public Color Color;

        public int MaterialIndex;

        public Triangle4Ex(Vector3 a, Vector3 b, Vector3 c) : this(a.To4(), b.To4(), c.To4()) { }
        public Triangle4Ex(Vector4 a, Vector4 b, Vector4 c)
        {
            PointA = a;
            PointB = b;
            PointC = c;

            TexA = a;
            TexB = b;
            TexC = c;

            Color = Color.Magenta;
            MaterialIndex = -1;
        }

        public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle4Ex triangleIn, out Triangle4Ex triangleOut1, out Triangle4Ex triangleOut2)
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

            Span<Vector3> insidePointsTex = stackalloc Vector3[3];
            int insidePointCountTex = 0;

            Span<Vector3> outsidePointsTex = stackalloc Vector3[3];
            int outsidePointCountTex = 0;

            float d0 = dist(triangleIn.PointA.To3());
            float d1 = dist(triangleIn.PointB.To3());
            float d2 = dist(triangleIn.PointC.To3());

            if (d0 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointA.To3();
                insidePointsTex[insidePointCountTex++] = triangleIn.TexA.To3();
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointA.To3();
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexA.To3();
            }

            if (d1 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointB.To3();
                insidePointsTex[insidePointCountTex++] = triangleIn.TexB.To3();
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointB.To3();
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexB.To3();
            }

            if (d2 >= 0f)
            {
                insidePoints[insidePointCount++] = triangleIn.PointC.To3();
                insidePointsTex[insidePointCountTex++] = triangleIn.TexC.To3();
            }
            else
            {
                outsidePoints[outsidePointCount++] = triangleIn.PointC.To3();
                outsidePointsTex[outsidePointCountTex++] = triangleIn.TexC.To3();
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

                triangleOut1.PointA = insidePoints[0].To4();
                triangleOut1.TexA = insidePointsTex[0].To4();

                triangleOut1.PointB = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t).To4();
                triangleOut1.TexB = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

                triangleOut1.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1], out t).To4();
                triangleOut1.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                triangleOut1 = triangleIn;
                triangleOut2 = triangleIn;

                triangleOut1.PointA = insidePoints[0].To4();
                triangleOut1.TexA = insidePointsTex[0].To4();

                triangleOut1.PointB = insidePoints[1].To4();
                triangleOut1.TexB = insidePointsTex[1].To4();

                triangleOut1.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t).To4();
                triangleOut1.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

                triangleOut2.PointA = insidePoints[1].To4();
                triangleOut2.TexA = insidePointsTex[1].To4();

                triangleOut2.PointB = triangleOut1.PointC;
                triangleOut2.TexB = triangleOut1.TexC;

                triangleOut2.PointC = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0], out t).To4();
                triangleOut2.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[1])) + insidePointsTex[1]).To4();

                return 2;
            }

            return 0;
        }
    }
}
