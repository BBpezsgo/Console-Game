using System.Text;

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
        const string _DCS = "P";
        const string _OSC = "]";

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
    }

    public partial struct Ansi
    {
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
        public static string MoveCursorUp(int lines) => $"{ESC}[{lines}A";
        /// <summary>
        /// moves cursor down # lines
        /// </summary>
        public static string MoveCursorDown(int lines) => $"{ESC}[{lines}B";
        /// <summary>
        /// moves cursor right # columns
        /// </summary>
        public static string MoveCursorRight(int columns) => $"{ESC}[{columns}C";
        /// <summary>
        /// moves cursor left # columns
        /// </summary>
        public static string MoveCursorLeft(int columns) => $"{ESC}[{columns}D";
        /// <summary>
        /// moves cursor to beginning of next line, # lines down
        /// </summary>
        public static string MoveCursorNextLine(int lines) => $"{ESC}[{lines}E";
        /// <summary>
        /// moves cursor to beginning of previous line, # lines up
        /// </summary>
        public static string MoveCursorPreviousLine(int lines) => $"{ESC}[{lines}F";
        /// <summary>
        /// moves cursor to column #
        /// </summary>
        public static string SetCursorPosColumn(int column) => $"{ESC}[{column}G";
        /// <summary>
        /// request cursor position (reports as ESC[#;#R)
        /// </summary>
        public const string ReportCursorPosition = $"{_ESC}[6n";
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
        public const string SaveCursorPositionSCO = $"{_ESC}[s";
        /// <summary>
        /// restores the cursor to the last saved position (SCO)
        /// </summary>
        public const string RestoreCursorPositionDSCO = $"{_ESC}[u";
    }

    public partial struct Ansi
    {
        /// <summary>
        /// erase in display (same as ESC[0J)
        /// </summary>
        public const string EraseInDisplay = $"{_ESC}[J";
        /// <summary>
        /// erase from cursor until end of screen
        /// </summary>
        public const string EraseAfterCursor = $"{_ESC}[0J";
        /// <summary>
        /// erase from cursor to beginning of screen
        /// </summary>
        public const string EraseUntilCursor = $"{_ESC}[1J";
        /// <summary>
        /// erase entire screen
        /// </summary>
        public const string ClearScreen = $"{_ESC}[2J";
        /// <summary>
        /// erase saved lines
        /// </summary>
        public const string EraseSavedLines = $"{_ESC}[3J";
        /// <summary>
        /// erase in line (same as ESC[0K)
        /// </summary>
        public const string EraseSavedLine = $"{_ESC}[K";
        /// <summary>
        /// erase from cursor to end of line
        /// </summary>
        public const string EraseLineFromCursor = $"{_ESC}[0K";
        /// <summary>
        /// erase start of line to the cursor
        /// </summary>
        public const string EraseLineUntilCursor = $"{_ESC}[1K";
        /// <summary>
        /// erase the entire line
        /// </summary>
        public const string EraseLine = $"{_ESC}[2K";
    }

    public partial struct Ansi
    {
        /// <summary>
        /// Set graphics modes for cell, separated by semicolon (;).
        /// </summary>
        public static string SetGraphicsModes(params uint[] cells) => $"{_ESC}[{string.Join(';', cells)}m";
        /// <summary>
        /// reset all modes (styles and colors)
        /// </summary>
        public const string Reset = $"{_ESC}[0m";
        /// <summary>
        /// set bold mode.
        /// </summary>
        public const string BoldSet = $"{_ESC}[1m";
        public const string BoldReset = $"{_ESC}[22m";
        /// <summary>
        /// set dim/faint mode.
        /// </summary>
        public const string DimSet = $"{_ESC}[2m";
        public const string DimReset = $"{_ESC}[22m";
        /// <summary>
        /// set italic mode.
        /// </summary>
        public const string ItalicSet = $"{_ESC}[3m";
        public const string ItalicReset = $"{_ESC}[23m";
        /// <summary>
        /// set underline mode.
        /// </summary>
        public const string UnderlineSet = $"{_ESC}[4m";
        public const string UnderlineReset = $"{_ESC}[24m";
        /// <summary>
        /// set blinking mode
        /// </summary>
        public const string BlinkingSet = $"{_ESC}[5m";
        public const string BlinkingReset = $"{_ESC}[25m";
        /// <summary>
        /// set inverse/reverse mode
        /// </summary>
        public const string InverseSet = $"{_ESC}[7m";
        public const string InverseReset = $"{_ESC}[27m";
        /// <summary>
        /// set hidden/invisible mode
        /// </summary>
        public const string HiddenSet = $"{_ESC}[8m";
        public const string HiddenReset = $"{_ESC}[28m";
        /// <summary>
        /// set strikethrough mode
        /// </summary>
        public const string StrikethroughSet = $"{_ESC}[9m";
        public const string StrikethroughReset = $"{_ESC}[29m";
    }

    [Flags]
    public enum AnsiGraphicsModes : uint
    {
        #region Foreground Colors
        ForegroundBlack = 30,
        ForegroundRed = 31,
        ForegroundGreen = 32,
        ForegroundYellow = 33,
        ForegroundBlue = 34,
        ForegroundMagenta = 35,
        ForegroundCyan = 36,
        ForegroundWhite = 37,
        ForegroundDefault = 39,
        #endregion

        #region Background Colors
        BackgroundBlack = 40,
        BackgroundRed = 41,
        BackgroundGreen = 42,
        BackgroundYellow = 43,
        BackgroundBlue = 44,
        BackgroundMagenta = 45,
        BackgroundCyan = 46,
        BackgroundWhite = 47,
        BackgroundDefault = 49,
        #endregion

        #region Foreground Bright Colors
        ForegroundBrightBlack = 90,
        ForegroundBrightRed = 91,
        ForegroundBrightGreen = 92,
        ForegroundBrightYellow = 93,
        ForegroundBrightBlue = 94,
        ForegroundBrightMagenta = 95,
        ForegroundBrightCyan = 96,
        ForegroundBrightWhite = 97,
        #endregion

        #region Background Bright Colors
        BackgroundBrightBlack = 100,
        BackgroundBrightRed = 101,
        BackgroundBrightGreen = 102,
        BackgroundBrightYellow = 103,
        BackgroundBrightBlue = 104,
        BackgroundBrightMagenta = 105,
        BackgroundBrightCyan = 106,
        BackgroundBrightWhite = 107,
        #endregion
    }

    public partial struct Ansi
    {
        public static StringBuilder SetGraphics(StringBuilder builder, params uint[] modes)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.AppendJoin(';', modes);
            builder.Append('m');
            return builder;
        }

        public static StringBuilder SetForegroundColor(StringBuilder builder, byte r, byte g, byte b)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('3'); builder.Append('8');
            builder.Append(';');
            builder.Append('2');
            builder.Append(';');
            builder.Append(r);
            builder.Append(';');
            builder.Append(g);
            builder.Append(';');
            builder.Append(b);
            builder.Append('m');
            return builder;
        }

        public static StringBuilder SetBackgroundColor(StringBuilder builder, byte r, byte g, byte b)
        {
            builder.Append(ESC);
            builder.Append(CSI);
            builder.Append('4');
            builder.Append('8');
            builder.Append(';');
            builder.Append('2');
            builder.Append(';');
            builder.Append(r);
            builder.Append(';');
            builder.Append(g);
            builder.Append(';');
            builder.Append(b);
            builder.Append('m');
            return builder;
        }
    }

    public class AnsiRenderer : IRenderer<Color>
    {
        Color[] buffer;
        short width;
        short height;

        public ref Color this[int i] => ref buffer[i];

        public Buffer<float> DepthBuffer { get; }

        public short Width => width;

        public short Height => height;

        public VectorInt Rect => new(width, height);

        public AnsiRenderer()
        {
            width = (short)Console.WindowWidth;
            height = (short)Console.WindowHeight;
            buffer = new Color[width * height];
            DepthBuffer = new Buffer<float>(this);
        }

        static void Threshold(Color[] buffer, int width, int height, Color threshold)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    buffer[i] = buffer[i] - threshold;
                    buffer[i].R = Math.Max(0, buffer[i].R);
                    buffer[i].G = Math.Max(0, buffer[i].G);
                    buffer[i].B = Math.Max(0, buffer[i].B);
                }
            }
        }

        static void Blur(Color24[] pix, int w, int h, int radius)
        {
            if (radius < 1)
            {
                return;
            }
            int wm = w - 1;
            int hm = h - 1;
            int wh = w * h;
            int div = radius + radius + 1;
            int[] r = new int[wh];
            int[] g = new int[wh];
            int[] b = new int[wh];
            int rsum, gsum, bsum, x, y, i, yp, yi, yw;
            Color24 p, p1, p2;
            int[] vmin = new int[Math.Max(w, h)];
            int[] vmax = new int[Math.Max(w, h)];

            int[] dv = new int[256 * div];
            for (i = 0; i < 256 * div; i++)
            {
                dv[i] = (i / div);
            }

            yw = yi = 0;

            for (y = 0; y < h; y++)
            {
                rsum = gsum = bsum = 0;
                for (i = -radius; i <= radius; i++)
                {
                    p = pix[yi + Math.Min(wm, Math.Max(i, 0))];
                    rsum += p.R;
                    gsum += p.G;
                    bsum += p.B;
                }
                for (x = 0; x < w; x++)
                {

                    r[yi] = dv[rsum];
                    g[yi] = dv[gsum];
                    b[yi] = dv[bsum];

                    if (y == 0)
                    {
                        vmin[x] = Math.Min(x + radius + 1, wm);
                        vmax[x] = Math.Max(x - radius, 0);
                    }
                    p1 = pix[yw + vmin[x]];
                    p2 = pix[yw + vmax[x]];

                    rsum += ((p1.R) - (p2.R));
                    gsum += ((p1.G) - (p2.G));
                    bsum += (p1.B) - (p2.B);
                    yi++;
                }
                yw += w;
            }

            for (x = 0; x < w; x++)
            {
                rsum = gsum = bsum = 0;
                yp = -radius * w;
                for (i = -radius; i <= radius; i++)
                {
                    yi = Math.Max(0, yp) + x;
                    rsum += r[yi];
                    gsum += g[yi];
                    bsum += b[yi];
                    yp += w;
                }
                yi = x;
                for (y = 0; y < h; y++)
                {
                    pix[yi] = new Color24((dv[rsum]), (dv[gsum]), dv[bsum]);
                    if (x == 0)
                    {
                        vmin[y] = Math.Min(y + radius + 1, hm) * w;
                        vmax[y] = Math.Max(y - radius, 0) * w;
                    }
                    p1 = ((Color24)(x + vmin[y]));
                    p2 = ((Color24)(x + vmax[y]));

                    rsum += r[(int)p1] - r[(int)p2];
                    gsum += g[(int)p1] - g[(int)p2];
                    bsum += b[(int)p1] - b[(int)p2];

                    yi += w;
                }
            }
        }

        static void Blur(Color[] pix, int w, int h, int radius)
        {
            if (radius < 1)
            {
                return;
            }
            int wm = w - 1;
            int hm = h - 1;
            int wh = w * h;
            int div = radius + radius + 1;
            int[] r = new int[wh];
            int[] g = new int[wh];
            int[] b = new int[wh];
            int rsum, gsum, bsum, x, y, i, yp, yi, yw;
            Color24 p, p1, p2;
            int[] vmin = new int[Math.Max(w, h)];
            int[] vmax = new int[Math.Max(w, h)];

            int[] dv = new int[256 * div];
            for (i = 0; i < 256 * div; i++)
            {
                dv[i] = (i / div);
            }

            yw = yi = 0;

            for (y = 0; y < h; y++)
            {
                rsum = gsum = bsum = 0;
                for (i = -radius; i <= radius; i++)
                {
                    p = (Color24)pix[yi + Math.Min(wm, Math.Max(i, 0))];
                    rsum += p.R;
                    gsum += p.G;
                    bsum += p.B;
                }
                for (x = 0; x < w; x++)
                {

                    r[yi] = dv[rsum];
                    g[yi] = dv[gsum];
                    b[yi] = dv[bsum];

                    if (y == 0)
                    {
                        vmin[x] = Math.Min(x + radius + 1, wm);
                        vmax[x] = Math.Max(x - radius, 0);
                    }
                    p1 = (Color24)pix[yw + vmin[x]];
                    p2 = (Color24)pix[yw + vmax[x]];

                    rsum += ((p1.R) - (p2.R));
                    gsum += ((p1.G) - (p2.G));
                    bsum += (p1.B) - (p2.B);
                    yi++;
                }
                yw += w;
            }

            for (x = 0; x < w; x++)
            {
                rsum = gsum = bsum = 0;
                yp = -radius * w;
                for (i = -radius; i <= radius; i++)
                {
                    yi = Math.Max(0, yp) + x;
                    rsum += r[yi];
                    gsum += g[yi];
                    bsum += b[yi];
                    yp += w;
                }
                yi = x;
                for (y = 0; y < h; y++)
                {
                    pix[yi] = new Color24((dv[rsum]), (dv[gsum]), dv[bsum]);
                    if (x == 0)
                    {
                        vmin[y] = Math.Min(y + radius + 1, hm) * w;
                        vmax[y] = Math.Max(y - radius, 0) * w;
                    }
                    p1 = ((Color24)(x + vmin[y]));
                    p2 = ((Color24)(x + vmax[y]));

                    rsum += r[(int)p1] - r[(int)p2];
                    gsum += g[(int)p1] - g[(int)p2];
                    bsum += b[(int)p1] - b[(int)p2];

                    yi += w;
                }
            }
        }

        static void Add(Color[] to, Color[] what)
        {
            for (int i = 0; i < what.Length; i++)
            { to[i] += what[i]; }
        }

        public static void Render(Color[] buffer, int width, int height)
        {
            Color[] bloomBuffer = new Color[buffer.Length];
            Array.Copy(buffer, bloomBuffer, buffer.Length);
            Threshold(bloomBuffer, width, height, Color.White);
            Blur(bloomBuffer, width, height, 5);
            Add(buffer, bloomBuffer);

            StringBuilder builder = new(width * height);
            TextWriter o = Console.Out;
            Color24 prevColor = default;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    Color24 color = (Color24)buffer[i];

                    if ((x == 0 && y == 0) || prevColor != color)
                    {
                        Ansi.SetBackgroundColor(builder, color.R, color.G, color.B);
                        prevColor = color;
                    }

                    builder.Append(' ');
                }
            }
            o.Write(builder);
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
            Render(buffer, width, height);
        }
    }
}
