using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    [Description("Creates a ZIP archive of the given directory. This is just a convenience.")]
    public class PackCommand : Command<PackCommand.Settings>
    {
        public override int Execute(CommandContext context, Settings settings) {
            var rootPath = new ModRootPath(settings.ModRootPath);
            var targetFile = string.IsNullOrWhiteSpace(settings.TargetFileName)
                ? new DirectoryInfo(settings.ModRootPath).Name
                : settings.TargetFileName;
            var tempFile = Path.Combine(Path.GetTempPath(), targetFile);
            targetFile = Path.GetFileNameWithoutExtension(targetFile) + (settings.Extension.IsSet ? settings.Extension.Value : ".zip");
            var absoluteTarget = Path.Combine(rootPath.AbsolutePath, targetFile);
            ZipFile.CreateFromDirectory(settings.ModRootPath, tempFile);
            File.Move(tempFile, absoluteTarget);
            Console.WriteLine($"Created archive file at ${absoluteTarget}");
            return File.Exists(absoluteTarget) ? 0 : 500;
        }

        public class Settings : AppSettings {
            [CommandOption("-e|--extension [value]")]
            [DefaultValue(".zip")]
            [Description("Optionally choose a non-default file extension.")]
            public FlagValue<string> Extension {get;set;}

            [CommandArgument(1, "[archiveFileName]")]
            [Description("The target file for the ZIP archive. Defaults to a file named for (and in) the mod root directory.")]
            public string TargetFileName {get;set;}

        }
    }
}