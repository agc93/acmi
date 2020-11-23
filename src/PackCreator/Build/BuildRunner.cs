using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExecEngine;

namespace PackCreator.Build
{
    public interface IBuildRunner {
        public (bool Success, FileInfo Output) RunBuild(BuildScript script, string targetFileName);
    }
    public class PythonBuildRunner : CommandRunner, IBuildRunner {
        // private readonly BuildScript _buildScript;
        public PythonBuildRunner(string pythonCmd) : base(pythonCmd, "u4pak.py") {
        }

        public (bool Success, FileInfo Output) RunBuild(BuildScript buildScript, string targetFileName) {
            var _buildScript = buildScript;
            // var args = new List<string> {_scriptContext.CurrentPath.ToArgument(), "pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var args = new List<string> {"pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var output = this.SetWorkingDirectory(_buildScript.WorkingDirectory).RunCommand(args);
            return (output.ExitCode == 0, new FileInfo(Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(_buildScript.WorkingDirectory, targetFileName)));
        }
    }

    public class UnrealBuildRunner : IBuildRunner {

        private CommandRunner _runner;
        public UnrealBuildRunner(string pathToUnrealPak)
        {
            this._runner = new CommandRunner(pathToUnrealPak);
        }

        public (bool Success, FileInfo Output) RunBuild(BuildScript buildScript, string targetFileName) {
            var _buildScript = buildScript;
            // var args = new List<string> {_scriptContext.CurrentPath.ToArgument(), "pack", targetFileName.ToArgument(), "Nimbus".ToArgument()};
            var args = new List<string> {targetFileName, "-create=filelist.txt", "-compress"}.ToArguments();
            CreateFileList(new DirectoryInfo(_buildScript.WorkingDirectory));
            this._runner.StartInfo.FileName = buildScript.CurrentPath;
            var output = this._runner.SetWorkingDirectory(_buildScript.WorkingDirectory).RunCommand(args);
            return (output.ExitCode == 0, new FileInfo(Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(_buildScript.WorkingDirectory, targetFileName)));
        }

        private FileInfo CreateFileList(DirectoryInfo directory) {
            var fileList = new FileInfo(Path.Combine(directory.FullName, "filelist.txt"));
            // var allFiles = directory.EnumerateFiles("*.u*", SearchOption.AllDirectories);
            var allNodes = directory.GetLeafNodes(true);
            // var relativePaths = allNodes.Select(a => $"{Path.Combine(Path.GetRelativePath(directory.FullName, a.FullName), "*.*").ToArgument()} {Path.Join(Path.GetRelativePath(a.FullName, directory.FullName), "*.*")}").ToList();
            // var relativePaths = allNodes.Select(a => $"\"{Path.Combine(Path.GetRelativePath(directory.FullName, a.FullName), "*.*")}\" \"..\\..\\*.*\"").ToList();
            var relativePaths = new[] { $"\"{Path.Combine(directory.FullName, "*.*")}\" \"..\\..\\..\\*.*\""};
            File.WriteAllLines(Path.Combine(directory.FullName, "filelist.txt"), relativePaths);
            return fileList;
        }
    }
}