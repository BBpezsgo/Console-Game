using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Win32;

namespace ConsoleGame
{
    public interface IRenderer
    {
        public short Width { get; }
        public short Height { get; }
        public int Size => Width * Height;

        VectorInt Rect { get; }

        public ref Win32.CharInfo this[int x, int y] => ref this[x + y * Width];
        public ref Win32.CharInfo this[VectorInt point] => ref this[point.X + point.Y * Width];
        public ref Win32.CharInfo this[int i] { get; }

        public void ClearBuffer();
        public void Render();

        public void FillTriangle(VectorInt a, VectorInt b, VectorInt c, ushort attributes, char character);
        public void FillTriangle(VectorInt a, VectorInt b, VectorInt c, CharInfo c1);
        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, CharInfo c);

        public void FillTriangle(
            VectorInt a, float depthA,
            VectorInt b, float depthB,
            VectorInt c, float depthC,
            CharInfo @char);
        public void FillTriangle(
            int x1, int y1, float w1,
            int x2, int y2, float w2,
            int x3, int y3, float w3,
            CharInfo @char);

        public void FillTriangle(
            VectorInt a, Vector3 texA,
            VectorInt b, Vector3 texB,
            VectorInt c, Vector3 texC,
            Image image);
        public void FillTriangle(
            int x1, int y1, float u1, float v1, float w1,
            int x2, int y2, float u2, float v2, float w2,
            int x3, int y3, float u3, float v3, float w3,
            Image image);
        bool IsVisible(VectorInt conPos);
    }
}
