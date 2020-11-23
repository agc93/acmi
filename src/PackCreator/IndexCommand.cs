using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using AceCore.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;
using static PackCreator.Startup;

namespace PackCreator
{
    public class IndexCommand : AsyncCommand<IndexCommand.Settings>
    {
        private CommandApp GetApp() {
            var app = new CommandApp(new DependencyInjectionRegistrar(GetServices()));
            // app.SetDefaultCommand<PackCommand>();
            app.Configure(c => {
                c.PropagateExceptions();
                c.SetApplicationName("acmi-pack");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<InitCommand>("init");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return app;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var di = new DirectoryInfo(settings.FolderPath);
            /* if (di.Name != "Nimbus" && di.GetDirectories().Length == 0 && di.GetFiles().Length == 0) {
                //time to init
                var app = GetApp();
                var args = new[] {"init", settings.FolderPath}.Concat(context.Remaining.Raw);
                return await app.RunAsync(args);
            } else { */
                //time to pack boys
                var app = GetApp();
                var info = new AppInfoService();
                var args = new[] { "pack", settings.FolderPath}.Concat(context.Remaining.Raw);
                var result = await app.RunAsync(args);
                if (result != 0) {
                    Console.WriteLine("It looks like there might have been an error running the pack command!");
                    Console.WriteLine("You can press <ENTER> to close, or copy/screenshot any errors you find above to help isolating any bugs.");
                    Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                    Console.ReadLine();
                }
                return result;
            // }
            // return await app.RunAsync(args);
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "<folder-path>")]
            public string FolderPath {get;set;}
        }
    }
}