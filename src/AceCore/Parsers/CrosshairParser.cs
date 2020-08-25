namespace AceCore.Parsers
{
    public interface IIdentifierParser  {
        (bool IsValid, Identifier identifier) TryParse(string value);
    }
    public class CrosshairParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = CrosshairIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class SkinParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = SkinIdentifier.TryParse(value, out var ident);
            return (parsed && ident.Type == "D", ident);
        }
    }

    public class PortraitParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = PortraitIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class WeaponParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = WeaponIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class EffectsParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = EffectsIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class CanopyParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value) {
            var parsed = CanopyIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }
}