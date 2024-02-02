using System.Numerics;

namespace ConsoleGame
{
    public struct Triangle3
    {
        public Vector3 A, B, C;

        public Triangle3(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle3 triangleIn, out Triangle3 triangleOut1, out Triangle3 triangleOut2)
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

            float d0 = dist(triangleIn.A);
            float d1 = dist(triangleIn.B);
            float d2 = dist(triangleIn.C);

            if (d0 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.A; }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.A; }

            if (d1 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.B; }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.B; }

            if (d2 >= 0f)
            { insidePoints[insidePointCount++] = triangleIn.C; }
            else
            { outsidePoints[outsidePointCount++] = triangleIn.C; }

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

                triangleOut1.A = insidePoints[0];
                triangleOut1.B = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]);
                triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1]);

                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                triangleOut1.A = insidePoints[0];
                triangleOut1.B = insidePoints[1];
                triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]);

                triangleOut2.A = insidePoints[1];
                triangleOut2.B = triangleOut1.C;
                triangleOut2.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0]);

                return 2;
            }

            return 0;
        }
    }
}
