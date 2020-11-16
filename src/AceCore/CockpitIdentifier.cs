using System.Linq;

namespace AceCore
{
    public class CockpitIdentifier : Identifier {
        private readonly string _aircraft;
        private readonly string _area;
        private readonly string _partSlot;
        private readonly string _type;
        private string _slotName;

        public string CockpitArea => _area;
        public string SpecificPart => _partSlot;
        public string Type => _type;

        private CockpitIdentifier(string rawValue, string aircraft, string parts, string type) {
            RawValue = rawValue.TrimEnd('.');
            if (!string.IsNullOrWhiteSpace(parts)) {
                var allParts = parts.Split('_', System.StringSplitOptions.RemoveEmptyEntries).ToList();
                _area = allParts.First();
                _partSlot = allParts.Count > 1 ? allParts[1] : string.Empty;
            }
            _aircraft = aircraft.Trim('/').Trim('_');
            _type = type;
        }

        public override string ToString() => GetSlotName();

        public override string GetSlotName() {
            _slotName ??= ParseCockpitName();
            return _slotName;
        }

        private string ParseCockpitName(string forceName = null) {
            var aircraftName = GetAircraftName(_aircraft);
            return $"{aircraftName} ({_area.OrDefault("Main")}{(string.IsNullOrWhiteSpace(_partSlot) ? string.Empty : "/" + _partSlot)})";
		}

        public static bool TryParse(string value, out CockpitIdentifier ident) {
            var rex = new System.Text.RegularExpressions.Regex(@"(\w{4,6})_CP_(\w+)?([A-Z]{1}|[A-Za-z]{4})\.(?!u[^a])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 3) {
                ident = match.Groups.Count == 3
                    ? new CockpitIdentifier(match.Groups[0].Value, match.Groups[1].Value, null, match.Groups[2].Value)
                    : new CockpitIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                return true;
            }
            ident = null;
            return false;
        }

        public override string ObjectPath => base.ObjectPath + $"Vehicles/Aircraft/{_aircraft}/CP";

        public override string BaseObjectName => _aircraft;
    }
}