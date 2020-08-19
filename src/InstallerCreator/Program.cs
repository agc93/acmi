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
            app.SetDefaultCommand<BuildCommand>();
            app.Configure(c => {
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
