using System.Collections.Generic;
using System.Linq;
using static AceCore.Constants;

namespace PackCreator {
    public class PackTarget {
        public PackTarget(string targetFileName, string objectPath)
        {
            TargetFileName = targetFileName;
            ObjectPath = objectPath;
        }

        public PackTarget(string targetFileName, AceCore.Vehicles.VehicleSlot slot, string slotName = null) : this(targetFileName, slot.PathRoot + slot.ObjectName)
        {
            if (!string.IsNullOrWhiteSpace(slotName)) {
                SlotName = slotName;
            }
        }

        public string TargetFileName { get; }
        public string ObjectPath { get; }
        public string SlotName {get;set;} = string.Empty;

        public List<AssetContext> TargetAssets {get;} = new List<AssetContext>();

        public string GetOutputPath() {
            if (string.IsNullOrWhiteSpace(ObjectPath)) {
                return "Mod Files";
            } else {
                var objName = System.IO.Path.GetFileName(ObjectPath);
                var previous = new System.IO.DirectoryInfo(ObjectPath).Parent.Name;
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
                    return ObjectPath.Replace("Nimbus/", string.Empty);
                } else {
                    return System.IO.Path.Join(segments.Select(f => f.MakeSafe(true)).ToArray()).Replace("\\", "/");
                }
            }
        }

        public bool IsMerged {get;set;}
    }
}