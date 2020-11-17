using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExecEngine;
using PackCreator.Diagnostics;

namespace PackCreator
{
    public class PythonService
    {
        private readonly CommandRunner _runner;
        private readonly ScriptDownloadService _scriptService;

        public PythonService(CommandRunner runner, ScriptDownloadService scriptService)
        {
            _runner = runner;
            _scriptService = scriptService;
        }

        public bool TestPathPython() {
            try
            {
                var versionOut = _runner.RunCommand("--version");
                return (versionOut.ExitCode == 0 && versionOut.Output.Contains("Python"));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public bool IsPythonAvailable() {
            var onPath = TestPathPython();
            return onPath || GetInstalledPythons().Any();
        }

        public List<string> GetInstalledPythons() {
            var envs = LostTech.WhichPython.PythonEnvironment.EnumerateEnvironments().ToList();
            return envs
                .Where(e => e.InterpreterPath.FullName.Contains("Python3") || e.LanguageVersion?.Major == 3)
                .Select(e => e.InterpreterPath.FullName)
                .ToList();
        }

        public async Task<(bool Success, FileInfo result)> RunPackScript(string filesDirectory, string targetFileName, bool enforceDirName = true) {
            var di = new DirectoryInfo(filesDirectory) ;
            if (!di.Exists) {
                throw new InvalidDirectoryException("Source directory doesn't seem to exist!");
            }
            var sourcePath = di.Exists
                ? enforceDirName
                    ?  di.Name == "Nimbus"
                        ? di.Name
                        : Path.GetRelativePath(filesDirectory, di.GetDirectories().FirstOrDefault(d => d.Name == "Nimbus").FullName)
                    : di.Name
                : null;
            if (string.IsNullOrWhiteSpace(sourcePath)) {
                throw new InvalidDirectoryException("Could not locate Nimbus folder!");
            }
            using (var ctx = await _scriptService.GetScriptContext(di.Name == "Nimbus" ? Directory.GetParent(di.FullName).FullName : di.FullName))
            {
                var args = new List<string> {ctx.CurrentPath.ToArgument(), "pack", targetFileName.ToArgument(), sourcePath.ToArgument()};
                var output = _runner.SetWorkingDirectory(ctx.WorkingDirectory).RunCommand(args);
                return (output.ExitCode == 0, new FileInfo(Path.IsPathRooted(targetFileName) ? targetFileName : Path.Combine(ctx.WorkingDirectory, targetFileName)));
            }
        }
    }
}