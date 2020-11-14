using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AceCore;
using static AceCore.Constants;

namespace PackCreator {
    public class FileNameService {
        private static Dictionary<string, string> ItemNames {get;} = new Dictionary<string, string>().Concat(AllItemNames).Concat(ModTypes).Concat(Aces).ToDictionary(k => k.Key, v => v.Value);
        private static Dictionary<string, string> AllNames {get;} = new Dictionary<string, string>().Concat(AllItemNames).Concat(SlotNames).Concat(ModTypes).Concat(Aces).ToDictionary(k => k.Key, v => v.Value);
        public string GetNameFromGroup(SourceGroup sourceGroup, BuildSettings bSettings, string separator = "_") {
            var segments = (sourceGroup.Name ?? sourceGroup.RawValue).Split('_');
            var parts = segments.Select(s => Constants.AllItemNames.TryGetValue(s, out var name) ? name : s).ToList();
            parts.AddIfSet(bSettings.Prefix);
            return string.Join(separator, parts).MakeSafe(true);
        }

        public string GetNameFromGroup<T>(SourceGroup sourceGroup, T ident, BuildSettings settings, string separator = "_") where T : Identifier {

            if (ident is SkinIdentifier sIdent) {
                var parts = new List<string> {sIdent.GetAircraftName(), $"Skin{sIdent.Slot.GetSlotNumber()}"};
                parts.AddIfSet(settings.Prefix);
                return string.Join(separator, parts).MakeSafe(true);
            } else {
                return GetNameFromGroup(sourceGroup, settings, separator);
            }
        }

        public string GetNameFromBuildGroup(KeyValuePair<SourceGroup, List<BuildInstruction>> pakBuild, BuildSettings settings, string separator = "_") {
            SourceGroup nameKey;
            if (string.IsNullOrWhiteSpace(pakBuild.Key.Name)) {
                //there's no pre-set name. time to guess one.
                var instructionName = pakBuild.Value.Aggregate(new List<string>(), (names, instruction) => names.AddIfNotExists(instruction.GetOutputName(separator)));
                nameKey = instructionName.Count == 1
                    ? //all the instructions agreed on a name, so we'll use that
                        (SourceGroup)instructionName.First()
                    : //either no names returned or they didn't match
                        pakBuild.Key;
            } else {
                //if there's a pre-set name, use that always
                nameKey = pakBuild.Key;
            }
            return GetNameFromGroup(nameKey, settings);
        }

        public string GetOutputPathForGroup(string sourceGroup, string prefix = null) {
            // var raw = (sourceGroup.Name ?? sourceGroup.RawValue).Split('_').SkipWhile(c => c.All(char.IsDigit)).First();
            var raw = sourceGroup.Split('_').SkipWhile(c => c.All(char.IsDigit)).First();
            return (Constants.AllNames.TryGetValue(raw, out var name) ? name : raw).MakeSafe(true);
        }
    }
}