using Win32;

namespace ConsoleGame
{
    public static class Keyboard
    {
        enum State : byte
        {
            None = 0b_00,
            Pressing = 0b_11,
            Pressed = 0b_10,
            Releasing = 0b_01,
        }

        readonly struct CompactKeyboard
        {
            readonly uint[] states;

            public CompactKeyboard()
            {
                states = new uint[16];
            }

            public State this[char key]
            {
                get => this[AsciiToKey[key]];
                set => this[AsciiToKey[key]] = value;
            }

            public State this[ushort key]
            {
                get
                {
                    key *= 2;
                    unchecked
                    {
                        int i = key / 32;
                        int bit = key % 32;

                        int segment = (int)states[i];
                        segment >>= bit;
                        segment &= 0b_11;

                        return (State)segment;
                    }
                }
                set
                {
                    key *= 2;
                    unchecked
                    {
                        int i = key / 32;
                        int bit = key % 32;

                        int segment = (int)states[i];
                        segment &= ~(0b_11 << (bit));
                        segment |= (byte)value << (bit);

                        states[i] = (uint)segment;
                    }
                }
            }

            public void Reset()
            {
                Array.Clear(states);
            }
        }

        static readonly Dictionary<char, ushort> AsciiToKey = new()
        {
            { '0', 0x30 },
            { '1', 0x31 },
            { '2', 0x32 },
            { '3', 0x33 },
            { '4', 0x34 },
            { '5', 0x35 },
            { '6', 0x36 },
            { '7', 0x37 },
            { '8', 0x38 },
            { '9', 0x39 },
            { 'A', 0x41 },
            { 'B', 0x42 },
            { 'C', 0x43 },
            { 'D', 0x44 },
            { 'E', 0x45 },
            { 'F', 0x46 },
            { 'G', 0x47 },
            { 'H', 0x48 },
            { 'I', 0x49 },
            { 'J', 0x4A },
            { 'K', 0x4B },
            { 'L', 0x4C },
            { 'M', 0x4D },
            { 'N', 0x4E },
            { 'O', 0x4F },
            { 'P', 0x50 },
            { 'Q', 0x51 },
            { 'R', 0x52 },
            { 'S', 0x53 },
            { 'T', 0x54 },
            { 'U', 0x55 },
            { 'V', 0x56 },
            { 'W', 0x57 },
            { 'X', 0x58 },
            { 'Y', 0x59 },
            { 'Z', 0x5A },
        };

        static readonly CompactKeyboard Keys = new();

        public static bool IsKeyPressed(char key) => IsKeyPressed(AsciiToKey[key]);
        public static bool IsKeyPressed(ushort key) => ((byte)Keys[key] & 0b_10) != 0;

        public static bool IsKeyHold(char key) => IsKeyHold(AsciiToKey[key]);
        public static bool IsKeyHold(ushort key) => Keys[key] == State.Pressed;

        public static bool IsKeyDown(char key, bool doTransition = true) => IsKeyDown(AsciiToKey[key], doTransition);
        public static bool IsKeyDown(ushort key, bool doTransition = true)
        {
            bool result = Keys[key] == State.Pressing;
            if (result && doTransition)
            { Keys[key] = State.Pressed; }
            return result;
        }

        public static bool IsKeyUp(char key, bool doTransition = true) => IsKeyUp(AsciiToKey[key], doTransition);
        public static bool IsKeyUp(ushort key, bool doTransition = true)
        {
            bool result = Keys[key] == State.Releasing;
            if (result && doTransition)
            { Keys[key] = State.None; }
            return result;
        }

        public static void Feed(KeyEvent e)
        {
            State state = Keys[e.VirtualKeyCode];
            bool isDown = e.IsDown != 0;

            State newState = state switch
            {
                State.None => isDown ? State.Pressing : State.None,
                State.Pressing => isDown ? State.Pressed : State.Releasing,
                State.Pressed => isDown ? State.Pressed : State.Releasing,
                State.Releasing => isDown ? State.Pressing : State.None,
                _ => state,
            };

            Keys[e.VirtualKeyCode] = newState;
        }
    }
}
