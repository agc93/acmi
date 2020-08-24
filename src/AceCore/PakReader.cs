using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AceCore.Parsers;

namespace AceCore {
    internal class SearchOptions {
        public string Key { get; set; } = "/Game/";
        public int Window { get; set; } = 64;
        public int MaxBytes = 4096;
    }
    public class PakReader {
        private readonly IEnumerable<IIdentifierParser> _parsers;

        public PakReader(IEnumerable<IIdentifierParser> parsers) {
            _parsers = parsers;
        }

        public IEnumerable<Identifier> ReadFile(string filePath, bool readWholeFile = false) {
            var headerFound = false;
            Identifier ParseMatchExplicit(string rawString) {
                if (SkinIdentifier.TryParse(rawString, out var skinId) && skinId.Type == "D") {
                    return skinId;
                } else if (PortraitIdentifier.TryParse(rawString, out var hudId)) {
                    return hudId;
                } else if (CrosshairIdentifier.TryParse(rawString, out var chId)) {
                    return chId;
                }
                return null;
            }
            Identifier ParseMatch(string rawString) {
                var matched = _parsers.Select(p => p.TryParse(rawString)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    return matched.identifier;
                }
                return null;
            }
            if (readWholeFile) {
                foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = int.MaxValue })) {
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
                    foreach (var match in FindIdents(filePath, new SearchOptions() { MaxBytes = 8192, Key = "Nimbus/" }, seekAction: () => (-8192, SeekOrigin.End))) {
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

        private IEnumerable<string> FindIdents(string filePath, SearchOptions opts = null, Func<(int offset, SeekOrigin origin)> seekAction = null) {
            // const int maxBytes = 4096;
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                opts ??= new SearchOptions();
                var key = opts.Key;
                if (seekAction != null) {
                    var seek = seekAction();
                    stream.Seek(seek.offset, seek.origin);
                    opts.MaxBytes = Convert.ToInt32(stream.Position) + opts.MaxBytes;
                }
                var maxBytes = Convert.ToInt64(opts.MaxBytes);
                int pos = 0;

                while (stream.Position < maxBytes && stream.Position < stream.Length) {
                    while (stream.Position < maxBytes && stream.Position < stream.Length && pos < key.Length) {
                        if (reader.ReadByte() == key[pos]) pos++;
                        else pos = 0;
                    }
                    // otherwise pos == key.Length, which means we found it
                    if (pos == key.Length) {
                        var rawBytes = reader.ReadBytes(64);
                        var rawString = Encoding.UTF8.GetString(rawBytes);
                        pos = 0;
                        yield return rawString;
                    }
                    // var offset = stream.Position;



                    // stream.Seek(64, SeekOrigin.Current)
                }

                /* if (stream.Position == stream.Length) // we went through the entire stream without finding the key
                    throw new KeyNotFoundException("Could not find key '" + key + "' in pak file"); */
            }
            yield break;
        }
    }
}