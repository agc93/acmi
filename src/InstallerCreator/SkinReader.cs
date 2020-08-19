using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InstallerCreator {
    public class SkinReader {
        public SkinReader() {
        }

        public SkinIdentifier ReadSkinSlot(string filePath) {
            var rawString = FindSkinIdent(filePath);
            var parsed = SkinIdentifier.TryParse(rawString, out var skinId);
            if (parsed) {
                return skinId;
            } else {
                return null;
            }
        }

        private string FindSkinIdent(string filePath) {
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                const string key = "/Game/";
                int pos = 0;

                while (stream.Position < stream.Length && pos < key.Length)
                {
                    if (reader.ReadByte() == key[pos]) pos++;
                    else pos = 0;
                }

                if (stream.Position == stream.Length) // we went through the entire stream without finding the key
                    throw new KeyNotFoundException("Could not find key '" + key + "' in pak file");

                // otherwise pos == key.Length, which means we found it
                // int offset = 136 - key.Length - sizeof(int);
                // stream.Seek(offset, SeekOrigin.Current); // advance past junk to beginning of string
                var offset = stream.Position;
                

                var rawBytes = reader.ReadBytes(64);
                var rawString = Encoding.UTF8.GetString(rawBytes);
                return rawString;
            }
        }
    }
}