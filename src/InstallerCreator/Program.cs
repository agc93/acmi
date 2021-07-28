using System;
using Spectre.Cli;
using Spectre.Cli.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InstallerCreator.Commands;
using System.Threading.Tasks;
using AceCore.Parsers;
using AceCore;
using UnPak.Core;

namespace InstallerCreator {
    class Program {
        private static bool IsDebugEnabled() {
            var envVar = System.Environment.GetEnvironmentVariable("ACMI_DEBUG");
            return !string.IsNullOrWhiteSpace(envVar) && envVar.ToLower() == "true";
        }
        private static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("ACMI_DEBUG");
            return string.IsNullOrWhiteSpace(envVar) 
                ? LogLevel.Information
                :  envVar.ToLower() == "trace"
                    ? LogLevel.Trace
                    : LogLevel.Debug;
        }
        static async Task<int> Main(string[] args) {
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsPrompt<BuildCommand.Settings>, SharpromptOptionsPrompt>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IIdentifierParser, PortraitParser>();
            services.AddSingleton<IIdentifierParser, CrosshairParser>();
            services.AddSingleton<IIdentifierParser, SkinParser>();
            services.AddSingleton<IIdentifierParser, WeaponParser>();
            services.AddSingleton<IIdentifierParser, EffectsParser>();
            services.AddSingleton<IIdentifierParser, CanopyParser>();
            services.AddSingleton<IIdentifierParser, EmblemParser>();
            services.AddSingleton<IIdentifierParser, CockpitParser>();
            services.AddSingleton<ArchiveService>();
            services.AddUnPak();
            services.AddSingleton<AppInfoService>();
            services.AddSingleton<ParserService>();
            services.AddSingleton<ModInstaller.ImageLocatorService>();
            services.AddLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = GetLogLevel();
                });
            });
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.SetDefaultCommand<BuildCommand>();
            app.Configure(c => {
                c.SetApplicationName("acmi");
                c.AddCommand<BuildCommand>("build");
                c.AddCommand<PackCommand>("zip");
                c.AddCommand<InfoCommand>("info");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return await app.RunAsync(args);
        }
    }

    internal static class ServiceExtensions
    {
        internal static IServiceCollection AddUnPak(this IServiceCollection services) {
            return services.AddSingleton<IPakFormat, PakVersion3Format>()
                .AddSingleton<IPakFormat, PakVersion8Format>()
                .AddSingleton<IFooterLayout, DefaultFooterLayout>()
                .AddSingleton<IFooterLayout, PaddedFooterLayout>()
                .AddSingleton<UnPak.Core.Crypto.IHashProvider, UnPak.Core.Crypto.NativeHashProvider>()
                .AddSingleton<PakFileProvider>()
                .AddSingleton<PakFileReader>();
        }
    }
}
