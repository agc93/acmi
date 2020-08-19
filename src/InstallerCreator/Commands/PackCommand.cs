using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    public class PackCommand : Command<PackCommand.Settings>
    {
        public override int Execute(CommandContext context, Settings settings) {
            var targetFile = string.IsNullOrWhiteSpace(settings.TargetFileName)
                ? new DirectoryInfo(settings.ModRootPath).Name
                : settings.TargetFileName;
            targetFile = Path.GetFileNameWithoutExtension(targetFile) + (settings.Extension.IsSet ? settings.Extension.Value : ".zip");
            var absoluteTarget = Path.Combine(settings.ModRootPath, targetFile);
            ZipFile.CreateFromDirectory(settings.ModRootPath, absoluteTarget);
            Console.WriteLine($"Created archive file at ${absoluteTarget}");
            return File.Exists(absoluteTarget) ? 0 : 500;
        }

        public class Settings : AppSettings {
            [CommandOption("-e|--extension [value]")]
            [DefaultValue(".zip")]
            public FlagValue<string> Extension {get;set;}

            [CommandArgument(1, "[archiveFileName]")]
            public string TargetFileName {get;set;}

        }
    }
}