using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Cli.AppInfo;
using Spectre.Console;

namespace PackCreator {
    public interface IScriptDownloadService {
        public Task<string> GetScriptPath();
        public Task<BuildScript> GetScriptContext(string targetPath);
    }
    public abstract class ScriptDownloadBase {
        protected readonly AppInfoService _infoService;
        protected virtual string _scriptFilePath {get;}
        private readonly ILogger<IScriptDownloadService> _logger;

        protected virtual string _scriptFileName {get;}

        protected ScriptDownloadBase(string fileName, AppInfoService appInfo, ILogger<IScriptDownloadService> logger) {
            _infoService = appInfo;
            _scriptFileName = fileName;
            _scriptFilePath = Path.Combine(GetTempPath(), _scriptFileName);
            _logger = logger;
        }

        private string GetTempPath() {
            var tempDir = Path.Combine(Path.GetTempPath(), "acmi");
            var di = Directory.CreateDirectory(tempDir);
            return di.FullName;
        }

        protected async Task DownloadScript(string url) {
            _logger.LogInformation($"Downloading new build file: '{_scriptFileName}'");
            using (var client = GetClient()) {
                // var file = await client.GetStringAsync(url);
                // await File.WriteAllTextAsync(_scriptFilePath, file);
                var response = await client.GetAsync(url);
                using (var fs = new FileStream(
                    _scriptFilePath, 
                    FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
        }

        private HttpClient GetClient() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"ACMI/{_infoService.GetAppVersion()}");
            return client;
        }
    }
    public class UnrealPakDownloadService : ScriptDownloadBase, IScriptDownloadService {
        private readonly IAnsiConsole _console;

        public UnrealPakDownloadService(IAnsiConsole console, AppInfoService appInfo, ILogger<UnrealPakDownloadService> logger) : base("UnrealPak.exe", appInfo, logger) {
            _console = console;
        }

        public async Task<BuildScript> GetScriptContext(string targetPath) {
            var sourceFile = await GetScriptPath();
            var targetFile = Path.Combine(targetPath, _scriptFileName);
            File.Copy(sourceFile, targetFile, true);
            return new BuildScript(targetFile);
        }

        public async Task<string> GetScriptPath() {
            var fi = new FileInfo(_scriptFilePath);
            if (fi.Exists && fi.Length > 0) {
                return fi.FullName;
            } else {
                _console.MarkupLine("To build this file, ACMI will now attempt to download UnrealPak, a part of Unreal Engine");
                _console.MarkupLine("Your access to and use of Unreal Engine on GitHub is governed by the Unreal Engine End User License Agreement. If you don't agree to those terms, as amended from time to time, you are not permitted to access or use Unreal Engine.");
                var toContinue = _console.Confirm("Do you want to continue?");
                if (!toContinue) {
                    throw new System.Exception("Aborted download of UnrealPak!");
                }
                await DownloadScript("https://static.modding.app/UnrealPak.exe");
                return fi.FullName;
            }
        }
    }
    public class PythonScriptDownloadService : ScriptDownloadBase, IScriptDownloadService {

        private (string FileUri, string Hash) GetSourceScript() {
            return ("https://raw.githubusercontent.com/panzi/u4pak/8ea81ea019c0fea4e3cbe29720ed9a94e3875e0b/u4pak.py",
                "6e5858b7fee1ccb304218b98886b1a1e");
        }
        

        public PythonScriptDownloadService(AppInfoService appInfo, ILogger<PythonScriptDownloadService> logger) : base("u4pak.py", appInfo, logger) {
        }

        public async Task<string> GetScriptPath() {
            var src = GetSourceScript();
            var fi = new FileInfo(_scriptFilePath);
            if (fi.Exists && fi.Length > 0 && fi.CalculateMD5() == src.Hash) {
                //check if it matches
                return fi.FullName;
            } else {
                await DownloadScript(src.FileUri);
                return fi.FullName;
            }
        }

        public async Task<BuildScript> GetScriptContext(string targetPath) {
            var sourceFile = await GetScriptPath();
            var targetFile = Path.Combine(targetPath, "u4pak.py");
            File.Copy(sourceFile, targetFile, true);
            return new BuildScript(targetFile);
        }
    }
}