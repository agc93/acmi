using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;
using UnPak.Core;

namespace InstallerCreator
{
    public class PakFileReader
    {
        private readonly ParserService _parserService;
        private readonly PakFileProvider _pakFileProvider;

        public PakFileReader(ParserService parserService, PakFileProvider pakFileProvider) {
            _parserService = parserService;
            _pakFileProvider = pakFileProvider;
        }

        public IEnumerable<Identifier> ReadFile(string filePath, PakReaderOptions readOptions = null) {
            return FindIdents(filePath).Select(match => _parserService.ParseMatch(match, true))
                .Where(ident => ident != null);
        }

        private IEnumerable<string> FindIdents(string filePath) {
            using var stream = File.OpenRead(filePath);
            using var reader = _pakFileProvider.GetReader(stream);
            var pakFile = reader.ReadFile();
            return pakFile.Records.Select(r => r.FileName);
        }
        
    }
}