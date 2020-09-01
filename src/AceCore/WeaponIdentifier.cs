using System.Collections.Generic;
using System.Linq;

namespace AceCore {
    public class WeaponIdentifier : Identifier {
        private WeaponIdentifier(string rawValue, string slot)
        {
            RawValue = rawValue.StartsWith("w_") ? rawValue : "w_" + rawValue;
            _slotName = ParseSlotName(slot) ?? slot;
            // var path ="/Game/Vehicles/Weapons/w_dptk_a0/Textures/w_dptk_a0_D.uasset";
        }

        public static bool TryParse(string value, out WeaponIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"\/?w_(\w+_\w+?)_\w(?![\/.])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new WeaponIdentifier(match.Groups[0].Value, match.Groups[1].Value);
                return true;
            }
            ident = null;
            return false;
        }
        private string _slotName;

		private string ParseSlotName(string forceName = null) {
			var knownName = Constants.WeaponNames.TryGetValue(forceName ?? RawValue, out var name);
            if (knownName) {
                return name;
            } else {
                try {
                    return (forceName ?? RawValue).Split('_').First().ToUpper();
                } catch {
                    return RawValue;
                }
            }
		}

        public override string ToString() {
            return GetSlotName();
        }

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
			return _slotName;
        }
    }
}