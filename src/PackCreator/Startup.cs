using System;
using System.Linq;
using AceCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli.AppInfo;

namespace PackCreator
{
    public static class Startup
    {
        internal static IServiceCollection GetServices() {
            var services = new ServiceCollection();
            services.AddSingleton<ScriptDownloadService>();
            services.AddSingleton<PythonService>();
            services.AddSingleton<ExecEngine.CommandRunner>(provider => new ExecEngine.CommandRunner("python") { Name = "python"});
            services.AddSingleton<AppInfoService>();
            services.AddSingleton<BuildContextFactory>();
            services.AddSingleton<BuildService>(GetBuildService);
            services.AddSingleton<FileNameService>();
            services.AddSingleton<ParserService>();
            services.AddLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = GetLogLevel();
                });
            });
            services.Scan(scan =>
                scan.FromAssemblyOf<Identifier>()
                    .AddClasses(classes => classes.AssignableTo(typeof(AceCore.Parsers.IIdentifierParser))).AsImplementedInterfaces().WithSingletonLifetime()
            );
            return services;
        }

        private static BuildService GetBuildService(IServiceProvider provider) {
            var pyService = provider.GetRequiredService<PythonService>();
            var logger = provider.GetRequiredService<ILogger<BuildService>>();
            var factory = provider.GetRequiredService<BuildContextFactory>();
            var check = pyService.TestPathPython();
            var env = pyService.GetInstalledPythons().FirstOrDefault();
            var runner = new ExecEngine.CommandRunner(check ? "python" : env, "u4pak.py");
            return new BuildService(
                    logger,
                    factory,
                    runner
                );
        }

        internal static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("ACMI_DEBUG");
            if (System.IO.File.Exists(@"C:\acmi-debug.txt")) envVar = "trace";
            return string.IsNullOrWhiteSpace(envVar)
                ? LogLevel.Information
                :  envVar.ToLower() == "trace"
                    ? LogLevel.Trace
                    : LogLevel.Debug;
        }
    }
}