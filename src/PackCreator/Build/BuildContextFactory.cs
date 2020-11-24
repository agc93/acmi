using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PackCreator {
    public class BuildContextFactory {
        private readonly IScriptDownloadService _scriptService;
        private readonly ILogger<BuildContext> _logger;

        public BuildContextFactory(IScriptDownloadService scriptService, ILogger<BuildContext> logger)
        {
            _scriptService = scriptService;
            _logger = logger;
        }
        public async Task<BuildContext> Create(string contextName) {
            var targetPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "acmi", contextName ?? Guid.NewGuid().ToString()));
            targetPath.Create();
            var scriptContext = await _scriptService.GetScriptContext(targetPath.FullName);
            return new BuildContext(scriptContext, targetPath, _logger);
        }
    }
}