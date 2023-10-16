namespace ConsoleGame
{
	public readonly struct Image
	{
		public readonly Color[] Data;
		public readonly int Width;
		public readonly int Height;

        public Color this[int x, int y] => Data[x + (Width * y)];
        public Color this[VectorInt point] => Data[point.X + (Width * point.Y)];

        public Color GetPixelWithUV(Vector uv, Vector point)
        {
            Vector transformedPoint = point / uv;
            transformedPoint *= new Vector(Width, Height);
            VectorInt imageCoord = Vector.Floor(transformedPoint);
            return this[imageCoord];
        }

        public Image(Color[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }

        public Image Duplicate()
        {
            Color[] data = new Color[Data.Length];
            Array.Copy(Data, data, Data.Length);
            return new Image(data, Width, Height);
        }
    }
}
