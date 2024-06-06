namespace ConsoleGame;

public static class Gradients
{
    public static Gradient Fire => new(new ColorF(1f, .8f, .3f), new ColorF(.4f, 0f, 0f));
    public static Gradient Smoke => new(new ColorF(.56f, .56f, .56f), new ColorF(.2f, .2f, .2f));
}
