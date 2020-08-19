using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InstallerCreator {
    public static class CoreExtensions {
        public static IEnumerable<string> GetFiles(this string path,
                       string[] searchPatterns,
                       SearchOption searchOption = SearchOption.TopDirectoryOnly) {
            return searchPatterns.AsParallel()
                   .SelectMany(searchPattern =>
                          Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }
}