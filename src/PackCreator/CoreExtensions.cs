namespace PackCreator
{
    public static class CoreExtensions
    {
        public static string ToArgument(this string path) {
            return path.Contains(' ')
                ? $"\"{path}\""
                : path;
        }
    }
}