using System.IO;

namespace AceCore.Assets
{
    internal static class PathExtensions
    {
        internal static string ChangeFileName(this FileInfo fileInfo, string newName) {
            var targetInstancePath = Path.Combine(fileInfo.Directory.FullName,
                $"{newName}{fileInfo.Extension}");
            return targetInstancePath;
        }

        internal static string ChangeFileName(this string path, string newName) {
            var targetInstancePath = Path.Combine(Path.GetDirectoryName(path),
                $"{newName}{Path.GetExtension(path)}");
            return targetInstancePath;
        }
    }
}