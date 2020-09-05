using System.ComponentModel;

namespace PackCreator {
    public static class PackingHelpers {
        public enum NPCPackMode {
            [Description("Separately")]
            None,
            [Description("For this aircraft")]
            PerAircraft,
            [Description("For all aircraft")]
            Global
        }

        public enum PortraitPackMode {

        }

        public enum OutputModes {
            [Description("Directly output to the mods folder")]
            None,
            [Description("A separate folder in the mods folder")]
            SubDirectory,
            [Description("Auto-organized into a complete set of folders")]
            NestedFolders
        }

    }
}