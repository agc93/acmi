using System.IO;

namespace AceCore
{
    public interface IParserService
    {
        Identifier ParseFilePath(FileInfo file, DirectoryInfo fileRoot = null);
        Identifier ParseMatch(string rawString, bool strictParsing = false);
    }
}