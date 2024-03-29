using System;
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
            // var rex = new Regex(@"([a-z0-9]+?)_x?(\d+\w?)_([A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a])");
            var rex = new Regex(@"(?<![a-zA-Z0-9]{1}_)\b([a-zA-Z0-9]{4,6}?)_x?((?:\d+|[A-Z])\w*)[_x]([A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a\W])");
            var match = rex.Match(value);
            if (match != null && match.MatchedGroups().Count >= 2) {
                ident = new SkinIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                return true;
            }
            ident = null;
            return false;
        }

        private SkinIdentifier(string rawValue, string aircraft, string slot, string type, string part = null) {
            RawValue = rawValue.Trim('.').Trim('\0').Replace(@"\0", string.Empty).Trim('\\');
            Aircraft = aircraft;
            if (slot.Contains('_')) {
                var slotParts = slot.Split('_');
                Part = slotParts.Last();
                slot = string.Join(string.Empty, slotParts[..^1]);
            }
            var slotNum = new string(slot.TakeWhile(char.IsDigit).ToArray());
            slotNum = int.TryParse(slotNum, out var i)
                    ? i.ToString("D2")
                    : slotNum;
            Slot = slotNum + new string(slot.SkipWhile(char.IsDigit).ToArray());
            // Slot = slot.All(char.IsDigit) 
            //     ? 
            //     : slot;
            // Type = type.CapitalizeFirst();
            Type = type;
			SlotName = ParseSlotName();
        }
		private string SlotName {get; set;}
        public string Aircraft { get; }
        public string Slot { get; }
        public string Type { get; }
        public string Part { get; }

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

        public string GetSlotNumber(string prefix = null) {
            var s = Slot;
            var npcRex = new Regex(@"(\d{2})(\w{1})");
            if (npcRex.IsMatch(s)) {
                var match = npcRex.Match(s);
                var baseSlot = match.Groups[1].Value;
                return $"NPC {baseSlot.GetSlotNumber()}{match.Groups[2].Value}";
            }
            return (s.Length > 0 && char.IsDigit(s[0])) ? $"{prefix.OrDefault(string.Empty)}{s.GetSlotNumber()}" : s;
        }

        public override string ToString() {
            return $"{GetAircraftName(Aircraft)} ({GetSlotName()})";
        }

        public string GetAircraftName() => base.GetAircraftName(Aircraft);

        public override string ObjectPath => base.ObjectPath + $"Vehicles/Aircraft/{Aircraft}/{Slot}";
        public bool IsNPC => Slot.Any(char.IsLetter);
        public override string BaseObjectName => $"{Aircraft}_{Slot}";
        // public override string BaseObjectName => $"{Aircraft}_{Slot}{Part.WithPrefix("_")}";
    }
}