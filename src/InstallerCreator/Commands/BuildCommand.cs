using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using InstallerCreator.ModInstaller;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    [Description("Builds the Mod Installer XML files for the given mod files.")]
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        public override int Execute(CommandContext context, Settings settings)
        {
            var isUnattended = settings.Author.IsSet && settings.Version.IsSet && settings.Title.IsSet;
            var rootPath = new ModRootPath(settings.ModRootPath);
            settings.ModRootPath = rootPath.RootPath;
            var modRoot = rootPath.AbsolutePath;
            if (!Directory.Exists(modRoot)) {
                Console.WriteLine("ERROR: The specified mod root doesn't appear to exist!");
                return 1;
            }
            settings = InputHelpers.PromptMissing(settings, isUnattended);
            var skins = FileHelpers.GetSkins(modRoot, out var extraFiles);
            if (skins.Count == 0) {
                Console.WriteLine("Could not locate any PAK files");
                return 204;
            }
            Console.WriteLine();
            Console.WriteLine($"INFO: Building installer from {skins.Count} detected skin mods ({extraFiles.Count} other mod files).");
            var builder = new ModInstallerBuilder(modRoot, settings.Title.Value, settings.Description.IsSet ? settings.Description.Value : null);
            var info = builder.GenerateInfoXml(settings.Author.Value, settings.Version.Value, settings.Groups, settings.Description.Value);
            
            var moduleConfig = builder.GenerateModuleConfigXml(skins, extraFiles);
            builder.WriteToInstallerFiles(info, moduleConfig);
            Console.WriteLine($"INFO: Mod Installer files have been written to the {Path.Combine(modRoot, "fomod")} directory!");
            if (!isUnattended) {
                Console.WriteLine("Press <ENTER> to continue...");
                Console.ReadLine();
            }
            return 0;
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
        }
    }
}