namespace ConsoleGame
{
    public interface ITimer
    {
        public float Elapsed { get; }
        public float Duration { get; }

        public void Do();
    }

    public class DynamicTimer : ITimer
    {
        public readonly float StartedTime;
        public readonly Delegate Callback;
        readonly float _duration;
        readonly object?[] Parameters;

        public float Elapsed => Time.Now - StartedTime;
        public float Duration => _duration;

        public unsafe DynamicTimer(float duration, Delegate callback, params object?[] parameters)
        {
            _duration = duration;
            StartedTime = Time.Now;
            Callback = callback;
            Parameters = parameters;
        }

        unsafe public void Do() => Callback.DynamicInvoke(Parameters);
    }

    public class Timer : ITimer
    {
        public readonly float StartedTime;
        unsafe public readonly delegate*<void> Callback;
        readonly float _duration;

        public float Elapsed => Time.Now - StartedTime;
        public float Duration => _duration;

        public unsafe Timer(float duration, delegate*<void> callback)
        {
            _duration = duration;
            StartedTime = Time.Now;
            Callback = callback;
        }

        unsafe public void Do() => Callback();
    }

    public class Timer<T1> : ITimer
    {
        public readonly float StartedTime;
        unsafe public readonly delegate*<T1, void> Callback;
        readonly float _duration;

        public float Elapsed => Time.Now - StartedTime;
        public float Duration => _duration;

        public readonly T1 Parameter1;

        public unsafe Timer(float duration, delegate*<T1, void> callback, T1 param1)
        {
            _duration = duration;
            StartedTime = Time.Now;
            Callback = callback;
            Parameter1 = param1;
        }

        unsafe public void Do() => Callback(Parameter1);
    }

    public class Timer<T1, T2> : ITimer
    {
        public readonly float StartedTime;
        unsafe public readonly delegate*<T1, T2, void> Callback;
        readonly float _duration;

        public readonly T1 Parameter1;
        public readonly T2 Parameter2;

        public float Elapsed => Time.Now - StartedTime;
        public float Duration => _duration;

        public unsafe Timer(float duration, delegate*<T1, T2, void> callback, T1 param1, T2 param2)
        {
            _duration = duration;
            StartedTime = Time.Now;
            Callback = callback;
            Parameter1 = param1;
            Parameter2 = param2;
        }

        unsafe public void Do() => Callback(Parameter1, Parameter2);
    }

    public class Timer<T1, T2, T3> : ITimer
    {
        public readonly float StartedTime;
        unsafe public readonly delegate*<T1, T2, T3, void> Callback;
        readonly float _duration;

        public float Elapsed => Time.Now - StartedTime;
        public float Duration => _duration;

        public readonly T1 Parameter1;
        public readonly T2 Parameter2;
        public readonly T3 Parameter3;

        public unsafe Timer(float duration, delegate*<T1, T2, T3, void> callback, T1 param1, T2 param2, T3 param3)
        {
            _duration = duration;
            StartedTime = Time.Now;
            Callback = callback;
            Parameter1 = param1;
            Parameter2 = param2;
            Parameter3 = param3;
        }

        unsafe public void Do() => Callback(Parameter1, Parameter2, Parameter3);
    }
}
