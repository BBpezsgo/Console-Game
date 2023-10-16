namespace ConsoleGame
{
	public readonly struct TransparentImage
    {
		public readonly TransparentColor[] Data;
		public readonly int Width;
		public readonly int Height;

        public TransparentColor this[int x, int y] => Data[x + (Width * y)];
        public TransparentColor this[VectorInt point] => Data[point.X + (Width * point.Y)];

        public TransparentColor GetPixelWithUV(Vector uv, Vector point)
        {
            Vector transformedPoint = point / uv;
            transformedPoint *= new Vector(Width, Height);
            VectorInt imageCoord = Vector.Floor(transformedPoint);
            return this[imageCoord];
        }

        public TransparentImage(TransparentColor[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }

        public TransparentImage Duplicate()
        {
            TransparentColor[] data = new TransparentColor[Data.Length];
            Array.Copy(Data, data, Data.Length);
            return new TransparentImage(data, Width, Height);
        }
    }
}
