using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AceCore.Parsers;

namespace AceCore {
    public class PakReaderOptions {
        public bool ReadWholeFile {get;set;} = false;
        public bool TreatAsLightweight {get;set;} = false;
    }
    public class PakReader : FileReader {
        private readonly ParserService _parser;

        public PakReader(ParserService parser) {
            _parser = parser;
        }

        public IEnumerable<Identifier> ReadFile(string filePath, PakReaderOptions readOptions = null) {
            readOptions ??= new PakReaderOptions();
            var headerFound = false;
            int GetOffsetLength(long fileSize) {
                return (int)Math.Min(Math.Max(Math.Ceiling(fileSize * 0.025), 8192), 65536);
            }
            if (readOptions.ReadWholeFile) {
                var searchLength = GetOffsetLength(new FileInfo(filePath).Length);
                // foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = 16384, Key = "Nimbus/Content/" }, seekAction: () => (-16384, SeekOrigin.End))) {
                foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = searchLength, Key = "Nimbus/Content/" }, seekAction: () => (-searchLength, SeekOrigin.End))) {
                        var ident = _parser.ParseMatch(match, true);
                        if (ident != null) {
                            yield return ident;
                        }
                    }
            } else {
                var opts = new SearchOptions {MaxBytes = 8192, Key = "Nimbus/Content/", RewindOnMatch = true, Window = 96};
                foreach (var match in FindIdents(filePath, opts, seekAction: () => (-8192, SeekOrigin.End))) {
                    var ident = _parser.ParseMatch(match, !readOptions.TreatAsLightweight);
                    if (ident != null) {
                        headerFound = true;
                        yield return ident;
                    }
                }
                if (!headerFound) {
                    foreach (var match in FindIdents(filePath)) {
                        var ident = _parser.ParseMatch(match, !readOptions.TreatAsLightweight);
                        // var fIdent = FancyParseMatch(match);
                        if (ident != null) {
                            headerFound = true;
                            yield return ident;
                        }
                    }
                }
                if (!headerFound && new FileInfo(filePath).Length < 1049000) {
                    foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = int.MaxValue })) {
                        var ident = _parser.ParseMatch(match, !readOptions.TreatAsLightweight);
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