using System.Collections.Generic;

namespace AceCore
{
    
    public class CrosshairIdentifier : Identifier {
        private CrosshairIdentifier(string rawValue, string slot)
        {
            RawValue = rawValue;
            Slot = slot;
            _slotName = GetSlotName();
        }

        private Dictionary<string, string> _weaponNames = new Dictionary<string, string> {
            ["seeker01"] = "MSL",
            ["seeker01_1"] = "ESM (MSL)",
            ["seeker02"] = "Sp. W",
            ["seeker02_1"] = "ESM (Sp. W)",
            ["rclReticle"] = "RKTL",
            ["gunReticle_mgp"] = "TLS/PLSL/EML",
			["Multi_Lock-"] = "8AAM/8AGM"
        };


        public static bool TryParse(string value, out CrosshairIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Crosshairs\/hud_(\w+[\d{2}-]?)");
            var rex = new System.Text.RegularExpressions.Regex(@"\/HUD\/(MultiLockon\/)?hud_(\w+[\d{2}-])(?=[^.\d]|.ua)");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new CrosshairIdentifier(match.Groups[0].Value, match.Groups[2].Value);
                return true;
            }
            ident = null;
            return false;
        }

        public string Slot { get; }
        private string _slotName;

		private string ParseSlotName() {
			var knownName = _weaponNames.TryGetValue(Slot, out var name);
            if (knownName) {
                return name;
            } else {
				return _slotName;
            }
		}

        public override string ToString() {
            var name = GetSlotName();
            return $"HUD: {(string.IsNullOrWhiteSpace(name) ? Slot : name)}";
        }

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
			return _slotName;
        }

        public override string ObjectPath => base.ObjectPath + $"UI/HUD/{(Slot.Contains("Multi") ? "MultiLockon" : string.Empty)}";
    }
}