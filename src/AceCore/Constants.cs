using System;
using System.Collections.Generic;
using System.Linq;
using AceCore.Vehicles;

namespace AceCore
{
    public static class Constants
    {
        public static Dictionary<string, string> AllVehicleNames => new List<VehicleSlot>().Concat(NonPlayableAircraft).Concat(Vessels).ToDictionary(k => k.ObjectName, v => v.Name);
        public static Dictionary<string, string> AllItemNames => new Dictionary<string, string>().Concat(AllVehicleNames).Concat(AircraftNames).Concat(WeaponNames).ToDictionary(k => k.Key, v => v.Value);

        public static Dictionary<string, string> AllNames => new Dictionary<string, string>().Concat(AllItemNames).Concat(SlotNames).Concat(ModTypes).Concat(Aces).ToDictionary(k => k.Key, v => v.Value);
        public static List<AircraftSlot> NonPlayableAircraft => new List<AircraftSlot> {
            new AircraftSlot("a130", "AC-130") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("adfx10", "ADFX-10") { Factions = NPCFaction.Universal},
            new AircraftSlot("ah64", "AH-64") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("aias", "Arsenal Bird") { Factions = NPCFaction.Universal},
            new AircraftSlot("av8b", "AV-8B") {Factions = NPCFaction.Erusea},
            new AircraftSlot("b01b", "B-1B") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("b02a", "B-2A") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("b52", "B-52H") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("c01a", "C-1A") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("c130", "C-130") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("c17g", "C-17G") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("ch47", "CH-47") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("e18g", "EA-18G") {Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("f17a", "F-117A") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("il76", "Il-76") { Factions = NPCFaction.Erusea},
            new AircraftSlot("kc10", "KC-10") { Factions = NPCFaction.Osea},
            new AircraftSlot("m101", "MQ-101") { Factions = NPCFaction.Osea},
            new AircraftSlot("mv22", "MV-22") { Factions = NPCFaction.Osea|NPCFaction.Erusea},
            new AircraftSlot("p01a", "P-1") { Factions = NPCFaction.Osea},
            new AircraftSlot("sups", "Supply Ship") { Factions = NPCFaction.Universal},
            new AircraftSlot("t095", "Tu-95") { Factions = NPCFaction.Erusea},
            new AircraftSlot("t160", "Tu-160") {Factions = NPCFaction.Erusea},
            new AircraftSlot("uavc", "MQ-99") { Factions = NPCFaction.Universal},
            new AircraftSlot("uavr", "Barrier Drone") { Factions = NPCFaction.Osea },
            new AircraftSlot("uavs", "SLUAV") { Factions = NPCFaction.Osea}
        };
        public static List<VesselSlot> Vessels => new List<VesselSlot> {
            new VesselSlot("aegs", "Aegis: Ticonderoga-class Cruiser"),
            new VesselSlot("alcn", "Alicorn"),
            new VesselSlot("arle", "Destroyer: Arleigh Burke-class"),
            new VesselSlot("ctsl", "Container Ship"),
            new VesselSlot("free", "LCS: Freedom-class Littoral Combat Ship"),
            new VesselSlot("gors", "Aegis: Admiral Gorshkov-class Frigate"),
            new VesselSlot("grig", "Frigate: Admiral Grigorovich-class Frigate"),
            new VesselSlot("gunb", "Gunboat"),
            new VesselSlot("krov", "Battlecruiser: Kirov-class"),
            new VesselSlot("kuzn", "Carrier: Admiral Kuznetsov-class Aircraft Cruiser"),
            new VesselSlot("mslb", "Missile Boat"),
            new VesselSlot("nimi", "Carrier: Nimitz-class Aircraft Carrier"),
            new VesselSlot("oilt", "Oil Tanker"),
            new VesselSlot("sana", "Landing Ship: San Antonio-class Landing Dock"),
            new VesselSlot("slav", "Cruiser: Slava-class Guided Missile Cruiser"),
            new VesselSlot("udal", "Destroyer: Udaloy-class Guided Missile Destroyer"),
            new VesselSlot("zumw", "Destroyer: Zumwalt-class Destroyer")
            // new VesselSlot("arle")
        };

        public static Dictionary<string, string> ModTypes = new Dictionary<string, string> {
            ["SubtitleSpeakerPortrait"] = "Radio Portraits"
        };

        public static Dictionary<string, string> WeaponNames = new Dictionary<string, string> {
            ["w_dptk_a0"] = "Droptanks",
            ["w_lacm_f0"] = "LACM (Rafale)",
            ["w_lacm_e0"] = "LACM (Gripen)",
            ["w_lacm_r0"] = "LACM (Russian)",
            ["w_4aam_a0"] = "4AAM/8AAM/6AAM/HCAA",
            ["w_4aam_f0"] = "Mirage/Rafale HCAA/4AAM",
            ["w_4aam_j0"] = "HCAA (F-15J)",
            ["w_4aam_r0"] = "Russian 4AAM/6AAM",
            ["w_4aam_r1"] = "4AAM (Su-57)",
            ["w_4aam_x0"] = "4AAM (X-02S)",
            ["w_4agm_a0"] = "4AGM (A-10C)",
            ["w_4agm_r0"] = "4AGM (Sukhoi)",
            ["w_8agm_a0"] = "8AGM (Typhoon/F-35)",
            ["w_aam_a0"] = "Hellfire",
            ["w_asrc_a0"] = "VL-ASROC",
            ["w_bom_x0"] = "UGB (Tu-95/Tu-160)",
            ["w_ecpm_x0"] = "IEWS",
            ["w_gpb_a0"] = "GPB (F-14/F-16)",
            ["w_gpb_f0"] = "GBP (Mirage)",
            ["w_gpb_r0"] = "GBP (Su-57)",
            ["w_grkt_a0"] = "GRKT (F-104C)",
            ["w_icbm"] = "IRBM",
            ["w_laam_a0"] = "LAAM (Phoenix)",
            ["w_laam_e0"] = "LAAM (Rafale/Typhoon)",
            ["w_laam_r0"] = "LAAM (MiG-31)",
            ["w_laam_r1"] = "LAAM (Su-35)",
            ["w_lagm_a0"] = "LAGM (F-4E)",
            ["w_lasm_a0"] = "LASM (F/A 18F)",
            ["w_lasm_f0"] = "LASM (Mirage)",
            ["w_lasm_j1"] = "LASM (F-2A)",
            ["w_lasm_r0"] = "LASM/LAGM (Russian)",
            ["w_lasm_x0"] = "LASM (X-02S)",
            ["w_mop"] = "Targeting Pod Bomb",
            ["w_msl_f1"] = "MSL (Mirage/Rafale)",
            ["w_msl_j0"] = "MSL (F-2A/F-15J)",
            ["w_msl_r0"] = "MSL (Russian)",
            ["w_qaam_j0"] = "QAAM (F-15J)",
            ["w_qaam_r0"] = "QAAM (Russian)",
            ["w_qaam_x0"] = "QAAM (Mimic)",
            ["w_rkt_a0"] = "RKT (A-10C/F-2A)",
            ["w_rkt_r0"] = "RKT (MiG-21)",
            ["w_rktl_a0"] = "Rocket Pod (A-10C)",
            ["w_rktl_j0"] = "Rocket Pod (F-2A)",
            ["w_rktl_r0"] = "Rocket Pod (MiG-21)",
            ["w_saam_a0"] = "SAAM/HVAA",
            ["w_saam_r0"] = "SAAM/HVAA (Russian)",
            ["w_saam_r1"] = "SAAM (MiG-31)",
            ["w_sffs_a0"] = "SFFS (F-15E)",
            ["w_sffs_r0"] = "SFFS (Su-34)",
            ["w_smin_r0"] = "Alicorn Jammer",
            ["w_sobuy_j0"] = "P-1 Buoy",
            ["w_tls_x0"] = "TLS (F-15E/Su-37)",
            ["w_tls_x1"] = "TLS (ADFX-01)",
            ["w_tls_x2"] = "TLS (ADF-01)",
            ["w_tls_x3"] = "TLS (ADF-11F)",
            ["w_trpd_j0"] = "VL-ASROC Torpedo",
            ["w_uav_x0"] = "UAV (ADF-11F)",
            ["w_ugb_r0"] = "UGB (Su-47)"
        };
        [Obsolete("Don't use this", false)]
        public static List<AircraftSlot> PlayerAircraft => new List<AircraftSlot> {
            new AircraftSlot("a10a", "A-10C", new[] {"00a", "02a"}),
            new AircraftSlot("adf11f", "ADF-11F") { NPCSlots = new List<string>{"02a"} },
            new AircraftSlot("f02a", "F-2A") { NPCSlots = new List<string> {"01a", "02a"}},
            new AircraftSlot("f04e", "F-4E", new[] {"01a"}),
            new AircraftSlot("f104", "F-104C"),
            new AircraftSlot("f104av", "Avril F-104"),
            new AircraftSlot("f14d", "F-14D", new[] {"00b", "00c", "01a", "02a"}),
            new AircraftSlot("f15c", "F-15C", new[] {"b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o"}.Select(s => "00" + s).Concat(new[] {"01a", "02a"})),
            new AircraftSlot("f15e", "F-15E", new[] {"00b", "01a", "02a"}),
            new AircraftSlot("f15j", "F-15J", new[] {"01a", "02a"}),
            new AircraftSlot("f16c", "F-16C", new[] {"00b", "00c", "00d", "00e", "01a", "02a"}),
            new AircraftSlot("f18f", "F/A-18F", new[] {"00b", "00d", "00e", "00f", "00g", "00h", "00i", "01a", "02a"}),
            new AircraftSlot("f22a", "F-22A", new[] { "00b", "01a", "02a"}),
            new AircraftSlot("f35c", "F-35C", new[] { "00b", "01a", "02a"}),
            new AircraftSlot("j39e", "Gripen", new[] { "01a", "02a"}),
            new AircraftSlot("m21b", "MiG-21", new[] {"01a", "02a"}),
            new AircraftSlot("m29a", "MiG-29", new[] {"00c", "01a", "02a"}), 
            new AircraftSlot("m31b", "MiG-31", new[] {"01a", "02a"}),
            new AircraftSlot("mr2k", "Mirage", new[] {"00b", "00c", "01a", "02a"}),
            new AircraftSlot("mrgn", "ADFX-01"), 
            new AircraftSlot("pkfa", "Su-57", new[] {"01a", "02a"}),
            new AircraftSlot("rflm", "Rafale M", new[] {"00b", "01a", "02a", "06a"}),
            new AircraftSlot("su30", "Su-30M2", new[] {"01a", "02a"}),
            new AircraftSlot("su30sm", "Su-30SM", new[] {"02a"}),
            new AircraftSlot("su33", "Su-33", new[] {"00c", "01a", "02a"}),
            new AircraftSlot("su34", "Su-34", new[] {"01a", "02a"}),
            new AircraftSlot("su35", "Su-35", new[] {"01a", "02a"}),
            new AircraftSlot("su37", "Su-37", new[] {"01a", "02a"}),
            new AircraftSlot("su47", "Su-47", new[] {"01a", "02a", "06a", "06b"}),
            new AircraftSlot("typn", "Typhoon", new[] {"00b", "01a", "02a"}),
            new AircraftSlot("x02s", "X-02S", new[] {"02a"}),
            new AircraftSlot("yf23", "YF-23", new[] {"01a", "02a"}),
            new AircraftSlot("zoef", "FALKEN"),
            new AircraftSlot("fa44", "CFA-44"),
            new AircraftSlot("fa27", "XFA-27"),
            new AircraftSlot("asfx", "ASF-X"),
            new AircraftSlot("f15m", "F-15 S-MTD"),
            new AircraftSlot("fb22", "FB-22"),
            new AircraftSlot("f16x", "F-16XL")
        };
        public static Dictionary<string, string> SlotNames {get;} = new Dictionary<string, string> {
            ["00"] = "Osea",
            ["01"] = "Erusea",
            ["02"] = "Special",
            ["03"] = "Mage",
            ["04"] = "Spare",
            ["05"] = "Strider",
			["06"] = "Slot 7",
			["07"] = "Slot 8",
            ["02a"] = "NPC Osea",
            ["01a"] = "NPC Erusea",
            ["06a"] = "SACS/GRGM",
            ["06b"] = "SACS/GRGM"
        };
        public static Dictionary<string, string> AircraftNames {get;} = new Dictionary<string, string> {
            ["a10a"] = "A-10C",
            ["adf11f"] = "ADF-11F",
            ["f02a"] = "F-2A",
            ["f04e"] = "F-4E",
            ["f14d"] = "F-14D",
            ["f15c"] = "F-15C",
            ["f15e"] = "F-15E",
            ["f15j"] = "F-15J",
            ["f16c"] = "F-16C",
            ["f18f"] = "F/A-18F",
            ["f22a"] = "F-22A",
            ["f35c"] = "F-35C",
            ["f104"] = "F-104C",
            ["f104av"] = "Avril F-104",
            ["j39e"] = "Gripen",
            ["m21b"] = "MiG-21",
            ["m29a"] = "MiG-29",
            ["m31b"] = "MiG-31",
            ["mr2k"] = "Mirage",
            ["mrgn"] = "ADFX-01",
            ["pkfa"] = "Su-57",
            ["rflm"] = "Rafale M",
            ["su30"] = "Su-30M2",
            ["su30sm"] = "Su-30SM",
            ["su33"] = "Su-33",
            ["su34"] = "Su-34",
            ["su35"] = "Su-35S",
            ["su37"] = "Su-37",
            ["su47"] = "Su-47",
            ["typn"] = "Typhoon",
            ["x02s"] = "X-02S",
            ["yf23"] = "YF-23",
            ["zoef"] = "FALKEN",
            ["fa44"] = "CFA-44",
            ["fa27"] = "XFA-27",
            ["asfx"] = "ASF-X",
            ["f15m"] = "F-15 SMTD",
            ["fb22"] = "FB-22",
            ["f16x"] = "F-16XL",
            ["f14a"] = "F-14A",
            ["f14atg"] = "F-14A (Top Gun)",
            ["f18e"] = "F/A-18E",
            ["f18etg"] = "F/A-18E (Top Gun)",
            ["f02x"] = "F-2A Super Kai",
            ["f18x"] = "F/A-18F Block III",
            ["m35d"] = "MiG-35D",
            ["dark"] = "DarkStar",
            ["su57tg"] = "Su-57 (Top Gun)"
        };

        public static Dictionary<string, string> Aces => new Dictionary<string, string> {

        };
    }
}