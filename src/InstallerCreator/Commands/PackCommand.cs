using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using AceCore;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    [Description("Creates a ZIP archive of the given directory. This is just a convenience.")]
    public class PackCommand : Command<PackCommand.Settings>
    {
        private readonly ArchiveService _archiver;

        public PackCommand(ArchiveService archiver)
        {
            _archiver = archiver;
        }
        public override int Execute(CommandContext context, Settings settings) {
            var rootPath = new ModRootPath(settings.ModRootPath);
            var targetFile = string.IsNullOrWhiteSpace(settings.TargetFileName)
                ? new DirectoryInfo(settings.ModRootPath).Name
                : settings.TargetFileName;
            var op = _archiver.MakeZip(rootPath, targetFile, settings.Extension.IsSet ? settings.Extension.Value : null);
            return op.Success ? 0 : 500;
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