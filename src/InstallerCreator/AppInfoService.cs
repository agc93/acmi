using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace InstallerCreator
{
    public class AppInfoService
    {
        private readonly Assembly _assembly;
        private readonly Version _version;
        public Stopwatch Timer {get;} = new Stopwatch();

        public AppInfoService()
        {
            _assembly = Assembly.GetExecutingAssembly();
            _version = _assembly.GetName().Version;
            Timer?.Start();
        }
        public string GetAppVersion(string prefix = "v") {
            var version = $"{prefix}{_version.Major}.{_version.Minor}.{_version.Build}";
            return version;
        }

        public string GetAppPath() =>  AppContext.BaseDirectory;

        public string GetRuntimeName() {
            return _assembly?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown";
        }

        public string OperatingSystemName => System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        public string GetCurrentTime() => Math.Round(Timer.Elapsed.TotalSeconds, 3).ToString();
    }
}