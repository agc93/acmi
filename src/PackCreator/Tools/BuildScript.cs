using System;
using System.IO;

namespace PackCreator {
    public class BuildScript : IDisposable {
        private FileInfo _tempPath;

        public string CurrentPath => _tempPath.FullName;

        public string WorkingDirectory => _tempPath.DirectoryName;

        public BuildScript(string targetPath, params string[] defaultArgs)
        {
            _tempPath = new FileInfo(targetPath);
        }

        public void Dispose() {
            File.Delete(_tempPath.FullName);
        }
    }
}