namespace AceCore.Parsers
{
    public interface IIdentifierParser  {
        (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false);
        int Priority => 100;
    }
    public class CrosshairParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = CrosshairIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class SkinParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = SkinIdentifier.TryParse(value, out var ident);
            return (parsed && (strict ? ident.Type == "D" : true), ident);
        }
    }

    public class PortraitParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = PortraitIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class WeaponParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = WeaponIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class EffectsParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = EffectsIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }
    }

    public class CanopyParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = CanopyIdentifier.TryParse(value, out var ident);
            return (parsed, ident);
        }

        public int Priority => 50;
    }

    public class EmblemParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = EmblemIdentifier.TryParse(value, out var ident);
            return (parsed && (strict ? ident.Format == "png" : true), ident);
        }
    }

    public class VesselParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = VesselIdentifier.TryParse(value, out var ident);
            return (parsed && (strict ? ident.Type == "D" : true), ident);
        }
    }

    public class CockpitParser : IIdentifierParser {
        public (bool IsValid, Identifier identifier) TryParse(string value, bool strict = false) {
            var parsed = CockpitIdentifier.TryParse(value, out var ident);
            return (parsed && (strict ? ident.Type == "D" : true), ident);
        }

        public int Priority => 50;
    }
}