﻿namespace ConsoleGame
{
    public static class Ascii
    {
        public struct Blocks
        {
            public const char Left = '▌';
            public const char Right = '▐';
            public const char Top = '▀';
            public const char Bottom = '▄';
            public const char Full = '█';
        }

        public static readonly char[] CircleNumbersOutline = new char[]
        {
            '⓪',
            '①',
            '②',
            '③',
            '④',
            '⑤',
            '⑥',
            '⑦',
            '⑧',
            '⑨',
            '⑩',
            '⑪',
            '⑫',
            '⑬',
            '⑭',
            '⑮',
            '⑯',
            '⑰',
            '⑱',
            '⑲',
            '⑳',
        };
        public static readonly char[] CircleNumbersFilled = new char[]
        {
            '⓿',
            '❶',
            '❷',
            '❸',
            '❹',
            '❺',
            '❻',
            '❼',
            '❽',
            '❾',
            '❿',
            '⓫',
            '⓬',
            '⓭',
            '⓮',
            '⓯',
            '⓰',
            '⓱',
            '⓲',
            '⓳',
            '⓴',
        };

        public static readonly char[] BlockShade = new char[]
        {
            '░',
            '▒',
            '▓',
        };

        public static readonly char[] CircleOutline = new char[]
        {
            '˚',
            '°',
            'o',
            '○',
            'O',
        };
        public static readonly char[] CircleFilled = new char[]
        {
            '·',
            '•',
            '●',
        };

        public static readonly char[] ShadeShort1 = new char[]
        {
            '`',
            '.',
            ',',
            '\'',
            '"',
            '-',
            '+',
            '&',
            '$',
            '#',
            '@',
        };
        public static readonly char[] ShadeShort2 = ".:-=+*#%@".ToCharArray();

        public static readonly char[] ShadeMedium = ".'`^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$".ToCharArray();

        public static readonly char[] ShadeLong = new char[] {
            '`',
            '.',
            '-',
            '\'',
            ':',
            '_',
            ',',
            '^',
            '=',
            ';',
            '>',
            '<',
            '+',
            '!',
            'r',
            'c',
            '*',
            '/',
            'z',
            '?',
            's',
            'L',
            'T',
            'v',
            ')',
            'J',
            '7',
            '(',
            '|',
            'F',
            'i',
            '{',
            'C',
            '}',
            'f',
            'I',
            '3',
            '1',
            't',
            'l',
            'u',
            '[',
            'n',
            'e',
            'o',
            'Z',
            '5',
            'Y',
            'x',
            'j',
            'y',
            'a',
            ']',
            '2',
            'E',
            'S',
            'w',
            'q',
            'k',
            'P',
            '6',
            'h',
            '9',
            'd',
            '4',
            'V',
            'p',
            'O',
            'G',
            'b',
            'U',
            'A',
            'K',
            'X',
            'H',
            'm',
            '8',
            'R',
            'D',
            '#',
            '$',
            'B',
            'g',
            '0',
            'M',
            'N',
            'W',
            'Q',
            '%',
            '&',
            '@',
        };
        public static readonly float[] ShadeLongV = new float[]
        {
            0.0751f,
            0.0829f,
            0.0848f,
            0.1227f,
            0.1403f,
            0.1559f,
            0.1850f,
            0.2183f,
            0.2417f,
            0.2571f,
            0.2852f,
            0.2902f,
            0.2919f,
            0.3099f,
            0.3192f,
            0.3232f,
            0.3294f,
            0.3384f,
            0.3609f,
            0.3619f,
            0.3667f,
            0.3737f,
            0.3747f,
            0.3838f,
            0.3921f,
            0.3960f,
            0.3984f,
            0.3993f,
            0.4075f,
            0.4091f,
            0.4101f,
            0.4200f,
            0.4230f,
            0.4247f,
            0.4274f,
            0.4293f,
            0.4328f,
            0.4382f,
            0.4385f,
            0.4420f,
            0.4473f,
            0.4477f,
            0.4503f,
            0.4562f,
            0.4580f,
            0.4610f,
            0.4638f,
            0.4667f,
            0.4686f,
            0.4693f,
            0.4703f,
            0.4833f,
            0.4881f,
            0.4944f,
            0.4953f,
            0.4992f,
            0.5509f,
            0.5567f,
            0.5569f,
            0.5591f,
            0.5602f,
            0.5602f,
            0.5650f,
            0.5776f,
            0.5777f,
            0.5818f,
            0.5870f,
            0.5972f,
            0.5999f,
            0.6043f,
            0.6049f,
            0.6093f,
            0.6099f,
            0.6465f,
            0.6561f,
            0.6595f,
            0.6631f,
            0.6714f,
            0.6759f,
            0.6809f,
            0.6816f,
            0.6925f,
            0.7039f,
            0.7086f,
            0.7235f,
            0.7302f,
            0.7332f,
            0.7602f,
            0.7834f,
            0.8037f,
            0.9999f,
        };

        public static readonly char[] BoxSidesSimple = new char[0b_1_0000]
        {
            //          TLBR
            ' ',  // 0b_0000
            '|',  // 0b_0001
            '-',  // 0b_0010
            '/',  // 0b_0011
            '|',  // 0b_0100
            '|',  // 0b_0101 invalid
            '\\', // 0b_0110
            '|',  // 0b_0111 invalid
            '-',  // 0b_1000
            '\\', // 0b_1001
            '-',  // 0b_1010 invalid
            '-',  // 0b_1011 invalid
            '/',  // 0b_1100
            '|',  // 0b_1101 invalid
            '-',  // 0b_1110 invalid
            ' ',  // 0b_1111 invalid
        };

        public static readonly char[] BoxSides = new char[0b_1_0000]
        {
            //          TLBR
            ' ',  // 0b_0000
            '│',  // 0b_0001
            '─',  // 0b_0010
            '┘',  // 0b_0011
            '│',  // 0b_0100
            '│',  // 0b_0101 invalid
            '└',  // 0b_0110
            '│',  // 0b_0111 invalid
            '─',  // 0b_1000
            '┐',  // 0b_1001
            '─',  // 0b_1010 invalid
            '─',  // 0b_1011 invalid
            '┌',  // 0b_1100
            '│',  // 0b_1101 invalid
            '─',  // 0b_1110 invalid
            ' ',  // 0b_1111 invalid
        };

        public static readonly char[] BoxSidesDouble = new char[0b_1_0000]
        {
            //          TLBR
            ' ',  // 0b_0000
            '║',  // 0b_0001
            '═',  // 0b_0010
            '╝',  // 0b_0011
            '║',  // 0b_0100
            '║',  // 0b_0101 invalid
            '╚',  // 0b_0110
            '║',  // 0b_0111 invalid
            '═',  // 0b_1000
            '╗',  // 0b_1001
            '═',  // 0b_1010 invalid
            '═',  // 0b_1011 invalid
            '╔',  // 0b_1100
            '║',  // 0b_1101 invalid
            '═',  // 0b_1110 invalid
            ' ',  // 0b_1111 invalid
        };

        public static readonly char[] BoxSidesShadow = new char[0b_1_0000]
        {
            //          TLBR
            ' ',  // 0b_0000
            '║',  // 0b_0001
            '═',  // 0b_0010
            '╝',  // 0b_0011
            '│',  // 0b_0100
            '?',  // 0b_0101 invalid
            '╘',  // 0b_0110
            '?',  // 0b_0111 invalid
            '─',  // 0b_1000
            '╖',  // 0b_1001
            '?',  // 0b_1010 invalid
            '?',  // 0b_1011 invalid
            '┌',  // 0b_1100
            '?',  // 0b_1101 invalid
            '?',  // 0b_1110 invalid
            ' ',  // 0b_1111 invalid
        };
    }
}