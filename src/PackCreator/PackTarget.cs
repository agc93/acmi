using System.Collections.Generic;
using System.Linq;
using static AceCore.Constants;

namespace PackCreator {
    public class PackTarget<T> : PackTarget where T : AceCore.Identifier {
        public PackTarget(string targetFileName, string objectName, string slot) : base(targetFileName, objectName) {
        }

        public PackTarget(string targetFileName, T identifier) : base(targetFileName, identifier.BaseObjectName) {}

        public T Identifier {get;set;}
        public override string GetOutputPath() {
            return base.GetOutputPath();
        }
    }
    public class PackTarget {
        public PackTarget(string targetFileName, string objectName)
        {
            TargetFileName = targetFileName;
            // TargetPath = objectPath;
            ObjectName = objectName;
            // Slot = slot;
        }

        public string TargetFileName { get; }
        // public string TargetPath { get; }
        public string ObjectName {get;set;} = string.Empty;
        // public string Slot {get;set;} = string.Empty;

        public List<AssetContext> ExtraAssets {get;} = new List<AssetContext>();

        public virtual string GetOutputPath() {
            /* if (string.IsNullOrWhiteSpace(TargetPath)) {
                return "Mod Files";
            } else {
                var objName = System.IO.Path.GetFileName(TargetPath);
                var previous = new System.IO.DirectoryInfo(TargetPath).Parent.Name;
                var segments = new List<string>();
                segments.Add(AllItemNames.TryGetValue(previous, out var cat)
                    ? cat
                    : previous == "Vehicles" || previous == "Content" || previous == "Nimbus"
                        ? string.Empty
                        : previous);
                segments.Add(AllItemNames.TryGetValue(objName, out var fn)
                    ? fn
                    : string.Empty);
                if (segments.All(s => string.IsNullOrWhiteSpace(s))) {
                    return TargetPath.Replace("Nimbus/", string.Empty);
                } else {
                    return System.IO.Path.Join(segments.Select(f => f.MakeSafe(true)).ToArray()).Replace("\\", "/");
                }
            } */
            return string.Empty;
        }

        public bool IsMerged {get;set;}
    }
}

