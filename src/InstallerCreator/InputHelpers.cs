using System.IO;
using InquirerCS;
using InstallerCreator.Commands;
using Semver;

namespace InstallerCreator
{
    public class InputHelpers
    {
        public static BuildCommand.Settings PromptMissing(BuildCommand.Settings settings, bool isUnattended = false) {
            if (!settings.Title.IsSet) {
                settings.Title.Value = Question.Input("Please enter your mod name")
                    .WithDefaultValue(new DirectoryInfo(settings.ModRootPath).Name)
                    .Prompt();
                settings.Title.IsSet = true;
            }
            if (!settings.Author.IsSet) {
                settings.Author.Value = Question.Input("Please enter your name")
                    .WithDefaultValue(System.Environment.UserName)
                    .Prompt();
                settings.Author.IsSet = true;
            }
            if (!settings.Version.IsSet) {
                settings.Version.Value = Question.Input("Please enter a valid version number")
                    .WithDefaultValue("1.0.0")
                    .WithValidation(input => SemVersion.TryParse(input, out var _), (input) => $"{input} is not a valid version number")
                    .WithConvertToString(input => SemVersion.Parse(input).ToString())
                    .Prompt();
                settings.Version.IsSet = true;
            }
            if (!settings.Description.IsSet && !isUnattended) {
                settings.Description.Value = Question.Input("Optionally enter a description")
                    .WithDefaultValue(string.Empty)
                    .Prompt();
                settings.Description.IsSet = !string.IsNullOrWhiteSpace(settings.Description.Value);
            }
            System.Console.WriteLine();
            return settings;
        }
    }
}