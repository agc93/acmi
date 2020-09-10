using System.IO;
using System.Linq;

namespace InstallerCreator.ModInstaller
{
    public class ImageLocatorService
    {
        public string GetImagePath(string fileName, string rootPath) {
            var skinFileName = Path.GetFileNameWithoutExtension(fileName).TrimEnd('_', '.');
            var pakFileLocation = new FileInfo(Path.Combine(rootPath, fileName)).Directory;
            var exts = new[] { ".png", ".jpg"};
            var possibleImages = exts.Select(e => Path.Join(pakFileLocation.FullName, skinFileName + e));
            possibleImages = possibleImages
                .Concat(exts.Select(e => Path.Join(pakFileLocation.FullName, "Screenshots", skinFileName + e)))
                .Concat(exts.Select(e => Path.Join(rootPath, "Screenshots", skinFileName + e)));
            var firstValid = possibleImages.FirstOrDefault(pi => File.Exists(pi));
            if (firstValid == null) {
                var relativeLocation = Path.GetRelativePath(rootPath, pakFileLocation.FullName);
                if (relativeLocation != ".") {
                    // skins are in their own folder
                    var files = pakFileLocation.FullName.GetFiles(new[] { "*.png", "*.jpg"}, SearchOption.TopDirectoryOnly).ToList();
                    if (files.Any()) {
                        if (files.Count == 1) {
                            return Path.GetRelativePath(rootPath, files[0]);
                        } else {
                            var preview = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLower().Contains("preview"));
                            if (preview != null) {
                                return Path.GetRelativePath(rootPath, preview);
                            }
                        }
                    }
                }
            }
            return firstValid;
        }
    }
}