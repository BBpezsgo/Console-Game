using System.Diagnostics;

namespace ConsoleGame
{
    public readonly struct SpeedTestResult
    {
        public readonly long[] Milliseconds;

        public SpeedTestResult(long[] milliseconds)
        {
            Milliseconds = milliseconds;
        }
        public SpeedTestResult Print()
        {
            for (int i = 0; i < Milliseconds.Length; i++)
            {
                Console.WriteLine($"#{i}: {Milliseconds[i].ToString(System.Globalization.CultureInfo.InvariantCulture)} ms");
            }
            return this;
        }
    }

    public readonly struct SpeedTest
    {
        const int Iterations = 10000000;

        public static SpeedTestResult Run(Action work)
        {
            Stopwatch sw = new();
            long[] result = new long[10];
            for (int i = 0; i < 10; i++)
            {
                sw.Restart();
                for (int j = 0; j < Iterations; j++)
                { work.Invoke(); }
                sw.Stop();
                result[i] = sw.ElapsedMilliseconds;
            }
            return new SpeedTestResult(result);
        }
    }
}
