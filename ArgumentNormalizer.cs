namespace ConsoleGame
{
    class ArgumentNormalizer
    {
        public readonly List<string> Result;

        NormalizerState State;
        string CurrentArg;

        enum NormalizerState
        {
            None,
            String,
        }

        public ArgumentNormalizer()
        {
            Result = new();
            State = NormalizerState.None;
            CurrentArg = string.Empty;
        }

        void FinishArgument()
        {
            string currentArg = CurrentArg.Trim();

            State = NormalizerState.None;
            CurrentArg = string.Empty;

            if (string.IsNullOrEmpty(currentArg)) return;

            Result.Add(currentArg);
        }

        public static string[] NormalizeArgs(params string[] args) => NormalizeArgs(string.Join(' ', args));
        public static string[] NormalizeArgs(string args)
        {
            ArgumentNormalizer argumentNormalizer = new();
            argumentNormalizer.NormalizeArgsInternal(args);
            return argumentNormalizer.Result.ToArray();
        }

        void NormalizeArgsInternal(string args)
        {
            Result.Clear();
            State = NormalizerState.None;
            CurrentArg = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];

                switch (c)
                {
                    case '"':
                        if (State == NormalizerState.String)
                        {
                            FinishArgument();
                            break;
                        }
                        State = NormalizerState.String;
                        break;
                    case '\t':
                    case ' ':
                        if (State == NormalizerState.String)
                        {
                            CurrentArg += c;
                            break;
                        }
                        FinishArgument();
                        break;
                    default:
                        CurrentArg += c;
                        break;
                }
            }

            FinishArgument();
        }
    }
}
