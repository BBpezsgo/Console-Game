using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame;

public readonly struct Image
{
    public readonly ImmutableArray<ColorF> Data;
    public readonly int Width;
    public readonly int Height;

    public ColorF this[int x, int y] => Data[x + (Width * y)];
    public ColorF this[Vector2Int point] => Data[point.X + (Width * point.Y)];

    public ColorF GetPixelWithUV(Vector2 uv, Vector2 point)
    {
        Vector2 transformedPoint = point / uv;
        transformedPoint *= new Vector2(Width, Height);
        Vector2Int imageCoord = transformedPoint.Floor();
        return this[imageCoord];
    }

    public Image(ImmutableArray<ColorF> data, int width, int height)
    {
        Data = data;
        Width = width;
        Height = height;
    }

    public Image(IEnumerable<TransparentColor> data, int width, int height)
    {
        Data = data.Select(v => (ColorF)v).ToImmutableArray();
        Width = width;
        Height = height;
    }

    public ColorF NormalizedSample(float texU, float texV)
    {
        int x = (int)(texU * Width);
        int y = (int)(texV * Height);

        x = Math.Clamp(x, 0, Width - 1);
        y = Math.Clamp(y, 0, Height - 1);

        return this[x, y];
    }

    [return: NotNullIfNotNull(nameof(imgFile))]
    public static Image? LoadFile(string? imgFile, ColorF background)
    {
        if (imgFile == null) return null;
        string extension = Path.GetExtension(imgFile);
        if (extension.Length <= 1) throw new NotImplementedException();
        extension = extension.ToLowerInvariant();
        return extension switch
        {
            ".png" => Png.LoadFile(imgFile, background),
            ".ppm" => Ppm.LoadFile(imgFile),
            _ => throw new NotImplementedException($"Unknown image file extension \"{extension}\""),
        };
    }

    [return: NotNullIfNotNull(nameof(imgFile))]
    public static TransparentImage? LoadFile(string? imgFile)
    {
        if (imgFile == null) return null;
        string extension = Path.GetExtension(imgFile);
        if (extension.Length <= 1) throw new NotImplementedException();
        extension = extension.ToLowerInvariant();
        return extension switch
        {
            ".png" => Png.LoadFile(imgFile),
            ".ppm" => (TransparentImage)Ppm.LoadFile(imgFile),
            _ => throw new NotImplementedException($"Unknown image file extension \"{extension}\""),
        };
    }
}
