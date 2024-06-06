namespace ConsoleGame;

public partial class Game
{
    void OnBufferSize(WindowBufferSizeEvent e)
    {
        // if (renderer is ConsoleRenderer consoleRenderer)
        // { consoleRenderer.ShouldResize(); }
    }

    void OnKey(KeyEvent e)
    {
        ConsoleKeyboard.Feed(e);

        if (e.VirtualKeyCode == VirtualKeyCode.Escape)
        {
            Exit();
            return;
        }
    }
}
