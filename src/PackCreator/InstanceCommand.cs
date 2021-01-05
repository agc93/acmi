using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Console;

namespace PackCreator
{
    public class InstanceCommand : Command<InstanceCommand.Settings>
    {
        private readonly ILogger<InstanceCommand> _logger;
        private readonly IAnsiConsole _console;
        private readonly ParserService _parser;

        public InstanceCommand(ILogger<InstanceCommand> logger, ParserService parser, IAnsiConsole console)
        {
            _logger = logger;
            _console = console;
            _parser = parser;
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            var iReader = new InstanceReader(_parser);
            var fi = GetInstanceFile(settings);
            if (fi == null) {
                return 4;
            }
            _logger.LogInformation($"Attempting to read MRECs and Normals from {fi.Name}...");
            WriteSeparator();
            var mrecs = iReader.FindMREC(fi.FullName).ToList();
            foreach (var mrec in mrecs) {
                _logger.LogInformation($"MREC path found for {mrec.Identifier.BaseObjectName}: {mrec.Identifier.RawValue}");
                _logger.LogInformation($"Target MREC packing path: {Identifier.BaseObjectPath + mrec.Path.TrimEnd('/')}/{mrec.Identifier.RawValue}.*");
                WriteSeparator();
            }
            var normals = iReader.FindNormal(fi.FullName).ToList();
            foreach (var (path, identifier) in normals) {
                _logger.LogInformation($"Normals path found for {identifier.BaseObjectName}: {identifier.RawValue}");
                _logger.LogInformation($"Target Normals packing path: {Identifier.BaseObjectPath + path.TrimEnd('/')}/{identifier.RawValue}.*");
                WriteSeparator();
            }
            if (mrecs.Any() && normals.Any()) {
                WriteSummary();
                _console.WriteLine();
                _console.WriteLine();
                _console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                System.Console.ReadLine();
                return 0;
            } else {
                return 1;
            }
        }

        private void WriteSummary() {
            _console.MarkupLine("The paths above are read directly from the Instance file you opened");
            _console.MarkupLine("If you're packing with [deepskyblue2]ACMI[/], make sure you name your MRECs/Normals correctly " +
                "and they will be automatically packed when using this instance file");
        }

        private void WriteSeparator() {
            _console.MarkupLine("[dim grey italic]================[/]");
        }

        private FileInfo GetInstanceFile(Settings settings) {
            if (File.Exists(settings.FileRootPath) && Path.GetExtension(settings.FileRootPath) == ".uasset") {
                return new FileInfo(settings.FileRootPath);
            } else if (File.Exists(settings.FileRootPath) && Path.GetExtension(settings.FileRootPath) == ".uexp") {
                var uexp = new FileInfo(settings.FileRootPath);
                return new FileInfo(Path.Join(uexp.Directory.FullName, $"{Path.GetFileNameWithoutExtension(uexp.FullName)}.uasset"));
            }
            return null;
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "[filePath]")]
            [Description("Path to an instance file to read from")]
            public string FileRootPath {get;set;}
        }
    }
}