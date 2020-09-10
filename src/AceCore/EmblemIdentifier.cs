using System.Collections.Generic;

namespace AceCore
{
    public class EmblemIdentifier : Identifier {
        private EmblemIdentifier(string rawValue, string slot, string format)
        {
            RawValue = rawValue;
            _slotName = ParseSlotName(slot) ?? slot;
            Format = format;
            // var path ="/Game/Vehicles/Weapons/w_dptk_a0/Textures/w_dptk_a0_D.uasset";
        }

        private Dictionary<string, string> _specialNames = new Dictionary<string, string> {
            ["022"] = "Airforce 08"
        };


        public static bool TryParse(string value, out EmblemIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"Emblem\/(tga|png)\/emblem_(\d{3})(?!\.u[^a])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 3) {
                ident = new EmblemIdentifier(match.Groups[0].Value, match.Groups[2].Value, match.Groups[1].Value);
                return true;
            }
            ident = null;
            return false;
        }
        private string _slotName;

		private string ParseSlotName(string forceName = null) {
			var knownName = _specialNames.TryGetValue(forceName ?? _slotName, out var name);
            if (knownName) {
                return name;
            } else {
                return $"Emblem {_slotName.TrimStart('0')}";
            }
		}

        public override string ToString() {
            return GetSlotName();
        }

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
			return _slotName;
        }

        public string Format { get; }

        public override string ObjectPath => base.ObjectPath + $"Emblem/{Format}";

        public override string BaseObjectName => $"emblem_{_slotName}";
    }
}