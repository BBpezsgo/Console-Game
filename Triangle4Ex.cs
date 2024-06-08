namespace Maths;

public struct Triangle4Ex :
    IAdditionOperators<Triangle4Ex, Vector4, Triangle4Ex>,
    ISubtractionOperators<Triangle4Ex, Vector4, Triangle4Ex>
{
    public Vector4 A, B, C;
    public Vector4 TexA, TexB, TexC;
    public ColorF Color;

    public int MaterialIndex;

    public Triangle4Ex(Vector3 a, Vector3 b, Vector3 c) : this(a.To4(), b.To4(), c.To4()) { }
    public Triangle4Ex(Vector4 a, Vector4 b, Vector4 c)
    {
        A = a;
        B = b;
        C = c;

        TexA = a;
        TexB = b;
        TexC = c;

        Color = ColorF.Magenta;
        MaterialIndex = -1;
    }

    public static int ClipAgainstPlane(Vector3 planePoint, Vector3 planeNormal, Triangle4Ex triangleIn, out Triangle4Ex triangleOut1, out Triangle4Ex triangleOut2)
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

        Span<Vector3> insidePointsTex = stackalloc Vector3[3];
        int insidePointCountTex = 0;

        Span<Vector3> outsidePointsTex = stackalloc Vector3[3];
        int outsidePointCountTex = 0;

        float d0 = dist(triangleIn.A.To3());
        float d1 = dist(triangleIn.B.To3());
        float d2 = dist(triangleIn.C.To3());

        if (d0 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.A.To3();
            insidePointsTex[insidePointCountTex++] = triangleIn.TexA.To3();
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.A.To3();
            outsidePointsTex[outsidePointCountTex++] = triangleIn.TexA.To3();
        }

        if (d1 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.B.To3();
            insidePointsTex[insidePointCountTex++] = triangleIn.TexB.To3();
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.B.To3();
            outsidePointsTex[outsidePointCountTex++] = triangleIn.TexB.To3();
        }

        if (d2 >= 0f)
        {
            insidePoints[insidePointCount++] = triangleIn.C.To3();
            insidePointsTex[insidePointCountTex++] = triangleIn.TexC.To3();
        }
        else
        {
            outsidePoints[outsidePointCount++] = triangleIn.C.To3();
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

            triangleOut1.A = insidePoints[0].To4();
            triangleOut1.TexA = insidePointsTex[0].To4();

            triangleOut1.B = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t).To4();
            triangleOut1.TexB = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

            triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1], out t).To4();
            triangleOut1.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

            return 1;
        }

        if (insidePointCount == 2 && outsidePointCount == 1)
        {
            triangleOut1 = triangleIn;
            triangleOut2 = triangleIn;

            triangleOut1.A = insidePoints[0].To4();
            triangleOut1.TexA = insidePointsTex[0].To4();

            triangleOut1.B = insidePoints[1].To4();
            triangleOut1.TexB = insidePointsTex[1].To4();

            triangleOut1.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0], out t).To4();
            triangleOut1.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[0])) + insidePointsTex[0]).To4();

            triangleOut2.A = insidePoints[1].To4();
            triangleOut2.TexA = insidePointsTex[1].To4();

            triangleOut2.B = triangleOut1.C;
            triangleOut2.TexB = triangleOut1.TexC;

            triangleOut2.C = Vector.IntersectPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0], out t).To4();
            triangleOut2.TexC = ((t * (outsidePointsTex[0] - insidePointsTex[1])) + insidePointsTex[1]).To4();

            return 2;
        }

        return 0;
    }

    public static explicit operator Triangle4Ex(Triangle4 triangle) => new(triangle.A, triangle.B, triangle.C);
    public static explicit operator Triangle4Ex(Triangle3 tri) => new(tri.A.To4(), tri.B.To4(), tri.C.To4());
    public static explicit operator Triangle4Ex(Triangle3Ex tri) => new()
    {
        A = tri.A.To4(),
        B = tri.B.To4(),
        C = tri.C.To4(),

        TexA = tri.TexA.To4(),
        TexB = tri.TexB.To4(),
        TexC = tri.TexC.To4(),

        MaterialIndex = tri.MaterialIndex,
        Color = tri.Color,
    };

    public static explicit operator Triangle3(Triangle4Ex tri) => new(tri.A.To3(), tri.B.To3(), tri.C.To3());
    public static implicit operator Triangle4(Triangle4Ex triangle) => new(triangle.A, triangle.B, triangle.C);

    public static Triangle4Ex operator +(Triangle4Ex tri, Vector4 vec)
    {
        tri.A += vec;
        tri.B += vec;
        tri.C += vec;
        return tri;
    }

    public static Triangle4Ex operator -(Triangle4Ex tri, Vector4 vec)
    {
        tri.A -= vec;
        tri.B -= vec;
        tri.C -= vec;
        return tri;
    }
}
