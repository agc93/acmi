using System.Linq;
using AceCore;

namespace PackCreator {
    public class FileNameService {
        public string GetNameFromGroup(SourceGroup sourceGroup, string prefix = null, string separator = "_") {
            var segments = (sourceGroup.Name ?? sourceGroup.RawValue).Split('_');
            var parts = segments.Select(s => Constants.AllNames.TryGetValue(s, out var name) ? name : s).ToList();
            if (!string.IsNullOrWhiteSpace(prefix)) {
                parts.Add(prefix);
            }
            return string.Join(separator, parts);
        }

        public string GetOutputPathForGroup(string sourceGroup, string prefix = null) {
            // var raw = (sourceGroup.Name ?? sourceGroup.RawValue).Split('_').SkipWhile(c => c.All(char.IsDigit)).First();
            var raw = sourceGroup.Split('_').SkipWhile(c => c.All(char.IsDigit)).First();
            return Constants.AllNames.TryGetValue(raw, out var name) ? name : raw;
        }
    }
}