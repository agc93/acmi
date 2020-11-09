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
                var buildResult = ctx.RunBuild(_runner, "packed-files.pak");
                if (buildResult.Success) {
                    _logger.LogInformation($"[bold green]Success![/] Files for {objName.GetFriendlyName()} successfully packed from {targets.Sum(t => t.SourceFiles.Count)} ({targets.Count}) files");
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

    public static class BuildExtensions {
        public static bool AddFromInstruction(this BuildContext ctx, BuildInstruction instruction) {
            var results = new List<bool>();
            foreach (var file in instruction.SourceFiles)
            {
                results.Add(ctx.AddFile(instruction.TargetPath, file));
            }
            return results.All(r => r == true);
        }
    }
}