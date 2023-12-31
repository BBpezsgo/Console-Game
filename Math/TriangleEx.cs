﻿namespace ConsoleGame
{
    public struct TriangleEx
    {
        public Vector3 PointA, PointB, PointC;
        public Vector3 TexA, TexB, TexC;
        public Color Color;

        public int MaterialIndex;

        public TriangleEx(Vector3 a, Vector3 b, Vector3 c)
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

        public static implicit operator Triangle(TriangleEx triangle) => new(triangle.PointA, triangle.PointB, triangle.PointC);
        public static implicit operator TriangleEx(Triangle triangle) => new(triangle.A, triangle.B, triangle.C);
    }
}
