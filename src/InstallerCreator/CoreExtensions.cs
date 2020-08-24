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

        internal static string JoinLines(this IEnumerable<string> lines, string separator = null) {
            return string.Join(separator ?? System.Environment.NewLine, lines);
        }

        internal static Dictionary<TKey, List<TValue>> EnumerateDictionary<TKey, TValue>(this Dictionary<TKey, IEnumerable<TValue>> input) {
            return input.ToDictionary(k => k.Key, v => v.Value.ToList());
        }

        
    }
}