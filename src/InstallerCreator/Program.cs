using System;
using Spectre.Cli;
using Spectre.Console;
using Spectre.Cli.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using InstallerCreator.Commands;
using System.Threading.Tasks;
using AceCore.Parsers;
using AceCore;

namespace InstallerCreator
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsPrompt<BuildCommand.Settings>, SharpromptOptionsPrompt>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IIdentifierParser, PortraitParser>();
            services.AddSingleton<IIdentifierParser, CrosshairParser>();
            services.AddSingleton<IIdentifierParser, SkinParser>();
            services.AddSingleton<IIdentifierParser, WeaponParser>();
            services.AddSingleton<IIdentifierParser, EffectsParser>();
            services.AddSingleton<PakReader>();
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.SetDefaultCommand<BuildCommand>();
            app.Configure(c => {
                c.SetApplicationName("acmi");
                c.AddCommand<BuildCommand>("build");
                c.AddCommand<PackCommand>("zip");
                c.AddExample(new[] {"build"});
                c.AddExample(new[] {"build", "./ModPackFiles"});
                c.AddExample(new[] {"build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0"});
            });
            return await app.RunAsync(args);
        }
    }
}
