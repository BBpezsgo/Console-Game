using Win32;

namespace ConsoleGame
{
    public static class Mouse
    {
        public struct CompactMouse
        {
            uint states;

            public bool this[MouseButtonState button]
            {
                readonly get => (states & (uint)button) != 0;
                set
                {
                    states ^= states & (uint)button;
                    states |= (uint)button;
                }
            }

            public void Reset() => states = 0;
            internal void Set(uint states) => this.states = states;

            public static explicit operator uint(CompactMouse v) => v.states;
            public static explicit operator CompactMouse(uint v) => new() { states = v };
        }

        static CompactMouse pressed;
        static Coord mousePosition;

        public static int X => mousePosition.X;
        public static int Y => mousePosition.Y;

        public static Vector WorldPosition => Game.ConsoleToWorld(new Vector(mousePosition.X, mousePosition.Y));

        public static bool IsLeftDown => pressed[MouseButtonState.Left];
        public static bool IsMiddleDown => pressed[MouseButtonState.Middle];
        public static bool IsRightDown => pressed[MouseButtonState.Right];

        public static void Feed(MouseEvent e)
        {
            pressed = (CompactMouse)e.ButtonState;
            mousePosition = e.MousePosition;
        }
    }
}
