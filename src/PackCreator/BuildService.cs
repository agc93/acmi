using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using ExecEngine;
using Microsoft.Extensions.Logging;

namespace PackCreator {
    public class BuildService {
        private readonly ILogger<BuildService> _logger;
        private readonly BuildContextFactory _contextFactory;
        private readonly CommandRunner _runner;

        public BuildService(ILogger<BuildService> logger, BuildContextFactory contextFactory, CommandRunner runner) {
            _logger = logger;
            _contextFactory = contextFactory;
            _runner = runner;
        }

        public async Task<FileInfo> RunBuild(string objName, string rootPath, params AssetContext[] contextTargets) {
            var targets = contextTargets.ToList();
            using (var ctx = await _contextFactory.Create(objName))
            {
                foreach (var target in targets)
                {
                    /* var relPath = Path.GetRelativePath(rootPath, target.SourcePath.FullName);
                    if (!string.IsNullOrWhiteSpace(target.PackTargetOverride)) {
                        relPath = (relPath == "." ? target.PackTargetOverride : Path.Combine(target.PackTargetOverride, relPath).Replace('\\', '/')).Replace("/_meta/", "");
                    } */
                    var relPath = target.GetTargetPath(rootPath, s => s.Replace("_meta\\", ""));
                    var linked = string.IsNullOrWhiteSpace(target.FilePattern)
                        ? ctx.AddFolder(relPath, target.SourcePath, target.FileFilter)
                        : ctx.AddFolder(relPath, target.SourcePath, fi => System.Text.RegularExpressions.Regex.IsMatch(fi.Name, target.FilePattern ?? ".*"));
                    if (!linked) {
                        _logger.LogError("[bold red]Failed to add folders to context directory![/]");
                        // Console.ReadLine();
                        return null;
                    }
                }
                var buildResult = ctx.RunBuild(_runner, "packed-files.pak");
                if (buildResult.Success) {
                    _logger.LogInformation($"[bold green]Success![/] Files for {objName.GetFriendlyName()} successfully packed from {targets.Count} folders");
                    var tempFile = Path.GetTempFileName();
                    buildResult.Output.CopyTo(tempFile, true);
                    return new FileInfo(tempFile);
                } else {
                    _logger.LogInformation($"[bold white on red]Failed![/] Files from {Directory.GetParent(contextTargets.First().SourcePath.FullName)} not packed successfully. Continuing...");
                    return null;
                }
            }
        }

        
    }
}