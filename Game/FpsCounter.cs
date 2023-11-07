namespace ConsoleGame
{
    public struct FpsCounter
    {
        int[] Samples;
        int[] Copy;

        int N;

        public readonly int Value
        {
            get
            {
                if (N == 0) return 0;
                int sum = 0;
                for (int i = 0; i < Samples.Length; i++)
                { sum += Samples[i]; }
                return sum / N;
            }
        }

        public FpsCounter(int sampleCount)
        {
            Samples = new int[sampleCount];
            Copy = new int[sampleCount];
            N = 0;
        }

        public void Sample(int fps)
        {
            Array.Copy(Samples, 0, Copy, 1, Samples.Length - 1);
            Copy[0] = fps;

            int[] temp = Samples;
            Samples = Copy;
            Copy = temp;

            N = Math.Min(N + 1, Samples.Length);
        }
    }

}
