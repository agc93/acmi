using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AceCore;
using InstallerCreator.ModInstaller;
using Spectre.Cli;
using Spectre.Console;

namespace InstallerCreator.Commands
{
    [Description("Builds the Mod Installer XML files for the given mod files.")]
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        private readonly IOptionsPrompt<Settings> _prompt;
        private readonly IFileService _fileService;

        public BuildCommand(IOptionsPrompt<BuildCommand.Settings> prompt, IFileService fileService)
        {
            _prompt = prompt;
            _fileService = fileService;
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            var isUnattended = settings.Author.IsSet && settings.Version.IsSet && settings.Title.IsSet;
            var rootPath = new ModRootPath(settings.ModRootPath);
            settings.ModRootPath = rootPath.RootPath;
            var modRoot = rootPath.AbsolutePath;
            if (!Directory.Exists(modRoot)) {
                AnsiConsole.MarkupLine("[bold red]ERROR[/]: The specified mod root doesn't appear to exist!");
                return 1;
            }
            settings = _prompt.PromptMissing(settings, isUnattended);
            AnsiConsole.MarkupLine("[dim]INFO[/]: Scanning and reading mod files. Please be patient!");
            var pack = _fileService.GetFiles(modRoot, settings.DetectMultipleSkins.Value);
            // var skins = FileHelpers.GetSkins(modRoot, out var extraFiles);
            if (pack.IsEmpty()) {
                Console.WriteLine("Could not locate any PAK files");
                return 204;
            }
            AnsiConsole.MarkupLine($"[dim]INFO:[/] Building installer from [bold]{pack.GetFileCount()}[/] detected mods ([bold]{pack.ExtraFiles.Count}[/] other mod files).");
            // Console.WriteLine($"INFO: Building installer from {pack.Skins.Count} detected skin mods ({pack.ExtraFiles.Count} other mod files{(pack.MultiSkinFiles.Any() ? " and " + pack.MultiSkinFiles.Count + "merged files" : string.Empty)}).");
            var builder = new ModInstallerBuilder(modRoot, settings.Title.Value, settings.Description.IsSet ? settings.Description.Value : null);
            var info = builder.GenerateInfoXml(settings.Author.Value, settings.Version.Value, settings.Groups, settings.Description.Value);
            
            var moduleConfig = builder.GenerateModuleConfigXml(pack);
            PrintSummaryTable(pack);
            builder.WriteToInstallerFiles(info, moduleConfig);
            Console.WriteLine($"INFO: Mod Installer files have been written to the {Path.Combine(modRoot, "fomod")} directory!");
            if (!isUnattended) {
                Console.WriteLine("Press <ENTER> to continue...");
                Console.ReadLine();
            }
            return 0;
        }

        private void PrintSummaryTable(SkinPack files) {
            var table = new Table();
            table.AddColumn(new TableColumn("[u]Type[/]"));
            table.AddColumn(new TableColumn("[u]Count[/]"));
            if (files.Skins.Keys.Any()) {
                table.AddRow("Skins", files.Skins.Count.ToString());
            }
            if (files.MultiSkinFiles.Keys.Any()) {
                table.AddRow("Merged Skins", files.MultiSkinFiles.Count.ToString());
            }
            if (files.Portraits.Keys.Any()) {
                table.AddRow("Portraits", files.Portraits.Count.ToString());
            }
            if (files.Weapons.Keys.Any()) {
                table.AddRow("Weapons", files.Weapons.Count.ToString());
            }
            if (files.Effects.Keys.Any()) {
                table.AddRow("Effects", files.Effects.Count.ToString());
            }
            if (files.Crosshairs.Keys.Any()) {
                table.AddRow("Crosshairs", files.Crosshairs.Count.ToString());
            }
            if (files.ExtraFiles.Any()) {
                table.AddRow("Extra Files", files.ExtraFiles.Count.ToString());
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
            public FlagValue<bool> DetectMultipleSkins {get;set;}
        }
    }
}