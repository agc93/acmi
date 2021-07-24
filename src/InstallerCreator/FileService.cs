using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;
using Microsoft.Extensions.Logging;

namespace InstallerCreator {
    public interface IFileService {
        SkinPack GetFiles(string rootPath, bool allowMultiple = false);
    }
    public class FileService : IFileService {
        private readonly PakFileReader _reader;
        private readonly ILogger<FileService> _logger;

        public FileService(PakFileReader reader, ILogger<FileService> logger)
        {
            _reader = reader;
            _logger = logger;
        }
        public SkinPack GetFiles(string rootPath, bool allowMultiple = false) {
            var reader = _reader;
            var files = new SkinPack();
            var pakFiles = GetAllPakFiles(rootPath);
            foreach (var file in pakFiles)
            {
                var relPath = Path.GetRelativePath(rootPath, file.FullName);
                // _logger.LogTrace($"FileService reading {relPath}");
                var useMultiple = allowMultiple || file.Name.Contains("MULTI");
                var readOpts = new PakReaderOptions {
                    ReadWholeFile = useMultiple,
                    TreatAsLightweight = System.Text.RegularExpressions.Regex.IsMatch(file.Name, @"[_[(](?:LW|LIGHT)[_\]\)]")
                };
                var idents = reader.ReadFile(file.FullName);
                files.Add(idents.ToList(), relPath);
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