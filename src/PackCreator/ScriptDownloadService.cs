using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Cli.AppInfo;

namespace PackCreator {
    public class ScriptDownloadService {
        private readonly AppInfoService _infoService;

        public ScriptDownloadService(AppInfoService appInfo) {
            _infoService = appInfo;
        }
        public string ScriptFilePath => Path.Combine(GetTempPath(), "u4pak.py");
        public async Task<string> GetScriptPath() {
            var fi = new FileInfo(ScriptFilePath);
            if (fi.Exists && fi.Length > 0) {
                return fi.FullName;
            } else {
                await DownloadScript();
                return fi.FullName;
            }
        }

        public async Task<ScriptContext> GetScriptContext(string targetPath) {
            var sourceFile = await GetScriptPath();
            var targetFile = Path.Combine(targetPath, "u4pak.py");
            File.Copy(sourceFile, targetFile, true);
            return new ScriptContext(targetFile);
        }

        private string GetTempPath() {
            var tempDir = Path.Combine(Path.GetTempPath(), "acmi");
            var di = Directory.CreateDirectory(tempDir);
            return di.FullName;
        }

        private async Task DownloadScript() {
            using (var client = GetClient()) {
                var file = await client.GetStringAsync("https://raw.githubusercontent.com/panzi/u4pak/master/u4pak.py");
                await File.WriteAllTextAsync(ScriptFilePath, file);
            }
        }

        private HttpClient GetClient() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"ACMI/{_infoService.GetAppVersion()}");
            return client;
        }
    }

    public class ScriptContext : IDisposable {
        private FileInfo _tempPath;

        public string CurrentPath => _tempPath.FullName;

        public string WorkingDirectory => _tempPath.DirectoryName;

        public ScriptContext(string targetPath)
        {
            _tempPath = new FileInfo(targetPath);
        }

        public void Dispose() {
            File.Delete(_tempPath.FullName);
        }
    }
}