using Win32;
using Win32.LowLevel;

namespace ConsoleGame
{
    public partial class Game
    {
        void OnBufferSize(WindowBufferSizeEvent e)
        {
            if (renderer is ConsoleRenderer consoleRenderer)
            { consoleRenderer.ShouldResize(); }
        }

        void OnMouse(MouseEvent e)
        {
            Mouse.Feed(e);
        }

        void OnKey(KeyEvent e)
        {
            Keyboard.Feed(e);

            if (e.VirtualKeyCode == VirtualKeyCode.ESCAPE)
            {
                Exit();
                return;
            }
        }

    }
}
