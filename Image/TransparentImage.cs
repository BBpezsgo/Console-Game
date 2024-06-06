namespace ConsoleGame;

public readonly struct TransparentImage
{
    public readonly ImmutableArray<TransparentColor> Data;
    public readonly int Width;
    public readonly int Height;

    public TransparentColor this[int x, int y] => Data[x + (Width * y)];
    public TransparentColor this[Vector2Int point] => Data[point.X + (Width * point.Y)];

    public TransparentColor GetPixelWithUV(Vector2 uv, Vector2 point)
    {
        Vector2 transformedPoint = point / uv;
        transformedPoint *= new Vector2(Width, Height);
        Vector2Int imageCoord = transformedPoint.Floor();
        return this[imageCoord];
    }

    public TransparentImage(ImmutableArray<TransparentColor> data, int width, int height)
    {
        Data = data;
        Width = width;
        Height = height;
    }

    public TransparentImage(IEnumerable<ColorF> data, int width, int height)
    {
        Data = data.Select(v => (TransparentColor)v).ToImmutableArray();
        Width = width;
        Height = height;
    }

    public static explicit operator TransparentImage(Image image) => new(image.Data, image.Width, image.Height);
    public static explicit operator Image(TransparentImage image) => new(image.Data, image.Width, image.Height);
}
