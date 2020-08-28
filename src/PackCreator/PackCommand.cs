using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Cli;

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
            var result = await _pyService.RunPackScript(settings.FileRootPath, settings.TargetFilePath.Value, !settings.AllowNonNimbus);
            return 0;
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