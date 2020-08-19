using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using InstallerCreator.ModInstaller;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        public override int Execute(CommandContext context, Settings settings)
        {
            settings = InputHelpers.PromptMissing(settings);
            var skins = FileHelpers.GetSkins(settings.ModRootPath, out var extraFiles);
            if (skins.Count == 0) {
                Console.WriteLine("Could not locate any PAK files");
                return 204;
            }
            var builder = new ModInstallerBuilder(settings.ModRootPath, settings.Title.Value, settings.Description.IsSet ? settings.Description.Value : null);
            var info = builder.GenerateInfoXml(settings.Author.Value, settings.Version.Value, settings.Groups, settings.Description.Value);
            
            var moduleConfig = builder.GenerateModuleConfigXml(skins, extraFiles);
            builder.WriteToInstallerFiles(info, moduleConfig);
            return 0;
        }

        public class Settings : AppSettings {
            [CommandOption("--author [VALUE]")]
            public FlagValue<string> Author {get;set;}

            [CommandOption("--title  [VALUE]")]
            public FlagValue<string> Title {get;set;}

            [CommandOption("--description [VALUE]")]
            public FlagValue<string> Description {get;set;}

            [CommandOption("--version [VERSION]")]
            [DefaultValue("1.0.0")]
            public FlagValue<string> Version {get;set;}

            [CommandOption("--group <VALUE>")]
            public List<string> Groups {get;set;} = new List<string> {"Models and Textures"};
        }
    }
}