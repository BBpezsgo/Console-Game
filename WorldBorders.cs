namespace ConsoleGame
{
    public class WorldBorders
    {
        public static bool Bounce(Rect rect, ref Vector position, ref Vector speed)
        {
            bool bounced = false;

            if (position.X < rect.Left)
            {
                position.X = rect.Left;
                speed.X = MathF.Abs(speed.X);
                bounced = true;
            }
            else if (position.X > rect.Right)
            {
                position.X = rect.Right;
                speed.X = -MathF.Abs(speed.X);
                bounced = true;
            }

            if (position.Y < rect.Top)
            {
                position.Y = rect.Top;
                speed.Y = MathF.Abs(speed.Y);
                bounced = true;
            }
            else if (position.Y > rect.Bottom)
            {
                position.Y = rect.Bottom;
                speed.Y = -MathF.Abs(speed.Y);
                bounced = true;
            }

            return bounced;
        }

        public static bool Clamp(Rect rect, ref Vector position)
        {
            bool clamped = false;

            if (position.X < rect.Left)
            {
                position.X = rect.Left;
                clamped = true;
            }
            else if (position.X > rect.Right)
            {
                position.X = rect.Right;
                clamped = true;
            }

            if (position.Y < rect.Top)
            {
                position.Y = rect.Top;
                clamped = true;
            }
            else if (position.Y > rect.Bottom)
            {
                position.Y = rect.Bottom;
                clamped = true;
            }

            return clamped;
        }
    }
}
