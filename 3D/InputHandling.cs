namespace ConsoleGame;

public partial class MeshRenderer
{
    bool shouldResizeRenderer;

    void OnBufferSize(WindowBufferSizeEvent e)
    {
        shouldResizeRenderer = true;
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
