using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore.Parsers;

namespace AceCore
{
    public class ParserService : IParserService
    {
        private readonly IEnumerable<IIdentifierParser> _parsers;

        public ParserService(IEnumerable<IIdentifierParser> parsers)
        {
            _parsers = parsers.OrderBy(p => p.Priority);
        }

        public Identifier ParseFilePath(FileInfo file, DirectoryInfo fileRoot = null) {
            var matched = _parsers.Select(p => p.TryParse(file.Name, false)).FirstOrDefault(m => m.IsValid);
            if (matched.identifier != null) {
                return matched.identifier;
            } else if (fileRoot != null && fileRoot.Exists){
                var pathMatched = _parsers.Select(p => p.TryParse(Path.GetRelativePath(fileRoot.FullName, file.FullName).Replace('\\', '/'), false)).FirstOrDefault(m => m.IsValid);
                if (pathMatched.identifier != null) {
                    return pathMatched.identifier;
                }
            }
            return null;
        }

        public Identifier ParseMatch(string rawString, bool strictParsing = false) {
            var matched = _parsers.Select(p => p.TryParse(rawString, strictParsing)).FirstOrDefault(m => m.IsValid);
            if (matched.identifier != null) {
                return matched.identifier;
            }
            return null;
        }
    }
}