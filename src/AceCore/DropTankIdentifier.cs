namespace AceCore
{
    public class DropTankIdentifier : Identifier
    {
        private DropTankIdentifier(string rawValue, string aircraft, string type) {
            RawValue = rawValue.TrimEnd('.');
            Aircraft = aircraft.Trim('/');
            Type = type;
        }

        public string Aircraft { get; set; }
        public string Type { get; }

        public static bool TryParse(string value, out DropTankIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"(?<aircraft>[a-zA-Z0-9]{4,6}?)_dptk_(?<type>[A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a\W])");
            var match = rex.Match(value);
            if (match != null && match.MatchedGroups().Count >= 3) {
                ident = new DropTankIdentifier(match.Groups[0].Value, match.Groups["aircraft"].Value, match.Groups["type"].Value);
                return true;
            }
            ident = null;
            return false;
        }

        public override string ObjectPath => base.ObjectPath + $"Vehicles/Aircraft/{Aircraft}/{Aircraft}_dptk";
        public override string BaseObjectName => $"{Aircraft}_dptk";
        public override string ToString() {
            return $"Drop tanks ({Aircraft})";
        }
    }
}