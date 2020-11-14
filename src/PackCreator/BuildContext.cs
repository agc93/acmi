using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AceCore;
using ExecEngine;
using Microsoft.Extensions.Logging;

namespace PackCreator {
    public class BuildContextFactory {
        private readonly ScriptDownloadService _scriptService;
        private readonly ILogger<BuildContext> _logger;

        public BuildContextFactory(ScriptDownloadService scriptService, ILogger<BuildContext> logger)
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
    public class BuildContext : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
        private readonly DirectoryInfo _workingDirectory;
        private readonly ILogger _logger;
        private readonly ScriptContext _scriptContext;

        internal BuildContext(ScriptContext ctx, DirectoryInfo targetPath, ILogger logger)
        {
            // _scriptService = scriptService;
            var path = targetPath;
            _scriptContext = ctx;
            _workingDirectory = targetPath;
            _logger = logger;
        }

        public bool AddFolder(string relPath, DirectoryInfo sourceDir, string fileFilter = "*") {
            var targetPath = Path.Combine(_workingDirectory.FullName, relPath);
            sourceDir.CopyTo(targetPath, fileFilter);
            return Directory.Exists(targetPath);
        }

        public bool AddFile(string relPath, FileInfo sourceFile) {
            var targetPath = Path.Combine(_workingDirectory.FullName, relPath);
            var targetFilePath = Path.Combine(targetPath, sourceFile.Name);
            if (!Directory.Exists(targetPath)) {
                Directory.CreateDirectory(targetPath);
            }
            sourceFile.CopyTo(targetFilePath, true);
            return File.Exists(targetFilePath);
        }

        public bool AddFolder(string relPath, DirectoryInfo sourceDir, Func<FileInfo, bool> fileFilter) {
            var targetPath = Path.Combine(_workingDirectory.FullName, relPath);
            sourceDir.CopyTo(targetPath, fileFilter);
            return Directory.Exists(targetPath);
        }

        public bool LinkFolder(string relPath, DirectoryInfo sourceDir) {
            var linkPath = Path.Combine(_workingDirectory.FullName, relPath);
            Directory.CreateDirectory(Path.GetDirectoryName(linkPath));
            // var result = CreateSymbolicLink(linkPath, sourceDir.FullName, SymbolicLink.Directory);
            var result = CreateLink(new DirectoryInfo(linkPath), sourceDir.FullName);
            if (!result) {
                _logger.LogWarning("Couldn't link folders, most likely due to missing permissions! Falling back to copying files...");
                AddFolder(relPath, sourceDir);
            }
            // FileHelpers.CreateSymbolicLink(linkPath, sourceDir.FullName, SymbolicLink.Directory);
            return Directory.Exists(linkPath);
        }

        private bool CreateLink(DirectoryInfo linkPath, string targetName) {
            var cmdRunner = new CommandRunner("cmd.exe");
            cmdRunner.SetWorkingDirectory(Path.GetDirectoryName(linkPath.FullName));
            var result = cmdRunner.RunCommand(new[] { "/C mklink /D", linkPath.Name.ToArgument(), targetName.ToArgument()});
            return result.ExitCode == 0 && Directory.Exists(linkPath.FullName);
            // return Directory.Exists(linkName);
        }

        public (bool Success, FileInfo Output) RunBuild(CommandRunner runner, string targetFileName) {
            var args = new List<string> {_scriptContext.CurrentPath.ToArgument(), "pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var output = runner.SetWorkingDirectory(_scriptContext.WorkingDirectory).RunCommand(args);
            return (output.ExitCode == 0, new FileInfo(Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(_scriptContext.WorkingDirectory, targetFileName)));
        }

        public void Dispose() {
            _scriptContext.Dispose();
            _workingDirectory.Delete(true);
        }
    }

    public class AssetContext {
        public AssetContext(DirectoryInfo sourcePath, string targetOverride = null)
        {
            SourcePath = sourcePath;
            PackTargetOverride = targetOverride;
        }
        public DirectoryInfo SourcePath {get;set;}
        public string GetTargetPath(string rootPath, Func<string, string> pathOverride = null) {
            pathOverride ??= s => s;
            var relPath = Path.GetRelativePath(rootPath, SourcePath.FullName);
            relPath = pathOverride(relPath);
            if (!string.IsNullOrWhiteSpace(PackTargetOverride)) {
                relPath = (relPath == "." ? PackTargetOverride : Path.Combine(PackTargetOverride, relPath).Replace('\\', '/'));
            }
            return relPath;
        }
        public string SlotName => string.IsNullOrWhiteSpace(PackTargetOverride) ? SourcePath.Name : Path.GetFileName(PackTargetOverride);
        public string PackTargetOverride {get;set;} 

        public string FileFilter {get;set;} = "*";

        public string FilePattern {get;set;}

        // public bool HasNpc => SourcePath.Name.Any(char.IsLetter) && SourcePath.Name.Any(char.IsDigit);
        // public bool HasNpc => SlotName == "ex" || Regex.IsMatch(string.IsNullOrWhiteSpace(PackTargetOverride) ? SourcePath.Name : PackTargetOverride, @"\d{2}[a-z]{1}(_|$)");
        // public bool HasPlayer => Regex.IsMatch(string.IsNullOrWhiteSpace(PackTargetOverride) ? SourcePath.Name : PackTargetOverride, @"[^a-z](\d{2}|x{1}\d{1})(?![a-z]{1})");
    }
}