using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AceCore
{
    public class VesselIdentifier : Identifier {
        private Dictionary<string, string> _slotNames = Constants.SlotNames;

        private Dictionary<string, string> _aircraftNames = Constants.AircraftNames;

        public static bool TryParse(string value, out VesselIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"([a-z0-9]+?)_(v?\d+a?\w{1}?)_(\w)(?!\.u[^a]).*");
            // var rex = new Regex(@"(?:\w+_)?([a-zA-Z]{4})_(\w{1})?(?:_?)([A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a])");
            var rex = new Regex(@"(?<mission>ex\d{2,}_)?(?<vessel>[a-zA-Z]{4})_(?<slot>\w{1})?(?:_?)(?<type>[A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a])");
            var match = rex.Match(value);
            // System.Console.WriteLine($"Matching {value}: {match.Success}");
            if (match != null && match.MatchedGroups().Count >= 4) {
                // System.Console.WriteLine($"matched vessel {value}");
                ident = new VesselIdentifier(match.Groups[0].Value, match.Groups["vessel"].Value, match.Groups["slot"].Value, match.Groups["type"].Value, match.Groups["mission"].Value);
                return true;
            }
            ident = null;
            return false;
        }

        private VesselIdentifier(string rawValue, string vessel, string slot, string type, string missionSlot) {
            RawValue = rawValue.Trim('.');
            Vessel = vessel;
            Slot = slot;
            Type = type;
            MissionSlot = missionSlot;
			// SlotName = ParseSlotName();
        }
        public string MissionSlot {get;}
        public string Vessel { get; }
        public string Slot { get; }
        public string Type {get;} = "D";

		/* private string ParseSlotName() {
			var knownName = _slotNames.TryGetValue(Slot, out var name);
            if (knownName) {
                return name;
            } else {
				var npcRex = new Regex(@"(\d{2})(\w{1})");
				if (npcRex.IsMatch(Slot)) {
					var match = npcRex.Match(Slot);
					var baseSlot = match.Groups[1].Value;
					var baseMatch = _slotNames.TryGetValue(baseSlot, out name);
					return $"NPC {(baseMatch ? name : baseSlot)} {match.Groups[2].Value}";
				}
                return $"0{(int.TryParse(Slot, out var num) ? (num + 1).ToString() : Slot)}";
            }
		}

        public override string GetSlotName() {
			SlotName ??= ParseSlotName();
			return SlotName;
        } */

        public override string ToString() {
            return $"{GetItemName(Vessel)} ({GetSlotName()})";
        }

        public string GetVesselName(bool shortName = false) {
            var itemName = base.GetItemName(Vessel);
            return shortName ? itemName.Split(':').First() : itemName;
        }

        public string GetObjectName() => $"{(string.IsNullOrWhiteSpace(MissionSlot) ? string.Empty : MissionSlot + "_")}{Vessel}{(string.IsNullOrWhiteSpace(Slot) ? string.Empty : "_" + Slot)}_{Type}";

        public override string ObjectPath => base.ObjectPath + $"Vehicles/Vessels/{Vessel}/Textures";

        public override string BaseObjectName => Vessel;
    }
}