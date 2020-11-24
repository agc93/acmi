using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AceCore;
using ExecEngine;
using Microsoft.Extensions.Logging;

namespace PackCreator {
    public class BuildContext : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
        private readonly DirectoryInfo _workingDirectory;
        private readonly ILogger _logger;
        public readonly BuildScript BuildScript;

        internal BuildContext(BuildScript ctx, DirectoryInfo targetPath, ILogger logger)
        {
            var path = targetPath;
            _workingDirectory = targetPath;
            _logger = logger;
            BuildScript = ctx;
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
        }

        /* public (bool Success, FileInfo Output) RunBuild(CommandRunner runner, string targetFileName) {
            // var args = new List<string> {_scriptContext.CurrentPath.ToArgument(), "pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var args = new List<string> {"pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var output = runner.SetWorkingDirectory(_scriptContext.WorkingDirectory).RunCommand(args);
            return (output.ExitCode == 0, new FileInfo(Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(_scriptContext.WorkingDirectory, targetFileName)));
        } */

        public void Dispose() {
            BuildScript.Dispose();
            _workingDirectory.Delete(true);
        }
    }
}