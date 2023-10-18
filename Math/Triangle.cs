using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public struct Triangle
    {
        public Vector3 A, B, C;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        unsafe public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle triangleIn, out Triangle triangleOut1, out Triangle triangleOut2)
        {
            planeNormal.Normalize();

            // Return signed shortest distance from point to plane, plane normal must be normalized
            float dist(Vector3 p)
            {
                Vector3 n = p.Normalized;
                return (planeNormal.X * p.X + planeNormal.Y * p.Y + planeNormal.Z * p.Z - Vector3.Dot(planeNormal, planePoint));
            };

            Vector3* insidePoints = stackalloc Vector3[3];
            int insidePointCount = 0;

            Vector3* outsidePoints = stackalloc Vector3[3];
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
                triangleOut1.B = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]);
                triangleOut1.C = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1]);

                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                triangleOut1.A = insidePoints[0];
                triangleOut1.B = insidePoints[1];
                triangleOut1.C = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]);

                triangleOut2.A = insidePoints[1];
                triangleOut2.B = triangleOut1.C;
                triangleOut2.C = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0]);

                return 2;
            }

            return 0;
        }

        unsafe public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, TriangleEx triangleIn, out TriangleEx triangleOut1, out TriangleEx triangleOut2)
        {
            planeNormal.Normalize();

            // Return signed shortest distance from point to plane, plane normal must be normalized
            float dist(Vector3 p)
            {
                Vector3 n = p.Normalized;
                return (planeNormal.X * p.X + planeNormal.Y * p.Y + planeNormal.Z * p.Z - Vector3.Dot(planeNormal, planePoint));
            };

            Vector3* insidePoints = stackalloc Vector3[3];
            int insidePointCount = 0;

            Vector3* outsidePoints = stackalloc Vector3[3];
            int outsidePointCount = 0;

            Vector3* insidePointsTex = stackalloc Vector3[3];
            int insidePointCountTex = 0;

            Vector3* outsidePointsTex = stackalloc Vector3[3];
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

            if (insidePointCount == 1 && outsidePointCount == 2)
            {
                triangleOut1 = triangleIn;

                float t;

                triangleOut1.PointA = insidePoints[0];
                triangleOut1.TexA = insidePointsTex[0];
                
                triangleOut1.PointB = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
                triangleOut1.TexB = t * (outsidePointsTex[0] - insidePointsTex[0]) + insidePointsTex[0];

                triangleOut1.PointC = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1], out t);
                triangleOut1.TexC = t * (outsidePointsTex[0] - insidePointsTex[0]) + insidePointsTex[0];
                return 1;
            }

            if (insidePointCount == 2 && outsidePointCount == 1)
            {
                float t;

                triangleOut1.PointA = insidePoints[0];
                triangleOut1.TexA = insidePointsTex[0];

                triangleOut1.PointB = insidePoints[1];
                triangleOut1.TexB = insidePointsTex[1];

                triangleOut1.PointC = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
                triangleOut1.TexC = t * (outsidePointsTex[0] - insidePointsTex[0]) + insidePointsTex[0];

                triangleOut2.PointA = insidePoints[1];
                triangleOut2.TexA = insidePointsTex[1];

                triangleOut2.PointB = triangleOut1.PointC;
                triangleOut2.TexB = triangleOut1.TexC;

                triangleOut2.PointC = Vector3.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0], out t);
                triangleOut2.TexC = t * (outsidePointsTex[0] - insidePointsTex[1]) + insidePointsTex[1];

                return 2;
            }

            return 0;
        }
    }
}
