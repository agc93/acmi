namespace AceCore
{
    public static class CoreExtensions
    {
        public static string CleanPath(this string rootPath) {
            return rootPath.Trim('\"', '/', '\\');
        }
    }
}