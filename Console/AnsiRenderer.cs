using System.Text;
using Win32.Common;
using Win32.LowLevel;

namespace ConsoleGame
{
    public partial struct Ansi
    {
        public const char ESC = '\x1B';
        public const char CSI = '[';
        public const char DCS = 'P';
        public const char OSC = ']';

        const string _ESC = "\x1B";
        const string _CSI = "[";

        #region General ASCII Codes
        /// <summary>
        /// Terminal bell
        /// </summary>
        public const char BEL = '\x07';
        /// <summary>
        /// Backspace
        /// </summary>
        public const char BS = '\x08';
        /// <summary>
        /// Horizontal TAB
        /// </summary>
        public const char HT = '\x09';
        /// <summary>
        /// Linefeed(newline)
        /// </summary>
        public const char LF = '\x0A';
        /// <summary>
        /// Vertical TAB
        /// </summary>
        public const char VT = '\x0B';
        /// <summary>
        /// Formfeed(also: New page NP)
        /// </summary>
        public const char FF = '\x0C';
        /// <summary>
        /// Carriage return
        /// </summary>
        public const char CR = '\x0D';
        /// <summary>
        /// Delete character
        /// </summary>
        public const char DEL = '\x7F';
        #endregion

        #region Cursor Controls
        /// <summary>
        /// moves cursor to home position (0, 0);
        /// </summary>
        public const string ResetCursor = $"{_ESC}{_CSI}H";
        public static string SetCursorPosition(ushort line, ushort column) => $"{ESC}{CSI}{line};{column}H";
        // /// <summary>
        // /// moves cursor to line #, column #
        // /// </summary>
        // public const string = $"{ESC}[{line};{column}f	
        /// <summary>
        /// moves cursor up # lines
        /// </summary>
        public static string MoveCursorUp(int lines) => $"{ESC}{_CSI}{lines}A";
        /// <summary>
        /// moves cursor down # lines
        /// </summary>
        public static string MoveCursorDown(int lines) => $"{ESC}{_CSI}{lines}B";
        /// <summary>
        /// moves cursor right # columns
        /// </summary>
        public static string MoveCursorRight(int columns) => $"{ESC}{_CSI}{columns}C";
        /// <summary>
        /// moves cursor left # columns
        /// </summary>
        public static string MoveCursorLeft(int columns) => $"{ESC}{_CSI}{columns}D";
        /// <summary>
        /// moves cursor to beginning of next line, # lines down
        /// </summary>
        public static string MoveCursorNextLine(int lines) => $"{ESC}{_CSI}{lines}E";
        /// <summary>
        /// moves cursor to beginning of previous line, # lines up
        /// </summary>
        public static string MoveCursorPreviousLine(int lines) => $"{ESC}{_CSI}{lines}F";
        /// <summary>
        /// moves cursor to column #
        /// </summary>
        public static string SetCursorPosColumn(int column) => $"{ESC}{_CSI}{column}G";
        /// <summary>
        /// request cursor position (reports as ESC[#;#R)
        /// </summary>
        public const string ReportCursorPosition = $"{_ESC}{_CSI}6n";
        /// <summary>
        /// moves cursor one line up, scrolling if needed
        /// </summary>
        public const string MoveCursorUpAndScroll = $"{_ESC} M";
        /// <summary>
        /// save cursor position (DEC)
        /// </summary>
        public const string SaveCursorPositionDEC = $"{_ESC} 7";
        /// <summary>
        /// restores the cursor to the last saved position (DEC)
        /// </summary>
        public const string RestoreCursorPositionDEC = $"{_ESC} 8";
        /// <summary>
        /// save cursor position (SCO)
        /// </summary>
        public const string SaveCursorPositionSCO = $"{_ESC}{_CSI}s";
        /// <summary>
        /// restores the cursor to the last saved position (SCO)
        /// </summary>
        public const string RestoreCursorPositionDSCO = $"{_ESC}{_CSI}u";

        #endregion

        #region Erase Functions
        // /// <summary>
        // /// erase in display (same as ESC[0J)
        // /// </summary>
        // public const string EraseInDisplay = $"{_ESC}{_CSI}J";
        /// <summary>
        /// erase from cursor until end of screen
        /// </summary>
        public const string EraseAfterCursor = $"{_ESC}{_CSI}0J";
        /// <summary>
        /// erase from cursor to beginning of screen
        /// </summary>
        public const string EraseUntilCursor = $"{_ESC}{_CSI}1J";
        /// <summary>
        /// erase entire screen
        /// </summary>
        public const string ClearScreen = $"{_ESC}{_CSI}2J";
        /// <summary>
        /// erase saved lines
        /// </summary>
        public const string EraseSavedLines = $"{_ESC}{_CSI}3J";
        /// <summary>
        /// erase in line (same as ESC{_CSI}0K)
        /// </summary>
        public const string EraseSavedLine = $"{_ESC}{_CSI}K";
        /// <summary>
        /// erase from cursor to end of line
        /// </summary>
        public const string EraseLineFromCursor = $"{_ESC}{_CSI}0K";
        /// <summary>
        /// erase start of line to the cursor
        /// </summary>
        public const string EraseLineUntilCursor = $"{_ESC}{_CSI}1K";
        /// <summary>
        /// erase the entire line
        /// </summary>
        public const string EraseLine = $"{_ESC}{_CSI}2K";
        #endregion

        #region Colors / Graphics Mode
        /// <summary>
        /// Set graphics modes for cell, separated by semicolon (;).
        /// </summary>
        public static string SetGraphicsModes(params uint[] cells) => $"{_ESC}{_CSI}{string.Join(';', cells)}m";

        public const string Reset = $"{_ESC}{_CSI}0m";

        public const string BoldSet = $"{_ESC}{_CSI}1m";
        public const string BoldReset = $"{_ESC}{_CSI}22m";

        public const string DimSet = $"{_ESC}{_CSI}2m";
        public const string DimReset = $"{_ESC}{_CSI}22m";

        public const string ItalicSet = $"{_ESC}{_CSI}3m";
        public const string ItalicReset = $"{_ESC}{_CSI}23m";

        public const string UnderlineSet = $"{_ESC}{_CSI}4m";
        public const string UnderlineReset = $"{_ESC}{_CSI}24m";

        public const string BlinkingSet = $"{_ESC}{_CSI}5m";
        public const string BlinkingReset = $"{_ESC}{_CSI}25m";

        public const string InverseSet = $"{_ESC}{_CSI}7m";
        public const string InverseReset = $"{_ESC}{_CSI}27m";

        public const string HiddenSet = $"{_ESC}{_CSI}8m";
        public const string HiddenReset = $"{_ESC}{_CSI}28m";

        public const string StrikethroughSet = $"{_ESC}{_CSI}9m";
        public const string StrikethroughReset = $"{_ESC}{_CSI}29m";
        #endregion

        /// <exception cref="WindowsException"/>
        public static void EnableVirtualTerminalSequences()
        {
            IntPtr handle = Kernel32.GetStdHandle(StdHandle.STD_OUTPUT_HANDLE);
            if (handle == Kernel32.INVALID_HANDLE_VALUE)
            { throw WindowsException.Get(); }
            uint mode = default;
            if (Kernel32.GetConsoleMode(handle, ref mode) == 0)
            { throw WindowsException.Get(); }
            mode |= (uint)0x0004;
            if (Kernel32.SetConsoleMode(handle, mode) == 0)
            { throw WindowsException.Get(); }
        }
        /// <exception cref="WindowsException"/>
        public static void DisableVirtualTerminalSequences()
        {
            IntPtr handle = Kernel32.GetStdHandle(StdHandle.STD_OUTPUT_HANDLE);
            if (handle == Kernel32.INVALID_HANDLE_VALUE)
            { throw WindowsException.Get(); }
            uint mode = default;
            if (Kernel32.GetConsoleMode(handle, ref mode) == 0)
            { throw WindowsException.Get(); }
            mode &= ~(uint)0x0004;
            if (Kernel32.SetConsoleMode(handle, mode) == 0)
            { throw WindowsException.Get(); }
        }

        public static StringBuilder SetGraphics(StringBuilder builder, params uint[] modes)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.AppendJoin(';', modes);
            builder.Append('m');
            return builder;
        }

        public static StringBuilder SetForegroundColor(StringBuilder builder, Color24 color)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('3');
            builder.Append('8');
            builder.Append(';');
            builder.Append('2');
            builder.Append(';');
            builder.Append(color.R);
            builder.Append(';');
            builder.Append(color.G);
            builder.Append(';');
            builder.Append(color.B);
            builder.Append('m');
            return builder;
        }

        public static StringBuilder SetBackgroundColor(StringBuilder builder, Color24 color)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('4');
            builder.Append('8');
            builder.Append(';');
            builder.Append('2');
            builder.Append(';');
            builder.Append(color.R);
            builder.Append(';');
            builder.Append(color.G);
            builder.Append(';');
            builder.Append(color.B);
            builder.Append('m');
            return builder;
        }

        public static StringBuilder SetForegroundColor(StringBuilder builder, byte colorCode)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('3');
            builder.Append('8');
            builder.Append(';');
            builder.Append('5');
            builder.Append(';');
            builder.Append(colorCode);
            return builder;
        }

        public static StringBuilder SetBackgroundColor(StringBuilder builder, byte colorCode)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('4');
            builder.Append('8');
            builder.Append(';');
            builder.Append('5');
            builder.Append(';');
            builder.Append(colorCode);
            builder.Append('m');
            return builder;
        }
    }

    [Flags]
    public enum AnsiGraphicsModes : uint
    {
        #region Foreground Colors
        ForegroundRed = 30 + 0b_0001,
        ForegroundGreen = 30 + 0b_0010,
        ForegroundBlue = 30 + 0b_0100,

        ForegroundYellow = 30 + 0b_0011,
        ForegroundMagenta = 30 + 0b_0101,
        ForegroundCyan = 30 + 0b_0110,

        ForegroundBlack = 30 + 0b_0000,
        ForegroundWhite = 30 + 0b_0111,
        ForegroundDefault = 39,
        #endregion

        #region Background Colors
        BackgroundRed = 40 + 0b_0001,
        BackgroundGreen = 40 + 0b_0010,
        BackgroundBlue = 40 + 0b_0100,

        BackgroundYellow = 40 + 0b_0011,
        BackgroundMagenta = 40 + 0b_0101,
        BackgroundCyan = 40 + 0b_0110,

        BackgroundBlack = 40 + 0b_0000,
        BackgroundWhite = 40 + 0b_0111,
        BackgroundDefault = 49,
        #endregion

        #region Foreground Bright Colors
        ForegroundBrightRed = 90 + 0b_0001,
        ForegroundBrightGreen = 90 + 0b_0010,
        ForegroundBrightBlue = 90 + 0b_0100,

        ForegroundBrightYellow = 90 + 0b_0011,
        ForegroundBrightMagenta = 90 + 0b_0101,
        ForegroundBrightCyan = 90 + 0b_0110,

        ForegroundBrightBlack = 90 + 0b_0000,
        ForegroundBrightWhite = 90 + 0b_0111,
        #endregion

        #region Background Bright Colors
        BackgroundBrightRed = 100 + 0b_0001,
        BackgroundBrightGreen = 100 + 0b_0010,
        BackgroundBrightBlue = 100 + 0b_0100,

        BackgroundBrightYellow = 100 + 0b_0011,
        BackgroundBrightMagenta = 100 + 0b_0101,
        BackgroundBrightCyan = 100 + 0b_0110,

        BackgroundBrightBlack = 100 + 0b_0000,
        BackgroundBrightWhite = 100 + 0b_0111,
        #endregion
    }

    public enum AnsiColorType
    {
        Extended,
        TrueColor,
    }

    public class AnsiRenderer : IRenderer<Color>
    {
        Color[] buffer;
        int width;
        int height;
        bool shouldResize;

        public AnsiColorType ColorType;
        public bool IsBloomEnabled;

        public Buffer<float> DepthBuffer { get; }
        public Color[] Buffer => buffer;

        public short Width => (short)width;
        public short Height => (short)height;

        public ref Color this[int i] => ref buffer[i];

        public event SimpleEventHandler? OnResized;

        public AnsiRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.buffer = new Color[width * height];
            this.DepthBuffer = new Buffer<float>(this);
            this.shouldResize = true;
            this.ColorType = AnsiColorType.TrueColor;
            this.IsBloomEnabled = true;
        }

        public static void Initialize()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
                Win32.ConsoleHandler.OutputFlags |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            }
        }

        public static void RenderExtended(Color[] buffer, int width, int height)
        {
            StringBuilder builder = new(width * height);
            byte prevColor = default;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Color24 color = (Color24)buffer[i];
                    byte bruh = Color24.ToAnsi256(color);

                    if ((x == 0 && y == 0) || prevColor != bruh)
                    {
                        Ansi.SetBackgroundColor(builder, bruh);
                        prevColor = bruh;
                    }

                    builder.Append(' ');
                }
            }
            Console.Out.Write(builder);
            Console.SetCursorPosition(0, 0);
        }

        public static void RenderTrueColor(Color[] buffer, int width, int height)
        {
            StringBuilder builder = new(width * height);
            Color24 prevColor = default;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Color24 color = (Color24)buffer[i];

                    if ((x == 0 && y == 0) || prevColor != color)
                    {
                        Ansi.SetBackgroundColor(builder, color);
                        prevColor = color;
                    }

                    builder.Append(' ');
                }
            }
            Console.Out.Write(builder);
            Console.SetCursorPosition(0, 0);
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer);
            DepthBuffer.Clear();
        }

        public bool IsVisible(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        public void Render()
        {
            if (IsBloomEnabled)
            { ColorUtils.Bloom(buffer, width, height, 5); }

            switch (ColorType)
            {
                case AnsiColorType.Extended:
                    RenderExtended(buffer, width, height);
                    break;
                case AnsiColorType.TrueColor:
                    RenderTrueColor(buffer, width, height);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void ShouldResize() => shouldResize = true;

        public bool Resize()
        {
            if (!shouldResize) return false;
            shouldResize = false;

            Console.Clear();

            width = Console.WindowWidth;
            height = Console.WindowHeight;

            buffer = new Color[width * height];

            DepthBuffer.Resize();

            OnResized?.Invoke();

            return true;
        }
    }
}
