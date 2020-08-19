using System;
using Spectre.Cli;
using Spectre.Console;
using Spectre.Cli.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using InstallerCreator.Commands;
using System.Threading.Tasks;

namespace InstallerCreator
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.Configure(c => {
                c.SetApplicationName("AC7 Mod Installer Creator");
                c.AddCommand<BuildCommand>("build");
                c.AddCommand<PackCommand>("zip");
            });
            return await app.RunAsync(args);
        }
    }
}
