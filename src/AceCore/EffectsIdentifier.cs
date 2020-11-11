namespace AceCore {
    public class EffectsIdentifier : Identifier {
        private readonly string _group;
        private readonly string _category;
        private readonly string _object;
        private readonly string _assetPath;
        private readonly string _asset;
        private readonly string _type;

        public override string BaseObjectName => _asset;
        public string EffectsObject => _object;

        private EffectsIdentifier(string rawValue, string group, string category, string obj, string asset) {
            RawValue = rawValue;
            _group = group.TrimEnd('/');
            _category = category.TrimEnd('/');
            _object = obj.TrimEnd('/');
            _assetPath = asset.Replace(System.IO.Path.GetFileName(asset), string.Empty).TrimEnd('/');
            _asset = System.IO.Path.GetFileNameWithoutExtension(asset.TrimEnd('/'));
        }

        public override string GetSlotName() => ToString();

        public override string ToString() {
            return $"VFX: {_object} ({_asset})";
        }

        public static bool TryParse(string value, out EffectsIdentifier ident) {
            ident = null;
            var rex = new System.Text.RegularExpressions.Regex(@"VFX\/(\w+?)\/(\w+\/?)(\w+\/)(.*?)\.ua\w+");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 4) {
                ident = new EffectsIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value);
                return true;
            }
            ident = null;
            return false;
        }

        public override string ObjectPath { get {
            var name = base.ObjectPath + $"VFX/{_group}/{_category}/{_object}";
            name += string.IsNullOrWhiteSpace(_assetPath) ? string.Empty : "/" + _assetPath;
            return name;
        } }
    }
}