using System.ComponentModel;
using Spectre.Cli;

namespace InstallerCreator.Commands
{
    public class AppSettings : CommandSettings
    {
        [CommandArgument(0, "[fileRoot]")]
        public string ModRootPath {get;set;} = System.Environment.CurrentDirectory;
        
        [CommandOption("-v|--verbose")]
        public bool Verbose {get;set;}
    }
}