using System;
using System.Linq;
using AceCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackCreator.Build;
using Spectre.Cli.AppInfo;
using Spectre.Console;

namespace PackCreator
{
    public static class Startup
    {
        internal static IServiceCollection GetServices() {
            var services = new ServiceCollection();
            services.AddSingleton<IScriptDownloadService, PythonScriptDownloadService>();
            services.AddSingleton<PythonService>();
            // services.AddSingleton<ExecEngine.CommandRunner>(provider => new ExecEngine.CommandRunner("python") { Name = "python"});
            services.AddSingleton<AppInfoService>();
            services.AddSingleton<BuildContextFactory>();
            services.AddBuildServices();
            // services.AddSingleton<BuildService>(GetBuildService);
            services.AddSingleton<FileNameService>();
            services.AddSingleton<ParserService>();
            services.AddSingleton<IAnsiConsole>(p => {
                return AnsiConsole.Create(
                    new AnsiConsoleSettings {
                        Ansi = AnsiSupport.Detect,
                        ColorSystem = ColorSystemSupport.Detect
                });
            });
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
            var envs = pyService.GetInstalledPythons();
            IBuildRunner runner = null;
            if (check || envs.Any()) {
                runner = new PythonBuildRunner(check ? "python" : envs.First());
            } else {
                logger.LogWarning("Python not detected! Attempting to fall back to UnrealPak.exe!");
                runner = new UnrealBuildRunner("UnrealPak.exe");
            }
            // var runner = new ExecEngine.CommandRunner(check ? "python" : env, "u4pak.py");
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

        internal static IServiceCollection AddBuildServices(this IServiceCollection services) {
            var pyService = new PythonService();
            var check = pyService.TestPathPython();
            var envs = pyService.GetInstalledPythons();
            // if (check || envs.Any()) {
            if (false) {
                services.AddSingleton<IBuildRunner>(provider => new PythonBuildRunner(check ? "python" : envs.First()));
                services.AddSingleton<IScriptDownloadService, PythonScriptDownloadService>();
                //python is available, use u4pak
            } else {
                services.AddSingleton<IBuildRunner>(provider => {
                    var uLogger = provider.GetRequiredService<ILogger<UnrealBuildRunner>>();
                    uLogger.LogWarning("Python not detected! Attempting to fall back to UnrealPak.exe!");
                    return new UnrealBuildRunner("UnrealPak.exe");
                });
                services.AddSingleton<IScriptDownloadService, UnrealPakDownloadService>();
            }
            services.AddSingleton<BuildService>();
            return services;
        }
    }
}