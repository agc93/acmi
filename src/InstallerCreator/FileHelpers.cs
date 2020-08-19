using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InstallerCreator
{
    public static class FileHelpers
    {
        public static IEnumerable<FileInfo> GetAllPakFiles(string filePath) {
            var rootDir = new DirectoryInfo(filePath);
            var pakFiles = rootDir.EnumerateFiles("*.pak", SearchOption.AllDirectories);
            return pakFiles;
        }

        public static Dictionary<string, SkinIdentifier> GetSkins(string rootPath, out List<string> extraPaks) {
            var reader = new SkinReader();
            extraPaks = new List<string>();
            var pakFiles = GetAllPakFiles(rootPath);
            var skinFiles = new Dictionary<string, SkinIdentifier>();
            foreach (var file in pakFiles)
            {
                var ident = reader.ReadSkinSlot(file.FullName);
                if (ident != null) {
                    skinFiles.Add(Path.GetRelativePath(rootPath, file.FullName), ident);
                } else {
                    extraPaks.Add(Path.GetRelativePath(rootPath, file.FullName));
                }
            }
            return skinFiles;
        }

        public static Dictionary<string, SkinIdentifier> GetSkins(string rootPath) {
            var reader = new SkinReader();
            var pakFiles = GetAllPakFiles(rootPath);
            var skinFiles = new Dictionary<string, SkinIdentifier>();
            foreach (var file in pakFiles)
            {
                var ident = reader.ReadSkinSlot(file.FullName);
                if (ident != null) {
                    skinFiles.Add(Path.GetRelativePath(rootPath, file.FullName), ident);
                }
            }
            return skinFiles;
        }
    }
}