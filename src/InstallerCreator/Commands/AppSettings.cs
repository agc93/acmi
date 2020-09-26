using System.ComponentModel;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    public class AppSettings : CommandSettings
    {
        [CommandArgument(0, "<fileRoot>")]
        [Description("Directory containing your mod files and/or folders. PAK files will be automatically located.")]
        public string ModRootPath {get;set;} = System.Environment.CurrentDirectory;
        
        [CommandOption("-v|--verbose")]
        public bool Verbose {get;set;}
    }
}