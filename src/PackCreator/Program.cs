using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;

namespace PackCreator
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var app = new CommandApp(new DependencyInjectionRegistrar(Startup.GetServices()));
            app.SetDefaultCommand<IndexCommand>();
            app.Configure(c => {
                c.SetApplicationName("acmi-pack");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<InfoCommand>("info");
                /* c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" }); */
            });
            return await app.RunAsync(args);
        }
    }
}
