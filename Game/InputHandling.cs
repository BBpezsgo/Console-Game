using Win32;

namespace ConsoleGame
{
    public partial class Game
    {

        void OnBufferSize(WindowBufferSizeEvent e)
        {
            renderer?.ShouldResize();
        }

        void OnMouse(MouseEvent e)
        {
            Mouse.Feed(e);
        }

        void OnKey(KeyEvent e)
        {
            Keyboard.Feed(e);

            if (e.VirtualKeyCode == VirtualKeyCodes.ESCAPE)
            {
                Exit();
                return;
            }
        }

    }
}
