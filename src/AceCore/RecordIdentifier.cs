using System.Collections.Generic;

namespace AceCore
{
    public class RecordIdentifier : Identifier
    {
        private RecordIdentifier(string rawValue, string slot)
        {
            RawValue = rawValue;
            Slot = slot;
            _slotName = GetSlotName();
        }

        public static bool TryParse(string value, out RecordIdentifier ident) {
            var rex = new System.Text.RegularExpressions.Regex(@"\/SubtitleNonsenseSpeakerPortrait\/\b\d{2}_([a-zA-Z_]+)(\d{2}|_)?\.ua");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new RecordIdentifier(match.Groups[0].Value, match.Groups[1].Value);
                return true;
            }
            ident = null;
            return false;
        }

        public string Slot { get; }
        private string _slotName;

		private string ParseSlotName() {
			var knownName = Constants.AllItemNames.TryGetValue(Slot, out var name);
            if (knownName) {
                return name;
            } else {
                var ti = new System.Globalization.CultureInfo("en-US",false).TextInfo;
                return ti.ToTitleCase(Slot.Replace('_', ' '));
            }
		}

        public override string ToString() {
            return GetSlotName();
        }

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
			return _slotName;
        }

        public override string ObjectPath => base.ObjectPath + "UI/HUD/SubtitleSpeakerPortrait";
    }
}