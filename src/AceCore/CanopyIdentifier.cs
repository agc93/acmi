namespace AceCore
{
    public class CanopyIdentifier : Identifier
    {
        private readonly string _aircraft;
        private readonly string _canopyId;
        private string _slotName;

        private CanopyIdentifier(string rawValue, string aircraft, string canopyId) {
            RawValue = rawValue;
            _aircraft = aircraft.Trim('/');
            _canopyId = canopyId.Trim('/');
        }

        public override string ToString() => GetSlotName();

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
            return _slotName;
        }

        private string ParseSlotName(string forceName = null) {
            var aircraftName = GetAircraftName(_aircraft);
            var canopySlot = _canopyId switch {
                "1" => "Main",
                "2" => "Side/Back",
                _ => _canopyId
            };
            return $"{aircraftName} ({canopySlot})";
		}

        public static bool TryParse(string value, out CanopyIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"\/(\w{4,6})_Canopy(\d{1})_Inst(?!\.u[^a])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 3) {
                ident = new CanopyIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value);
                return true;
            }
            ident = null;
            return false;
        }
    }
}