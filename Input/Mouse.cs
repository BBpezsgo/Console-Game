using Win32;

namespace ConsoleGame
{
    public static class Mouse
    {
        public struct CompactMouse
        {
            uint states;

            public bool this[uint button]
            {
                readonly get => (states & button) != 0;
                set
                {
                    states ^= states & button;
                    states |= button;
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

        public static Vector WorldPosition => Game.ConsoleToWorld(new VectorInt(mousePosition.X, mousePosition.Y));

        public static bool IsLeftDown => pressed[MouseButton.Left];
        public static bool IsMiddleDown => pressed[MouseButton.Middle];
        public static bool IsRightDown => pressed[MouseButton.Right];

        public static void Feed(MouseEvent e)
        {
            pressed = (CompactMouse)e.ButtonState;
            mousePosition = e.MousePosition;
        }
    }
}
