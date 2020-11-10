using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AceCore.Parsers;

namespace AceCore {
    public class PakReader : FileReader {
        private readonly IEnumerable<IIdentifierParser> _parsers;

        public PakReader(IEnumerable<IIdentifierParser> parsers) {
            _parsers = parsers;
        }

        public IEnumerable<Identifier> ReadFile(string filePath, bool readWholeFile = false) {
            var headerFound = false;
            Identifier ParseMatch(string rawString) {
                var matched = _parsers.Select(p => p.TryParse(rawString, true)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    return matched.identifier;
                }
                return null;
            }
            int GetOffsetLength(long fileSize) {
                return (int)Math.Min(Math.Max(Math.Ceiling(fileSize * 0.025), 8192), 65536);
            }
            if (readWholeFile) {
                /* foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = int.MaxValue })) {
                    var ident = ParseMatch(match);
                    if (ident != null) {
                        yield return ident;
                    }
                } */ //this is just waaaaaaaaaaaaaayyyyyyyyyyyyyyyyy tooooooooooo ssslllllloooooooowwwwwwww
                var searchLength = GetOffsetLength(new FileInfo(filePath).Length);
                // foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = 16384, Key = "Nimbus/Content/" }, seekAction: () => (-16384, SeekOrigin.End))) {
                foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = searchLength, Key = "Nimbus/Content/" }, seekAction: () => (-searchLength, SeekOrigin.End))) {
                        var ident = ParseMatch(match);
                        if (ident != null) {
                            yield return ident;
                        }
                    }
            } else {
                foreach (var match in FindIdents(filePath)) {
                    var ident = ParseMatch(match);
                    // var fIdent = FancyParseMatch(match);
                    if (ident != null) {
                        headerFound = true;
                        yield return ident;
                    }
                }
                if (!headerFound) {
                    foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = 8192, Key = "Nimbus/Content/" }, seekAction: () => (-8192, SeekOrigin.End))) {
                        var ident = ParseMatch(match);
                        if (ident != null) {
                            headerFound = true;
                            yield return ident;
                        }
                    }
                }
                if (!headerFound && new FileInfo(filePath).Length < 1049000) {
                    foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = int.MaxValue })) {
                        var ident = ParseMatch(match);
                        if (ident != null) {
                            yield return ident;
                        }
                    }
                }
            }
            yield break;
        }

        private void _FindIdents() {
            var searcher = new BoyerMooreBinarySearch(System.Text.Encoding.UTF8.GetBytes("/Game/"));
        }   
    }
}