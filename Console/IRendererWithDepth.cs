namespace ConsoleGame
{
    public interface IRendererWithDepth
    {
        public Buffer<float> DepthBuffer { get; }
    }
}
