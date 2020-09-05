using System.Collections.Generic;
using System.Linq;

namespace AceCore {
    public class WeaponIdentifier : Identifier {
        private WeaponIdentifier(string rawValue, string weapon, string slot, string type)
        {
            RawValue = (rawValue.StartsWith("w_") ? rawValue : "w_" + rawValue).TrimEnd('.');
            Weapon = weapon;
            _slotId = slot;
            _weaponName = ParseWeaponName(weapon) ?? weapon.ToUpper();
            Type = type;
            // var path ="/Game/Vehicles/Weapons/w_dptk_a0/Textures/w_dptk_a0_D.uasset";
        }

        public static bool TryParse(string value, out WeaponIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"(?:w_)([a-zA-Z0-9]+)_(\w{2})?(?:_?)([A-Z]{1}|[A-Za-z]{4})(?:[^\w])(?!u[^a])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new WeaponIdentifier(match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                return true;
            }
            ident = null;
            return false;
        }
        public string Weapon;
        private string _weaponName;
        private string _slotId;
        public string Type {get;} = "D";

        public string GetWeaponName() {
            _weaponName ??= ParseWeaponName();
            return _weaponName;
        }

		private string ParseWeaponName(string forceName = null) {
			var knownName = Constants.WeaponNames.TryGetValue(forceName ?? RawValue, out var name);
            if (knownName) {
                return name;
            } else {
                try {
                    return (forceName ?? RawValue).Split('_').First().ToUpper();
                } catch {
                    return RawValue;
                }
            }
		}

        public override string ToString() {
            return GetWeaponName();
        }

        public override string GetSlotName() => GetWeaponName();

        public override string ObjectPath => base.ObjectPath + $"Vehicles/Weapons/{RawValue}/Textures";
    }
}