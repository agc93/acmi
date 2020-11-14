using System.Collections.Generic;

namespace AceCore
{
    public class PortraitIdentifier : Identifier {
        private PortraitIdentifier(string rawValue, string slot)
        {
            RawValue = rawValue;
            Slot = slot;
            _fileName = System.IO.Path.GetFileNameWithoutExtension(RawValue);
            _slotName = GetSlotName();
        }

        private Dictionary<string, string> _specialNames = new Dictionary<string, string> {
            ["avril"] = "Avril/Scrap Queen",
            ["mcKinsey"] = "McKinsey",
			["MaCony"] = "McOnie",
            ["golem"] = "Knocker",
            ["mage"] = "Clown",
            ["wit"] = "Sol 2",
            ["seymour"] = "Sol 3",
            ["hermann"] = "Sol 4",
            ["roald"] = "Sol 5",
            ["georges"] = "Georg",
            ["Edouard"] = "General Labarthe",
            ["karl"] = "Captain Karl",
            ["David"] = "David North"
        };


        public static bool TryParse(string value, out PortraitIdentifier ident) {
            var rex = new System.Text.RegularExpressions.Regex(@"SubtitleSpeakerPortrait\/\b\d{2}_([a-zA-Z_]+)(\d{2}|_)?\.ua.*");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new PortraitIdentifier(match.Groups[0].Value, match.Groups[1].Value);
                return true;
            }
            ident = null;
            return false;
        }

        private string Slot { get; }

        private readonly string _fileName;
        private string _slotName;

		private string ParseSlotName() {
			var knownName = _specialNames.TryGetValue(Slot, out var name);
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

        public override string BaseObjectName => _fileName;
    }
}