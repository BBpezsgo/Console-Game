using System.Runtime.InteropServices;

namespace ConsoleGame;

public class Png
{
    struct InformationChunk
    {
        public uint width;
        public uint height;
        public byte bitDepth;
        public byte colorType;
        public const byte compressionMethod = 0;
        public const byte filterMethod = 0;
        public byte interlaceMethod;

        /// <exception cref="NotImplementedException"/>
        public void Decode(ReadOnlySpan<byte> data, ref int i)
        {
            this.width = GetInt(data, ref i);
            this.height = GetInt(data, ref i);

            this.bitDepth = GetByte(data, ref i);
            this.colorType = GetByte(data, ref i);

            byte compressionMethod = GetByte(data, ref i);
            if (compressionMethod != 0)
            { throw new NotImplementedException($"Invalid compression method {compressionMethod}"); }

            byte filterMethod = GetByte(data, ref i);
            if (filterMethod != 0)
            { throw new NotImplementedException($"Invalid filter method {filterMethod}"); }

            this.interlaceMethod = GetByte(data, ref i);

            if (colorType != 6)
            { throw new NotImplementedException("We only support true-color with alpha"); }
            if (bitDepth != 8)
            { throw new NotImplementedException("We only support a bit depth of 8"); }
            if (interlaceMethod != 0)
            { throw new NotImplementedException("We only support no interlacing"); }
        }
    }

    InformationChunk Information;
    readonly List<byte> Data = new();

    static uint SwapEndianness(uint x) =>
        ((x & 0x000000ff) << 24) +  // First byte
        ((x & 0x0000ff00) << 8) +   // Second byte
        ((x & 0x00ff0000) >> 8) +   // Third byte
        ((x & 0xff000000) >> 24);   // Fourth byte

    static ushort SwapEndianness(ushort x) => (ushort)(
        ((x & 0x00ff) << 8) +       // First byte
        ((x & 0xff00) >> 8));       // Second byte

    static bool DecodeHeader(ReadOnlySpan<byte> data, ref int i)
    {
        i++;
        string signatureText = System.Text.Encoding.ASCII.GetString(data.Slice(i, 3));
        i += 3;
        i += 2;
        i++;
        i++;

        return signatureText == "PNG";
    }

    static uint GetInt(ReadOnlySpan<byte> data, ref int i)
    {
        uint v = BitConverter.ToUInt32(data.Slice(i, 4));
        i += 4;
        if (BitConverter.IsLittleEndian)
        { v = SwapEndianness(v); }
        return v;
    }

    static byte GetByte(ReadOnlySpan<byte> data, ref int i) => data[i++];

    bool DecodeChunk(ReadOnlySpan<byte> data, ref int i)
    {
        uint length = GetInt(data, ref i);
        string type = System.Text.Encoding.ASCII.GetString(data.Slice(i, 4));
        i += 4;
        ReadOnlySpan<byte> chunkData = data.Slice(i, (int)length);
        i += (int)length;
        // uint crc = BitConverter.ToUInt32(data.Slice(i, 4));
        i += 4;

        return DecodeChunkData(chunkData, type);
    }

    static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new(data);

        ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream zStream = new(input);

        MemoryStream output = new();
        zStream.CopyTo(output);

        return output.ToArray();
    }

    bool DecodeChunkData(ReadOnlySpan<byte> data, string type)
    {
        switch (type)
        {
            case "IHDR":
            {
                int i = 0;
                Information = new InformationChunk();
                Information.Decode(data, ref i);
                return true;
            }
            case "IDAT":
            {
                Data.AddRange(data.ToArray());
                return true;
            }
            default: return false;
        }
    }

    static int PaethPredictor(int a, int b, int c)
    {
        int p = a + b - c;

        int pa = Math.Abs(p - a);
        int pb = Math.Abs(p - b);
        int pc = Math.Abs(p - c);

        if (pa <= pb && pa <= pc)
        { return a; }
        else if (pb <= pc)
        { return b; }
        else
        { return c; }
    }

    const int BytesPerPixel = 4;

    TransparentImage LoadFileInternal(string file)
    {
        byte[] rawFileData = File.ReadAllBytes(file);
        Decode(rawFileData);
        ReadOnlySpan<byte> imageData = LoadImageData();
        return GenerateImage(imageData);
    }

    Image LoadFileInternal(string file, ColorF background)
    {
        byte[] rawFileData = File.ReadAllBytes(file);
        Decode(rawFileData);
        ReadOnlySpan<byte> imageData = LoadImageData();
        return GenerateImage(imageData, background);
    }

    void Decode(ReadOnlySpan<byte> rawFileData)
    {
        int i = 0;
        DecodeHeader(rawFileData, ref i);
        while (i < rawFileData.Length)
        {
            DecodeChunk(rawFileData, ref i);
        }
    }

    ReadOnlySpan<byte> LoadImageData()
    {
        byte[] imageData = Decompress(Data.ToArray());

        // int expectedLength = (int)(Information.height * (1 + (Information.width * BytesPerPixel)));

        int stride = (int)(Information.width * BytesPerPixel);

        List<byte> reconstructed = new((int)Information.height * stride);

        int ReconA(int r, int c) => c >= BytesPerPixel ? reconstructed[(r * stride) + c - BytesPerPixel] : 0;
        int ReconB(int r, int c) => r > 0 ? reconstructed[((r - 1) * stride) + c] : 0;
        int ReconC(int r, int c) => (r > 0 && c >= BytesPerPixel) ? reconstructed[((r - 1) * stride) + c - BytesPerPixel] : 0;

        int pixelIndex = 0;

        for (int r = 0; r < Information.height; r++)
        {
            //  for each scanline
            byte filterType = imageData[pixelIndex]; // first byte of scanline is filter type
            pixelIndex++;
            for (int c = 0; c < stride; c++)
            {
                //  for each byte in scanline
                byte p = imageData[pixelIndex];
                pixelIndex++;
                int reconstructedP;
                if (filterType == 0) //  None
                { reconstructedP = p; }
                else if (filterType == 1) //  Sub
                { reconstructedP = p + ReconA(r, c); }
                else if (filterType == 2) //  Up
                { reconstructedP = p + ReconB(r, c); }
                else if (filterType == 3) //  Average
                { reconstructedP = p + (ReconA(r, c) + ReconB(r, c)); }// 2
                else if (filterType == 4) //  Paeth
                { reconstructedP = p + PaethPredictor(ReconA(r, c), ReconB(r, c), ReconC(r, c)); }
                else
                { throw new NotImplementedException($"Unknown filter type {filterType}"); }
                reconstructed.Add((byte)(reconstructedP & byte.MaxValue)); // truncation to byte
            }
        }

        return CollectionsMarshal.AsSpan(reconstructed);
    }

    TransparentImage GenerateImage(ReadOnlySpan<byte> reconstructed)
    {
        ImmutableArray<TransparentColor>.Builder pixels = ImmutableArray.CreateBuilder<TransparentColor>();

        for (int j = 0; j < reconstructed.Length; j += BytesPerPixel)
        {
            int r = reconstructed[j + 0];
            int g = reconstructed[j + 1];
            int b = reconstructed[j + 2];
            int a = reconstructed[j + 3];

            pixels.Add(new TransparentColor(r / 255f, g / 255f, b / 255f, a / 255f));
            /*
            float alpha = (float)a / (float)byte.MaxValue;
            alpha = Math.Clamp(alpha, byte.MinValue, byte.MaxValue);

            Color color = Color.From24bitRGB(r, g, b) * alpha;
            Color backgroundColor_ = backgroundColor * (1f - alpha);
            pixels.Add(color + backgroundColor_);
            */
        }

        return new TransparentImage(pixels.ToImmutable(), (int)Information.width, (int)Information.height);
    }

    Image GenerateImage(ReadOnlySpan<byte> reconstructed, ColorF background)
    {
        ImmutableArray<ColorF>.Builder pixels = ImmutableArray.CreateBuilder<ColorF>();

        for (int j = 0; j < reconstructed.Length; j += BytesPerPixel)
        {
            int r = reconstructed[j + 0];
            int g = reconstructed[j + 1];
            int b = reconstructed[j + 2];
            int a = reconstructed[j + 3];

            float alpha = (float)a / (float)byte.MaxValue;
            alpha = Math.Clamp(alpha, byte.MinValue, byte.MaxValue);

            ColorF color = new ColorF(r / 255f, g / 255f, b / 255f) * alpha;
            ColorF backgroundColor_ = background * (1f - alpha);
            pixels.Add(color + backgroundColor_);
        }

        return new Image(pixels.ToImmutable(), (int)Information.width, (int)Information.height);
    }

    public static TransparentImage LoadFile(string file) => new Png().LoadFileInternal(file);
    public static Image LoadFile(string file, ColorF background) => new Png().LoadFileInternal(file, background);
}
