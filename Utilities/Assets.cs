namespace ConsoleGame
{
    public static class Assets
    {
        public static string AssetsProject => @$"C:\Users\{Environment.UserName}\source\repos\ConsoleGame\Assets\";
        public static string AssetsRuntime => @$"{Path.Combine(Directory.GetCurrentDirectory(), "Assets").TrimEnd('\\')}\";

        public static string? GetAsset(string localPath)
        {
            string? path = null;

            if (Directory.Exists(AssetsProject))
            { path = Path.Combine(AssetsProject, localPath); }

            if (Directory.Exists(AssetsRuntime))
            { path = Path.Combine(AssetsRuntime, localPath); }

            if (!File.Exists(path)) return null;

            return path;
        }
    }
}
