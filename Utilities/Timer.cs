namespace ConsoleGame;

public class Timer
{
    readonly float _startedTime;
    readonly Action _callback;

    public float Duration { get; }
    public float Elapsed => Time.Now - _startedTime;

    public unsafe Timer(float duration, Action callback)
    {
        _startedTime = Time.Now;
        _callback = callback;
        Duration = duration;
    }

    public void Invoke() => _callback.Invoke();
}
