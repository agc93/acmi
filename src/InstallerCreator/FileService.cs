using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;

namespace InstallerCreator
{
    public class FileService
    {
        public IEnumerable<FileInfo> GetAllPakFiles(string filePath) {
            var rootDir = new DirectoryInfo(filePath);
            var pakFiles = rootDir.EnumerateFiles("*.pak", SearchOption.AllDirectories);
            return pakFiles;
        }

        public SkinPack GetFiles(string rootPath, bool allowMultiple = false) {
            var reader = new SkinReader();
            var files = new SkinPack();
            var pakFiles = GetAllPakFiles(rootPath);
            foreach (var file in pakFiles)
            {
                var relPath = Path.GetRelativePath(rootPath, file.FullName);
                if (allowMultiple || file.Name.Contains('^')) {
                    var idents = reader.ReadAllSkinSlots(file.FullName);
                    if (idents == null || !idents.Any()) {
                        files.ExtraFiles.Add(relPath);
                    } else if (idents.Count() > 1) {
                        files.MultiSkinFiles.Add(relPath, idents);
                    } else {
                        files.Skins.Add(relPath, idents.First());
                    }
                } else {
                    var ident = reader.ReadSkinSlot(file.FullName);
                    if (ident != null) {
                        files.Skins.Add(relPath, ident);
                    } else {
                        files.ExtraFiles.Add(relPath);
                    }
                }
            }
            var textFiles = Directory.EnumerateFiles(rootPath, "*.txt", SearchOption.TopDirectoryOnly);
            files.ReadmeFiles = textFiles.Count() > 0 ? textFiles.Select(f => Path.GetRelativePath(rootPath, f)).ToList() : new List<string>();
            return files;
        }
    }
}