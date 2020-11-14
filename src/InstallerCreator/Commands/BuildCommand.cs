using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AceCore;
using InstallerCreator.ModInstaller;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Console;

namespace InstallerCreator.Commands
{
    [Description("Builds the Mod Installer XML files for the given mod files.")]
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        private readonly IOptionsPrompt<Settings> _prompt;
        private readonly IFileService _fileService;
        private readonly ILogger<BuildCommand> _logger;
        private readonly AppInfoService _infoService;
        private readonly ImageLocatorService _imageLocator;
        private readonly ArchiveService _archiveService;

        public BuildCommand(
            IOptionsPrompt<BuildCommand.Settings> prompt, 
            IFileService fileService, 
            ILogger<BuildCommand> logger, 
            AppInfoService infoService, 
            ImageLocatorService imageLocator,
            ArchiveService archiveService)
        {
            _prompt = prompt;
            _fileService = fileService;
            _logger = logger;
            _infoService = infoService;
            _imageLocator = imageLocator;
            _archiveService = archiveService;
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            var isUnattended = settings.Author.IsSet && settings.Version.IsSet && settings.Title.IsSet;
            var rootPath = new ModRootPath(settings.ModRootPath);
            settings.ModRootPath = rootPath.RootPath;
            var modRoot = rootPath.AbsolutePath;
            if (!Directory.Exists(modRoot)) {
                _logger.LogCritical("The specified mod root doesn't appear to exist!");
                return 1;
            }
            settings = _prompt.PromptMissing(settings, isUnattended);
            _infoService.Timer.Restart();
            _logger.LogInformation("Scanning and reading mod files. Please be patient!");
            if (settings.DetectMultipleSkins) {
                _logger.LogWarning("If your pak files contain more than one skin, we can scan the [bold]whole[/] file instead of just the headers.");
                _logger.LogWarning("However, this will take an [italic red]extremely[/] long time compared to just using the first texture we find.");
            }
            _logger.LogTrace($"{_infoService.GetCurrentTime()}: Starting GetFiles");
            var pack = _fileService.GetFiles(modRoot, settings.DetectMultipleSkins);
            // var skins = FileHelpers.GetSkins(modRoot, out var extraFiles);
            if (pack.IsEmpty()) {
                _logger.LogError("Could not locate any PAK files!");
                return 204;
            }
            _logger.LogInformation($"Building installer from [bold]{pack.GetFileCount()}[/] detected mods ([bold]{pack.ExtraFiles.Count}[/] other mod files).");
            // Console.WriteLine($"INFO: Building installer from {pack.Skins.Count} detected skin mods ({pack.ExtraFiles.Count} other mod files{(pack.MultiSkinFiles.Any() ? " and " + pack.MultiSkinFiles.Count + "merged files" : string.Empty)}).");
            _logger.LogTrace($"{_infoService.GetCurrentTime()}: Creating ModInstallerBuilder");
            var builder = new ModInstallerBuilder(modRoot, settings.Title.Value, settings.Description.IsSet ? settings.Description.Value : null, infoService: _infoService, imageLocator: _imageLocator);
            _logger.LogTrace($"{_infoService.GetCurrentTime()}: Generating info xml");
            var info = builder.GenerateInfoXml(settings.Author.Value, settings.Version.Value, settings.Groups, settings.Description.Value);
            _logger.LogTrace($"{_infoService.GetCurrentTime()}: Generating module config xml");
            var moduleConfig = builder.GenerateModuleConfigXml(pack);
            _logger.LogTrace($"{_infoService.GetCurrentTime()}: Module Config completed!");
            PrintSummaryTable(pack);
            _logger.LogDebug($"Generated {pack.GetFileCount()} files in {_infoService.GetCurrentTime()} seconds");
            builder.WriteToInstallerFiles(info, moduleConfig);
            _logger.LogInformation($"Mod Installer files have been written to the {Path.Combine(modRoot, "fomod")} directory!");
            if (!isUnattended) {
                var toZip = _prompt.Confirm("Do you want to immediately zip up your mod files?", true);
                if (toZip) {
                    var fn = _prompt.PromptFileName(
                        "What name should the zip file have?", 
                        defaultValue: settings.Title.IsSet ? settings.Title.Value.MakeSafe() : Path.GetDirectoryName(System.Environment.CurrentDirectory)
                    );
                    var op = _archiveService.MakeZip(rootPath, fn);
                    if (op.Success) {
                        _logger.LogInformation($"[bold green]Success![/] Your new archive file (ready for upload) has been created at {op.Result.FullName}");
                    } else {
                        _logger.LogWarning($"Failed to create ZIP file! [bold]Your files are still there![/] You just need to zip them up manually...");
                    }
                }
                Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                Console.ReadLine();
            }
            return 0;
        }

        private void PrintSummaryTable(SkinPack files) {
            var table = new Table();
            table.AddColumn(new TableColumn("[u]Type[/]"));
            table.AddColumn(new TableColumn("[u]Count[/]"));
            var summary = files.GetSummaryCount(true);
            foreach (var item in summary)
            {
                if (item.Value != 0) {
                    table.AddRow(item.Key, item.Value.ToString());
                }
            }
            AnsiConsole.Render(table);
        }
        public class Settings : AppSettings {
            [CommandOption("--author [VALUE]")]
            [Description("Name and/or username to use as the author in installer files. Only affects the installer.")]
            public FlagValue<string> Author {get;set;}

            [CommandOption("--title  [VALUE]")]
            [Description("Title for the generated installer. This is usually shown as the dialog title.")]
            public FlagValue<string> Title {get;set;}

            [CommandOption("--description [VALUE]")]
            [Description("An optional description to include in the installer files.")]
            public FlagValue<string> Description {get;set;}

            [CommandOption("--version [VERSION]")]
            [DefaultValue("1.0.0")]
            [Description("Version string to use for the generated info files. Only affects the installer.")]
            public FlagValue<string> Version {get;set;}

            [CommandOption("--group <VALUE>")]
            [Description("Optional list of named groups for this installer. Usually used as categories.")]
            public List<string> Groups {get;set;} = new List<string> {"Models and Textures"};

            [CommandOption("--multi")]
            [DefaultValue(false)]
            [Description("Include this if any of the files in your mod include skins for multiple aircraft/slots in the same `.pak` file. This will dramatically slow down detection, be patient!")]
            public bool DetectMultipleSkins {get;set;} = false;
        }
    }
}