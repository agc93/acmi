using System;
using System.Collections.Generic;
using System.Linq;
using AceCore.Parsers;

namespace AceCore
{
    public class InstanceReader : FileReader
    {
        private readonly ParserService _parser;

        public InstanceReader(ParserService parser)
        {
            _parser = parser;
        }
        public IEnumerable<(string Path, SkinIdentifier Identifier)> FindMREC(string filePath, bool fullPath = false) {
            var opts = new SearchOptions {Window = 48, MaxBytes = int.MaxValue, RewindOnMatch = true};
            foreach (var match in FindIdents(filePath, opts)) {
                var ident = _parser.ParseMatch(match, false);
                

                // var fIdent = FancyParseMatch(match);
                if (ident != null && ident is SkinIdentifier sIdent && sIdent.Type == "MREC") {
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
            var opts = new SearchOptions {Window = 50, MaxBytes = int.MaxValue, RewindOnMatch = true};
            foreach (var match in FindIdents(filePath, opts)) {
                var ident = _parser.ParseMatch(match, false);
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