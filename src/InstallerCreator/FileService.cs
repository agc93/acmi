using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;

namespace InstallerCreator
{
    public interface IFileService {
        SkinPack GetFiles(string rootPath, bool allowMultiple = false);
    }
    public class AdvancedFileService : IFileService {
        private readonly PakReader _reader;

        public AdvancedFileService(PakReader reader)
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
                // var useMultiple = allowMultiple || file.Name.Contains("MULTI") || file.Length < 6291000;
                var useMultiple = allowMultiple || file.Name.Contains("MULTI");
                // if (useMultiple) {
                    var idents = reader.ReadFile(file.FullName, useMultiple);
                    /* var first = idents.FirstOrDefault();
                    if (first == null) {
                        files.ExtraFiles.Add(relPath);
                        continue;
                    }
                    if (first is SkinIdentifier skinId) {
                        var skinIds = idents.Cast<SkinIdentifier>().ToList();
                        if (skinIds.Count > 1) {
                            files.MultiSkinFiles.Add(relPath, skinIds);
                        } else {
                            files.Skins.Add(relPath, first as SkinIdentifier);
                        }
                    } else if (first is PortraitIdentifier) {
                        files.Portraits.Add(relPath, idents.Cast<PortraitIdentifier>());
                    } else if (first is CrosshairIdentifier) {
                        files.Crosshairs.Add(relPath, idents.Cast<CrosshairIdentifier>());
                    }  */
                    files.Add(idents, relPath);
                // }
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
    public class FileService : IFileService
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
                var useMultiple = allowMultiple || file.Name.Contains("MULTI") || file.Length < 8192;
                if (useMultiple) {
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