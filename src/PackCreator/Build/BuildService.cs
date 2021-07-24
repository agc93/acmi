using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using BuildEngine;
using BuildEngine.Builder;
using ExecEngine;
using Microsoft.Extensions.Logging;
using PackCreator.Build;
using UnPak.Core;

namespace PackCreator {
    
    public class PackBuildService : BuildService<DirectoryBuildContext>
    {
        private readonly ILogger<PackBuildService> _logger;

        private readonly PakFileProvider _pakFileProvider;
        // private readonly IModBuilder _modBuilder;

        public PackBuildService(DirectoryBuildContext context, ILogger<PackBuildService> logger, PakFileProvider pakFileProvider) : base(context) {
            _logger = logger;
            _pakFileProvider = pakFileProvider;
            // _modBuilder = modBuilder;
        }

        public override  Task<(bool Success, FileSystemInfo Output)> RunBuildAsync(string targetFileName) {
            var writer = _pakFileProvider.GetWriter();
            var gameDir = new DirectoryInfo(Directory.GetDirectories(Context.WorkingDirectory.FullName).First());
            var targetBuildPath = Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(Context.WorkingDirectory.FullName, targetFileName);
            // var fi = writer.BuildFromDirectory(gameDir, new FileInfo(targetBuildPath), new PakFileCreationOptions {Compression = new PackageCompression(CompressionMethod.Zlib)});
            var fi = writer.BuildFromDirectory(gameDir, new FileInfo(targetBuildPath));
            return Task.FromResult((fi.Exists, (FileSystemInfo) fi));
        }
    }

    public class BuildService {
        private readonly ILogger<BuildService> _logger;
        private readonly DirectoryBuildContextFactory _contextFactory;
        // private readonly IBuildRunner _runner;

        public BuildService(ILogger<BuildService> logger, DirectoryBuildContextFactory contextFactory) {
            _logger = logger;
            _contextFactory = contextFactory;
            // _runner = runner;
        }


        public async Task<FileInfo> RunBuild(string objName, string rootPath, params BuildInstruction[] contextTargets) {
            var targets = contextTargets.ToList();
            using (var ctx = _contextFactory.CreateContext(objName))
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
                // var buildResult = _runner.RunBuild(null, "packed-files.pak");
                (bool Success, FileInfo Output) buildResult = (false, null);
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