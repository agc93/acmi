using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AceCore
{
    public class SkinIdentifier : Identifier {
        private Dictionary<string, string> _slotNames = Constants.SlotNames;

        private Dictionary<string, string> _aircraftNames = Constants.AircraftNames;

        public static bool TryParse(string value, out SkinIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"([a-z0-9]+?)_(v?\d+a?\w{1}?)_(\w)(?!\.u[^a]).*");
            var rex = new Regex(@"([a-z0-9]+?)_x?(\d+\w?)_([A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new SkinIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                return true;
            }
            ident = null;
            return false;
        }
        public static bool TryParsePath(string pathValue, out SkinIdentifier ident) {
            var rex = new Regex(@"Vehicles\/Aircraft\/([\w\d]+)\/(?:(x?(?:\d+\w?))|\w+)\/");
            var match = rex.Match(pathValue);
            if (match != null && match.Groups.Count >= 1) {
                ident = new SkinIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups.Count > 2 ? match.Groups[2].Value : null, null);
                return true;
            }
            ident = null;
            return false;
        }
        private SkinIdentifier(string rawValue, string aircraft, string slot, string type) {
            RawValue = rawValue;
            Aircraft = aircraft;
            Slot = slot;
            Type = type;
			SlotName = ParseSlotName();
        }
		public string SlotName {get; private set;}
        public string Aircraft { get; }
        public string Slot { get; }
        public string Type { get; }

		private string ParseSlotName() {
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
        }

        public override string ToString() {
            return $"{GetAircraftName(Aircraft)} ({GetSlotName()})";
        }

        public string GetAircraftName() => base.GetAircraftName(Aircraft);

        public string GetObjectName() => $"{Aircraft}_{Slot}_{Type}";

        public override string ObjectPath => base.ObjectPath + $"Vehicles/{Aircraft}/{Slot}";
        public bool IsNPC => Slot.Any(char.IsLetter);
    }
}