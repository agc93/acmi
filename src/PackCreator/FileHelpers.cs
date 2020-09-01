using System.Runtime.InteropServices;

namespace PackCreator {
    public static class FileHelpers {
        [DllImport("kernel32.dll")]
        internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
    }

    enum SymbolicLink {
        File = 0,
        Directory = 1
    }
}