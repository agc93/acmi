using System.IO;

namespace AceCore
{
    public class ModRootPath
    {
        public ModRootPath(string rootPathParam)
        {
            this.RootPath = rootPathParam.CleanPath();
            this.AbsolutePath = Path.IsPathRooted(RootPath)
                ? RootPath
                : Path.Combine(System.Environment.CurrentDirectory, RootPath);
        }

        public string RootPath { get; }
        public string AbsolutePath { get; }
    }
}