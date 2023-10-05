using Win32;

namespace ConsoleGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            if (false) // { 0.00 - 0.31(Avg: 0.09, N: 16581375) }
            {
                Yeah yeah = new();
                for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                    for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                        for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                        {
                            Color color = Color.From24bitRGB(r, g, b);
                            byte irgb = Color.To4bitIRGB_BruteForce(color);
                            float error = Color.Distance((Color)irgb, color);
                            yeah.Add(error);
                        }

                Console.WriteLine(yeah);
            }

            if (false) // { 0.00 - 1.35 (Avg: 0.28, N: 16581375) }
            {
                Yeah yeah = new();
                for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                    for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                        for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                        {
                            Color color = Color.From24bitRGB(r, g, b);
                            byte irgb = Color.To4bitIRGB(color);
                            float error = Color.Distance((Color)irgb, color);
                            yeah.Add(error);
                        }

                Console.WriteLine(yeah);
            }

            if (true) // { 0.00 - 0.38 (Avg: 0.10, N: 16581375) }
            {
                Yeah yeah = new();
                for (byte r = byte.MinValue; r < byte.MaxValue; r++)
                    for (byte g = byte.MinValue; g < byte.MaxValue; g++)
                        for (byte b = byte.MinValue; b < byte.MaxValue; b++)
                        {
                            Color color = Color.From24bitRGB(r, g, b);
                            byte irgb = Color.RgbiApprox(color);
                            float error = Color.Distance((Color)irgb, color);
                            yeah.Add(error);
                        }

                Console.WriteLine(yeah);
            }

            return;
            */

            try
            {
                Game game = new();
                game.Start();
            }
            catch (WindowsException windowsException)
            { windowsException.ShowMessageBox(); }
        }
    }
}
