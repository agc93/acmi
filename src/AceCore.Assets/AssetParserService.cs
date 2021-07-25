using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore.Parsers;
using Microsoft.Extensions.Logging;
using UAssetAPI;

namespace AceCore.Assets
{
    public class InstanceAssetReader
    {
        private readonly IParserService _parserService;

        public InstanceAssetReader(IParserService parserService) {
            _parserService = parserService;
        }

        public (string Path, SkinIdentifier Identifier)? FindNormal(string filePath, SkinIdentifier sourceIdentifier) {
            var reader = new AssetReader(filePath, null, null);
            var normal = reader.GetHeaderIndexList()
                .FirstOrDefault(hi => hi.Contains($"{sourceIdentifier.Aircraft}_{sourceIdentifier.Slot}") && hi.Contains("_N.") && hi.Contains("/Game/"));
            var parsed = _parserService.ParseMatch(normal);
            if (parsed is SkinIdentifier {Type: "N"} skinIdentifier) {
                return (normal, skinIdentifier);
            }
            return null;
        }
        
        public (string Path, SkinIdentifier Identifier)? FindMREC(string filePath, SkinIdentifier sourceIdentifier) {
            var reader = new AssetReader(filePath, null, null);
            var mrec = reader.GetHeaderIndexList()
                .FirstOrDefault(hi => hi.Contains($"{sourceIdentifier.Aircraft}_") && hi.Contains("MREC") && hi.Contains("/Game/"));
            var parsed = _parserService.ParseMatch($"{mrec}.uasset"); //TODO: oh boy what a fuckin hack
            if (parsed is SkinIdentifier {Type: "MREC"} skinIdentifier) {
                return (mrec, skinIdentifier);
            }
            return null;
        }
    }
    [Obsolete("Not ready for use", true)]
    public class AssetParserService : IParserService
    {
        private readonly IEnumerable<IIdentifierParser> _parsers;

        public AssetParserService(IEnumerable<IIdentifierParser> parsers, ILogger<AssetParserService> logger) {
            _parsers = parsers;
        }
        
        public Identifier ParseFilePath(string assetPath, FileInfo file, DirectoryInfo fileRoot = null) {
            var matched = _parsers.Select(p => p.TryParse(assetPath, false)).FirstOrDefault(m => m.IsValid);
            if (matched.identifier != null) {
                return matched.identifier;
            } else {
                matched = _parsers.Select(p => p.TryParse(file.Name, false)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    return matched.identifier;
                } else if (fileRoot != null && fileRoot.Exists) {
                    var pathMatched = _parsers
                        .Select(p =>
                            p.TryParse(Path.GetRelativePath(fileRoot.FullName, file.FullName).Replace('\\', '/'),
                                false)).FirstOrDefault(m => m.IsValid);
                    if (pathMatched.identifier != null) {
                        return pathMatched.identifier;
                    }
                }
                return null;
            }
        }

        public Identifier ParseFilePath(FileInfo file, DirectoryInfo fileRoot = null) {
            var assetPathMatch = file.Name;
            try {
                var reader = new AssetReader(file.FullName, null, null);
                
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
            throw new System.NotImplementedException();
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