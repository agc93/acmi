using System.Collections.Generic;
using System.Linq;

namespace AceCore {
    public class WeaponIdentifier : Identifier {
        private WeaponIdentifier(string rawValue, string slot)
        {
            RawValue = rawValue;
            _slotName = ParseSlotName(slot) ?? slot;
            // var path ="/Game/Vehicles/Weapons/w_dptk_a0/Textures/w_dptk_a0_D.uasset";
        }

        private Dictionary<string, string> _specialNames = new Dictionary<string, string> {
            ["dptk_a0"] = "Droptanks",
            ["lacm_f0"] = "LACM (Rafale)",
            ["lacm_e0"] = "LACM (Gripen)",
            ["4aam_a0"] = "4AAM/8AAM/6AAM/HCAA",
            ["4aam_f0"] = "Mirage/Rafale HCAA/4AAM",
            ["4aam_j0"] = "HCAA (F-15J)",
            ["4aam_r0"] = "Russian 4AAM/6AAM",
            ["4aam_r1"] = "4AAM (Su-57)",
            ["4aam_x0"] = "4AAM (X-02S)",
            ["4agm_a0"] = "4AGM (A-10C)",
            ["4agm_r0"] = "4AGM (Sukhoi)",
            ["8agm_a0"] = "8AGM (Typhoon/F-35)",
            ["aam_a0"] = "Hellfire",
            ["asrc_a0"] = "VL-ASROC",
            ["bom_x0"] = "UGB (Tu-95/Tu-160)",
            ["ecpm_x0"] = "IEWS",
            ["gpb_a0"] = "GPB (F-14/F-16)",
            ["gpb_f0"] = "GBP (Mirage)",
            ["gpb_r0"] = "GBP (Su-57)",
            ["grkt_a0"] = "GRKT (F-104C)",
            ["icbm"] = "IRBM",
            ["laam_a0"] = "LAAM (Phoenix)",
            ["laam_e0"] = "LAAM (Rafale/Typhoon)",
            ["laam_r0"] = "LAAM (MiG-31)",
            ["laam_r1"] = "LAAM (Su-35)",
            ["lagm_a0"] = "LAGM (F-4E)",
            ["lasm_a0"] = "LASM (F/A 18F)",
            ["lasm_f0"] = "LASM (Mirage)",
            ["lasm_j1"] = "LASM (F-2A)",
            ["lasm_r0"] = "LASM/LAGM (Russian)",
            ["lasm_x0"] = "LASM (X-02S)",
            ["mop"] = "Targeting Pod Bomb",
            ["msl_f1"] = "MSL (Mirage/Rafale)",
            ["msl_j0"] = "MSL (F-2A/F-15J)",
            ["msl_r0"] = "MSL (Russian)",
            ["qaam_j0"] = "QAAM (F-15J)",
            ["qaam_r0"] = "QAAM (Russian)",
            ["qaam_x0"] = "QAAM (Mimic)",
            ["rkt_a0"] = "RKT (A-10C/F-2A)",
            ["rkt_r0"] = "RKT (MiG-21)",
            ["rktl_a0"] = "Rocket Pod (A-10C)",
            ["rktl_j0"] = "Rocket Pod (F-2A)",
            ["rktl_r0"] = "Rocket Pod (MiG-21)",
            ["saam_a0"] = "SAAM/HVAA",
            ["saam_r0"] = "SAAM/HVAA (Russian)",
            ["saam_r1"] = "SAAM (MiG-31)",
            ["sffs_a0"] = "SFFS (F-15E)",
            ["sffs_r0"] = "SFFS (Su-34)",
            ["smin_r0"] = "Alicorn Jammer",
            ["sobuy_j0"] = "P-1 Buoy",
            ["tls_x0"] = "TLS (F-15E/Su-37)",
            ["tls_x1"] = "TLS (ADFX-01)",
            ["tls_x2"] = "TLS (ADF-01)",
            ["tls_x3"] = "TLS (ADF-11F)",
            ["trpd_j0"] = "VL-ASROC Torpedo",
            ["uav_x0"] = "UAV (ADF-11F)",
            ["ugb_r0"] = "UGB (Su-47)"
        };


        public static bool TryParse(string value, out WeaponIdentifier ident) {
            // var rex = new System.Text.RegularExpressions.Regex(@"\/Weapons\/w_(\w+_\w+)");
            var rex = new System.Text.RegularExpressions.Regex(@"\/?w_(\w+_\w+?)_\w(?![\/.])");
            var match = rex.Match(value);
            if (match != null && match.Groups.Count >= 2) {
                ident = new WeaponIdentifier(match.Groups[0].Value, match.Groups[1].Value);
                return true;
            }
            ident = null;
            return false;
        }
        private string _slotName;

		private string ParseSlotName(string forceName = null) {
			var knownName = _specialNames.TryGetValue(forceName ?? RawValue, out var name);
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
            return GetSlotName();
        }

        public override string GetSlotName() {
            _slotName ??= ParseSlotName();
			return _slotName;
        }
    }
}