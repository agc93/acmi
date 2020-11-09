using System;
using System.Collections.Generic;
using System.Linq;
using AceCore.Parsers;

namespace AceCore
{
    public class InstanceReader : FileReader
    {
        private readonly IEnumerable<IIdentifierParser> _parsers;
        public InstanceReader(IEnumerable<IIdentifierParser> parsers)
        {
            _parsers = parsers;
        }
        public IEnumerable<(string Path, SkinIdentifier Identifier)> FindMREC(string filePath, bool fullPath = false) {
            var headerFound = false;
            Identifier ParseMatch(string rawString) {
                var matched = _parsers.Select(p => p.TryParse(rawString, false)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    
                    return matched.identifier;
                }
                return null;
            }
            var opts = new SearchOptions {Window = 42, MaxBytes = int.MaxValue, RewindOnMatch = true};
            foreach (var match in FindIdents(filePath, opts)) {
                var ident = ParseMatch(match);
                

                // var fIdent = FancyParseMatch(match);
                if (ident != null && ident is SkinIdentifier sIdent && sIdent.Type == "MREC") {
                    headerFound = true;
                    var pathIdx = match.IndexOf(ident.RawValue);
                    var path = fullPath 
                        ? opts.Key + match.Substring(0, pathIdx) + ident.RawValue
                        : match.Substring(0, pathIdx);
                    yield return (path, sIdent);
                }
            }
            yield break;
        }

        public IEnumerable<(string Path, SkinIdentifier Identifier)> FindNormal(string filePath, bool fullPath = false) {
            Identifier ParseMatch(string rawString) {
                var matched = _parsers.Select(p => p.TryParse(rawString, false)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    
                    return matched.identifier;
                }
                return null;
            }
            var opts = new SearchOptions {Window = 50, MaxBytes = int.MaxValue, RewindOnMatch = true};
            foreach (var match in FindIdents(filePath, opts)) {
                var ident = ParseMatch(match);
                

                // var fIdent = FancyParseMatch(match);
                if (ident != null && ident is SkinIdentifier sIdent && sIdent.Type == "N") {
                    var pathIdx = match.IndexOf(ident.RawValue);
                    var path = fullPath 
                        ? opts.Key + match.Substring(0, pathIdx) + ident.RawValue
                        : match.Substring(0, pathIdx);
                    yield return (path, sIdent);
                }
            }
            yield break;
        }
    }
}