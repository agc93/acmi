using System;
using System.Linq;
using AceCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackCreator.Build;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;
using Spectre.Console;

namespace PackCreator
{
    public static class Startup
    {
        internal static CommandApp GetApp() {
            var app = new CommandApp(new DependencyInjectionRegistrar(GetServices()));
            // app.SetDefaultCommand<PackCommand>();
            app.Configure(c => {
                c.PropagateExceptions();
                c.SetApplicationName("acmi-pack");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<InitCommand>("init");
                c.AddCommand<InstanceCommand>("instance");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return app;
        }
        
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
                        Ansi = Spectre.Console.AnsiSupport.Detect,
                        ColorSystem = ColorSystemSupport.Detect
                });
            });
            services.AddLogging(logging => {
                var level = GetLogLevel();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = level;
                });
                AddFileLogging(logging, level);
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

        internal static ILoggingBuilder AddFileLogging(ILoggingBuilder logging, LogLevel level) {
            var options = new NReco.Logging.File.FileLoggerOptions {
                Append = true,
                FileSizeLimitBytes = 4096,
                MaxRollingFiles = 5
            };
            if (level < LogLevel.Information) {
                logging.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, NReco.Logging.File.FileLoggerProvider>(
                    (srvPrv) => {
                        return new NReco.Logging.File.FileLoggerProvider("acmi.log", options) { MinLevel = level };
                    }
                ));
            }
            return logging;
        }

        internal static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("ACMI_DEBUG");
            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "acmi-debug.txt"))) envVar = "trace";
            if (System.IO.File.Exists(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName, "acmi-debug.txt"))) envVar = "trace";
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
            if (check || envs.Any()) {
            // if (false) {
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