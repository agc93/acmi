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
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var result = 0;
            if (File.Exists(settings.FolderPath) && Path.GetExtension(settings.FolderPath) is ".uasset" or ".uexp" && Path.GetFileNameWithoutExtension(settings.FolderPath) is {
            } folderPath && folderPath.Contains("Inst")) {
                //instance reader
                var app = GetApp();
                var args = new[] { "instance", settings.FolderPath}.Concat(context.Remaining.Raw);
                result = await app.RunAsync(args);
            } else {
                var di = new DirectoryInfo(settings.FolderPath);
                //time to pack boys
                var app = GetApp();
                var info = new AppInfoService();
                var args = new[] { "pack", settings.FolderPath}.Concat(context.Remaining.Raw);
                result = await app.RunAsync(args);
            // }
            // return await app.RunAsync(args);
            }
            if (result != 0) {
                Console.WriteLine("It looks like there might have been an error running the pack command!");
                Console.WriteLine("You can press <ENTER> to close, or copy/screenshot any errors you find above to help isolating any bugs.");
                Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                Console.ReadLine();
            }
            return result;
            
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "<folder-path>")]
            public string FolderPath {get;set;}
        }
    }
}