using System.Globalization;

namespace ConsoleGame
{
    public struct Yeah
    {
        public readonly float Min => min;
        public readonly float Max => max;
        public readonly float Sum => sum;
        public readonly float N => n;
        public readonly float Average => sum / n;

        float min;
        float max;
        float sum;
        int n;

        public void Add(float v)
        {
            n++;
            sum += v;
            min = Math.Min(min, v);
            max = Math.Max(max, v);
        }

        public override readonly string ToString() => $"{{ {min.ToString("0.00", CultureInfo.InvariantCulture)} - {max.ToString("0.00", CultureInfo.InvariantCulture)} (Avg: {Average.ToString("0.00", CultureInfo.InvariantCulture)}, N: {n}) }}";
    }
}
