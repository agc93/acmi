using System.Linq;
using AceCore;

namespace PackCreator {
    public class FileNameService {
        public string GetNameFromGroup(string sourceGroup, string prefix = null, string separator = "_") {
            var segments = sourceGroup.Split('_');
            var parts = segments.Select(s => Constants.AllNames.TryGetValue(s, out var name) ? name : s).ToList();
            if (!string.IsNullOrWhiteSpace(prefix)) {
                parts.Add(prefix);
            }
            return string.Join(separator, parts);
        }

        public string GetOutputPathForGroup(string sourceGroup, string prefix = null) {
            var raw = sourceGroup.Split('_').First();
            return Constants.AllNames.TryGetValue(raw, out var name) ? name : raw;
        }
    }
}