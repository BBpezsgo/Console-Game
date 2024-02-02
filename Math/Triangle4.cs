using System.Numerics;

namespace ConsoleGame
{
    public struct Triangle4
    {
        public Vector4 A, B, C;

        public Triangle4(Vector3 a, Vector3 b, Vector3 c) : this(a.To4(), b.To4(), c.To4()) { }
        public Triangle4(Vector4 a, Vector4 b, Vector4 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle4 triangleIn, out Triangle4 triangleOut1, out Triangle4 triangleOut2)
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

            float d0 = dist(triangleIn.A.To3());
            float d1 = dist(triangleIn.B.To3());
            float d2 = dist(triangleIn.C.To3());

            if (d0 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.A.To3(); }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.A.To3(); }

            if (d1 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.B.To3(); }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.B.To3(); }

            if (d2 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.C.To3(); }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.C.To3(); }

            if (insidePointCount == 0)
            {
                return 0;
            }

            if (insidePointCount == 3)
            {
                triangleOut1 = triangleIn;
                return 1;
            }

            if (insidePointCount == 1 && outsidePointCount == 2)
            {
                triangleOut1 = triangleIn;

                triangleOut1.A = insidePoints[0].To4();
                triangleOut1.B = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]).To4();
                triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1]).To4();

                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                triangleOut1.A = insidePoints[0].To4();
                triangleOut1.B = insidePoints[1].To4();
                triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]).To4();

                triangleOut2.A = insidePoints[1].To4();
                triangleOut2.B = triangleOut1.C;
                triangleOut2.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0]).To4();

                return 2;
            }

            return 0;
        }

        public static implicit operator Triangle4(Triangle4Ex triangle) => new(triangle.PointA, triangle.PointB, triangle.PointC);
        public static implicit operator Triangle4Ex(Triangle4 triangle) => new(triangle.A, triangle.B, triangle.C);
      
        public static implicit operator Triangle4(Triangle3 triangle) => new(triangle.A.To4(), triangle.B.To4(), triangle.C.To4());
        public static implicit operator Triangle3(Triangle4 triangle) => new(triangle.A.To3(), triangle.B.To3(), triangle.C.To3());
    }
}
