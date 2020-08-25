using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;

namespace InstallerCreator {
    public interface IFileService {
        SkinPack GetFiles(string rootPath, bool allowMultiple = false);
    }
    public class FileService : IFileService {
        private readonly PakReader _reader;

        public FileService(PakReader reader)
        {
            _reader = reader;
        }
        public SkinPack GetFiles(string rootPath, bool allowMultiple = false) {
            var reader = _reader;
            var files = new SkinPack();
            var pakFiles = GetAllPakFiles(rootPath);
            foreach (var file in pakFiles)
            {
                var relPath = Path.GetRelativePath(rootPath, file.FullName);
                var useMultiple = allowMultiple || file.Name.Contains("MULTI");
                    var idents = reader.ReadFile(file.FullName, useMultiple);
                    files.Add(idents, relPath);
            }
            var textFiles = Directory.EnumerateFiles(rootPath, "*.txt", SearchOption.TopDirectoryOnly);
            files.ReadmeFiles = textFiles.Count() > 0 ? textFiles.Select(f => Path.GetRelativePath(rootPath, f)).ToList() : new List<string>();
            return files;
        }

        public IEnumerable<FileInfo> GetAllPakFiles(string filePath) {
            var rootDir = new DirectoryInfo(filePath);
            var pakFiles = rootDir.EnumerateFiles("*.pak", SearchOption.AllDirectories);
            return pakFiles;
        }
    }
}