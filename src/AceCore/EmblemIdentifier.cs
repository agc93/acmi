using System.Collections.Generic;
using System.Linq;

namespace AceCore
{
    public class EmblemIdentifier : Identifier {
        private EmblemIdentifier(string rawValue, string slot, string format, string size)
        {
            RawValue = rawValue;
            Slot = slot;
            _slotName = ParseSlotName(slot) ?? slot;
            Format = format;
            if (!string.IsNullOrWhiteSpace(size)) {
                Size = size.TrimStart('_');
            }
        }

        private Dictionary<string, string> _specialNames = new Dictionary<string, string> {
            ["022"] = "Airforce 08"
        };


        public static bool TryParse(string value, out EmblemIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"Emblem\/(tga|png)\/emblem_(\d{3})(?!\.u[^a])");
            var rex = new System.Text.RegularExpressions.Regex(@"emblem_(\d{3})(\w{2,3}_?)?(?=\.ua)");
            var match = rex.Match(value);
            if (match != null && match.MatchedGroups().Count >= 2) {
                ident = match.MatchedGroups().Count == 2
                    ? new EmblemIdentifier(match.Groups[0].Value, match.Groups[1].Value, "tga", null)
                    : new EmblemIdentifier(match.Groups[0].Value, match.Groups[1].Value, "png", match.Groups[2].Value);
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
                return $"Emblem {Slot.TrimStart('0')}";
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
        public string Slot { get; }
        public string Size { get; }

        public override string ObjectPath => base.ObjectPath + $"Emblem/{Format}";

        public override string BaseObjectName => $"emblem_{Slot}";
    }
}