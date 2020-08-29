using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Cli;
using Spectre.Console;
using System;

namespace PackCreator
{
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly PythonService _pyService;

        public PackCommand(PythonService pyService)
        {
            _pyService = pyService;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            settings = InputHelpers.PromptMissing(settings, false);
            try
            {
                var check =  _pyService.TestPython();
                if (!check) {
                    AnsiConsole.MarkupLine("[bold white on red]ERROR[/]: It looks like Python isn't installed on your PC. Install Python [bold]3[/] and try again!");
                    return 3;
                }
            }
            catch (System.Exception ex)
            {
                AnsiConsole.MarkupLine("[bold white on red]ERROR[/]: It looks like Python isn't installed on your PC. Install Python [bold]3[/] and try again!");
                AnsiConsole.MarkupLine(ex.Message);
                return 3;
            }
            var result = await _pyService.RunPackScript(settings.FileRootPath, settings.TargetFilePath.Value, !settings.AllowNonNimbus);
            if (result.Success) {
                AnsiConsole.MarkupLine($"[bold green]Success[/], it looks like [italic]u4pak[/] has successfully packed your mod files!");
                AnsiConsole.MarkupLine($"You should find your new mod file at [dim grey]{result.result.FullName}[/]");
            } else {
                AnsiConsole.MarkupLine("[orange3]It looks like something has gone wrong![/]");
                AnsiConsole.MarkupLine("We called u4pak, but it looks like it failed for some reason. Unfortunately, there's no way to tell exactly what went wrong 😢");
            }
            Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
            Console.ReadLine();
            return result.Success ? 0 : 1;
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "[fileRoot]")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandOption("--target [fileName]")]
            [Description("The target pak file to generate.")]
            public FlagValue<string> TargetFilePath {get;set;}

            [CommandOption("--allow-any")]
            [Description("Allows any folder to be packed, not just 'Nimbus'")]
            public bool AllowNonNimbus {get;set;} = false;
        }
    }
}