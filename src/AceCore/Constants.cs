using System.Collections.Generic;

namespace AceCore
{
    public static class Constants
    {
        public static Dictionary<string, string> SlotNames {get;} = new Dictionary<string, string> {
            ["00"] = "Osea",
            ["01"] = "Erusea",
            ["02"] = "Special",
            ["03"] = "Mage",
            ["04"] = "Spare",
            ["05"] = "Strider",
			["06"] = "Slot 7",
			["07"] = "Slot 8"
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
            ["zoef"] = "FALKEN"
        };
    }
}