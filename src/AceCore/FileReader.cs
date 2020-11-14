using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AceCore
{
    public abstract class FileReader
    {
        protected FileReader()
        {
            
        }

        protected IEnumerable<string> FindIdents(string filePath, SearchOptions opts = null, Func<(int offset, SeekOrigin origin)> seekAction = null) {
            // const int maxBytes = 4096;
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                opts ??= new SearchOptions();
                if (stream.Length == 0) {
                    yield break;
                }
                var key = opts.Key;
                if (seekAction != null) {
                    var seek = seekAction();
                    /* var offset = seek.offset < stream.Length
                        ? stream.Length
                        : seek.offset; */
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
                        var rawBytes = reader.ReadBytes(opts.Window);
                        if (opts.RewindOnMatch && reader.BaseStream.CanSeek) {
                            reader.BaseStream.Seek(-opts.Window, SeekOrigin.Current);
                        }
                        var rawString = Encoding.UTF8.GetString(rawBytes);
                        pos = 0;
                        yield return rawString;
                    }
                }
            }
            yield break;
        }
    }
}