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

        public PythonService(CommandRunner runner)
        {
            _runner = runner;
        }

        public PythonService()
        {
            _runner = new CommandRunner("python");
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
    }
}