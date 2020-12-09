using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using ExecEngine;
using Microsoft.Extensions.Logging;
using PackCreator.Build;

namespace PackCreator {
    public class BuildService {
        private readonly ILogger<BuildService> _logger;
        private readonly BuildContextFactory _contextFactory;
        private readonly IBuildRunner _runner;

        public BuildService(ILogger<BuildService> logger, BuildContextFactory contextFactory, IBuildRunner runner) {
            _logger = logger;
            _contextFactory = contextFactory;
            _runner = runner;
        }


        public async Task<FileInfo> RunBuild(string objName, string rootPath, params BuildInstruction[] contextTargets) {
            var targets = contextTargets.ToList();
            using (var ctx = await _contextFactory.Create(objName))
            {
                foreach (var target in targets)
                {
                    var linked = ctx.AddFromInstruction(target);
                    if (!linked) {
                        _logger.LogError("[bold red]Failed to add folders to context directory![/]");
                        // Console.ReadLine();
                        return null;
                    }
                }
                var buildResult = _runner.RunBuild(ctx.BuildScript, "packed-files.pak");
                // var buildResult = ctx.RunBuild(_runner, "packed-files.pak");
                if (buildResult.Success) {
                    _logger.LogInformation($"[bold green]Success![/] Files for {objName.GetFriendlyName()} successfully packed from {targets.Sum(t => t.SourceFiles.Count)} files (in {targets.Count} targets)");
                    var tempFile = Path.GetTempFileName();
                    buildResult.Output.CopyTo(tempFile, true);
                    return new FileInfo(tempFile);
                } else {
                    _logger.LogInformation($"[bold white on red]Failed![/] Files from {Directory.GetParent(contextTargets.First().SourceGroup)} not packed successfully. Continuing...");
                    return null;
                }
            }
        }
    }
}