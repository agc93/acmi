using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AceCore
{
    public class SkinIdentifier {
        private Dictionary<string, string> _slotNames = new Dictionary<string, string> {
            ["00"] = "Osea",
            ["01"] = "Erusea",
            ["02"] = "Special",
            ["03"] = "Mage",
            ["04"] = "Spare",
            ["05"] = "Strider",
			["06"] = "Slot 7",
			["07"] = "Slot 8"
        };

        private Dictionary<string, string> _aircraftNames = new Dictionary<string, string> {
            ["a10a"] = "A-10C",
            ["adf11f"] = "ADF-11F",
            ["f02a"] = "F-2A",
            ["f04e"] = "F-4E",
            ["f14d"] = "F-14D",
            ["f15c"] = "F-15C",
            ["f15e"] = "F-15E",
            ["f15j"] = "F-15J",
            ["f16c"] = "F-16C",
            ["f18f"] = "F/A-18F",
            ["f22a"] = "F-22A",
            ["f35c"] = "F-35C",
            ["f104"] = "F-104C",
            ["f104av"] = "Avril F-104",
            ["j39e"] = "Gripen",
            ["m21b"] = "MiG-21",
            ["m29a"] = "MiG-29",
            ["m31b"] = "MiG-31",
            ["mr2k"] = "Mirage",
            ["mrgn"] = "ADFX-01",
            ["pkfa"] = "Su-57",
            ["rflm"] = "Rafale M",
            ["su30"] = "Su-30M2",
            ["su30sm"] = "Su-30SM",
            ["su33"] = "Su-33",
            ["su34"] = "Su-34",
            ["su35"] = "Su-35S",
            ["su37"] = "Su-37",
            ["su47"] = "Su-47",
            ["typn"] = "Typhoon",
            ["x02s"] = "X-02S",
            ["yf23"] = "YF-23",
            ["zoef"] = "FALKEN"
        };

        public static bool TryParse(string value, out SkinIdentifier ident) {
            var rex = new System.Text.RegularExpressions.Regex(@"([a-z0-9]+?)_(v?\d+a?\w{1}?)_(\w).*");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new SkinIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
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

        public string RawValue { get; }

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

        public string GetSlotName() {
			SlotName ??= ParseSlotName();
			return SlotName;
        }

        public string GetAircraftName() {
            var knownName = _aircraftNames.TryGetValue(Aircraft, out var name);
            if (knownName) {
                return name;
            } else {
                return Aircraft.ToUpper();
            }
        }

        public override string ToString() {
            return $"{GetAircraftName()} ({GetSlotName()})";
        }
    }
}