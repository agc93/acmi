using System;
using System.Linq;
using AceCore;
using AceCore.Parsers;
using BuildEngine;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackCreator.Build;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;
using Spectre.Console;
using UnPak.Core;
using UnPak.Core.Crypto;

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
                // c.AddCommand<InitCommand>("init");
                c.AddCommand<InstanceCommand>("instance");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return app;
        }
        
        internal static IServiceCollection GetServices() {
            var services = new ServiceCollection();
            // services.AddSingleton<ExecEngine.CommandRunner>(provider => new ExecEngine.CommandRunner("python") { Name = "python"});
            services.AddSingleton<AppInfoService>();
            
            services.AddBuildServices().AddUnPak().AddMessaging();
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

        internal static IServiceCollection AddUnPak(this IServiceCollection services) {
            return services.AddSingleton<IPakFormat, PakVersion3Format>()
                .AddSingleton<IPakFormat, PakVersion8Format>()
                .AddSingleton<IFooterLayout, DefaultFooterLayout>()
                .AddSingleton<IFooterLayout, PaddedFooterLayout>()
                .AddSingleton<IHashProvider, NativeHashProvider>()
                .AddSingleton<PakFileProvider>();
        }

        internal static IServiceCollection AddMessaging(this IServiceCollection services) {
            return services.AddMediatR(mc => mc.AsScoped(),
                typeof(Startup), typeof(IIdentifierParser));
        }

        internal static IServiceCollection AddBuildServices(this IServiceCollection services) {
            return services.AddSingleton<IBuildServiceProvider, BuildServiceProvider>()
                .AddSingleton<DirectoryBuildContextFactory>();
        }
    }
}