using System;
using System.Linq;

namespace AceCore
{
    public static class CoreExtensions
    {
        public static string CleanPath(this string rootPath) {
            return rootPath.Trim('\"', '/', '\\');
        }

        internal static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };
    }
}