using System;
using System.IO;
using InquirerCS;
using InstallerCreator.Commands;
using Semver;
using Sharprompt;
using Sharprompt.Validations;
using Spectre.Cli;
using Spectre.Console;
using ValidationResult = Sharprompt.Validations.ValidationResult;

namespace InstallerCreator
{

    public interface IOptionsPrompt<T> where T : CommandSettings {
        T PromptMissing(T settings, bool isUnattended = false);
    }

    public class SharpromptOptionsPrompt : IOptionsPrompt<BuildCommand.Settings> {
        private Func<object, Sharprompt.Validations.ValidationResult> SemVerValid = (raw) => {
            var valid = (string.IsNullOrWhiteSpace(raw as string)) || (raw is string input && SemVersion.TryParse(input, out var _));
            return valid ? null : new ValidationResult($"{raw.ToString()} is not a valid version number!");
        };

        public BuildCommand.Settings PromptMissing(BuildCommand.Settings settings, bool isUnattended = false) {
            if (!settings.Title.IsSet) {
                settings.Title.Value = Prompt.Input<string>("Please enter your mod name", new DirectoryInfo(settings.ModRootPath).Name);
                settings.Title.IsSet = true;
            }
            if (!settings.Author.IsSet) {
                settings.Author.Value = Prompt.Input<string>("Please enter your name", System.Environment.UserName);
                settings.Author.IsSet = true;
            }
            if (!settings.Version.IsSet) {
                var versionInput = Prompt.Input<string>("Please enter a valid version number", "1.0.0", new[] {SemVerValid});
                settings.Version.Value = SemVersion.Parse(versionInput).ToString();
                settings.Version.IsSet = true;
            }
            if (!settings.DetectMultipleSkins.IsSet && !isUnattended) {
                AnsiConsole.MarkupLine("If your pak files contain more than one skin, we can scan the [bold]whole[/] file instead of just the headers.");
                AnsiConsole.MarkupLine("However, this will take an [italic red]extremely[/] long time compared to just using the first texture we find.");
                settings.DetectMultipleSkins.Value = Prompt.Confirm("Do any of your files contain more than one skin?", false);
                settings.DetectMultipleSkins.IsSet = true;
            }
            if (!settings.Description.IsSet && !isUnattended) {
                settings.Description.Value = Prompt.Input<string>("Optionally enter a description", string.Empty);
                settings.Description.IsSet = !string.IsNullOrWhiteSpace(settings.Description.Value);
            }
            System.Console.WriteLine();
            return settings;
        }
    }


    public class InquirerPrompts : IOptionsPrompt<BuildCommand.Settings>
    {
        public BuildCommand.Settings PromptMissing(BuildCommand.Settings settings, bool isUnattended = false) {
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