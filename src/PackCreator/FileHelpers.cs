using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PackCreator {
    public static class FileHelpers {
        [DllImport("kernel32.dll")]
        internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
        
        internal static string CalculateMD5(this FileInfo fi)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = fi.OpenRead())
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }

    enum SymbolicLink {
        File = 0,
        Directory = 1
    }
}