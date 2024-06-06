namespace ConsoleGame;

public partial class MeshRenderer
{
    void OnBufferSize(WindowBufferSizeEvent e)
    {
        if (renderer is ConsoleRenderer consoleRenderer)
        { consoleRenderer.ShouldResize(); }
    }

    void OnMouse(MouseEvent e)
    {
        ConsoleMouse.Feed(e);
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
