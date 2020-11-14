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

        public bool TestPython() {
            var versionOut = _runner.RunCommand("--version");
            return (versionOut.ExitCode == 0 && versionOut.Output.Contains("Python"));
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