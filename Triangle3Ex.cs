namespace Maths;

public struct Triangle3Ex :
    IAdditionOperators<Triangle3Ex, Vector3, Triangle3Ex>,
    ISubtractionOperators<Triangle3Ex, Vector3, Triangle3Ex>
{
    public Vector3 A, B, C;

    public Vector2 TexA, TexB, TexC;
    public ColorF Color;
    public int MaterialIndex;

    public Triangle3Ex(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;

        TexA = a.To2();
        TexB = b.To2();
        TexC = c.To2();

        Color = ColorF.Magenta;
        MaterialIndex = -1;
    }

    public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle3Ex triangleIn, out Triangle3Ex triangleOut1, out Triangle3Ex triangleOut2)
    {
        triangleOut1 = default;
        triangleOut2 = default;

        planeNormal = Vector3.Normalize(planeNormal);

        // Return signed shortest distance from point to plane, plane normal must be normalized
        float dist(Vector3 p) => (planeNormal.X * p.X) + (planeNormal.Y * p.Y) + (planeNormal.Z * p.Z) - Vector3.Dot(planeNormal, planePoint);

        Span<Vector3> insidePoints = stackalloc Vector3[3];
        int insidePointCount = 0;

        Span<Vector3> outsidePoints = stackalloc Vector3[3];
        int outsidePointCount = 0;

        Span<Vector2> insidePointsTex = stackalloc Vector2[3];
        int insidePointCountTex = 0;

        Span<Vector2> outsidePointsTex = stackalloc Vector2[3];
        int outsidePointCountTex = 0;

        float d0 = dist(triangleIn.A);
        float d1 = dist(triangleIn.B);
        float d2 = dist(triangleIn.C);

        if (d0 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.A;
            insidePointsTex[insidePointCountTex++] = triangleIn.TexA;
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.A;
            outsidePointsTex[outsidePointCountTex++] = triangleIn.TexA;
        }

        if (d1 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.B;
            insidePointsTex[insidePointCountTex++] = triangleIn.TexB;
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.B;
            outsidePointsTex[outsidePointCountTex++] = triangleIn.TexB;
        }

        if (d2 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.C;
            insidePointsTex[insidePointCountTex++] = triangleIn.TexC;
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.C;
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

            triangleOut1.A = insidePoints[0];
            triangleOut1.TexA = insidePointsTex[0];

            triangleOut1.B = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
            triangleOut1.TexB = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

            triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1], out t);
            triangleOut1.TexC = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

            return 1;
        }

        if (insidePointCount == 2 && outsidePointCount == 1)
        {
            triangleOut1 = triangleIn;
            triangleOut2 = triangleIn;

            triangleOut1.A = insidePoints[0];
            triangleOut1.TexA = insidePointsTex[0];

            triangleOut1.B = insidePoints[1];
            triangleOut1.TexB = insidePointsTex[1];

            triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t);
            triangleOut1.TexC = (t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0];

            triangleOut2.A = insidePoints[1];
            triangleOut2.TexA = insidePointsTex[1];

            triangleOut2.B = triangleOut1.C;
            triangleOut2.TexB = triangleOut1.TexC;

            triangleOut2.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0], out t);
            triangleOut2.TexC = (t * (outsidePointsTex[0] - insidePointsTex[1])) + insidePointsTex[1];

            return 2;
        }

        return 0;
    }

    public static explicit operator Triangle3Ex(Triangle3 tri) => new(tri.A, tri.B, tri.C);
    public static explicit operator Triangle3Ex(Triangle4 tri) => new(tri.A.To3(), tri.B.To3(), tri.C.To3());
    public static explicit operator Triangle3Ex(Triangle4Ex tri) => new()
    {
        A = tri.A.To3(),
        B = tri.B.To3(),
        C = tri.C.To3(),

        TexA = tri.TexA.To2(),
        TexB = tri.TexB.To2(),
        TexC = tri.TexC.To2(),

        MaterialIndex = tri.MaterialIndex,
        Color = tri.Color,
    };

    public static implicit operator Triangle3(Triangle3Ex tri) => new(tri.A, tri.B, tri.C);
    public static explicit operator Triangle4(Triangle3Ex triangle) => new(triangle.A.To4(), triangle.B.To4(), triangle.C.To4());

    public static Triangle3Ex operator +(Triangle3Ex tri, Vector3 vec)
    {
        tri.A += vec;
        tri.B += vec;
        tri.C += vec;
        return tri;
    }

    public static Triangle3Ex operator -(Triangle3Ex tri, Vector3 vec)
    {
        tri.A -= vec;
        tri.B -= vec;
        tri.C -= vec;
        return tri;
    }
}
