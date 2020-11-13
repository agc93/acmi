using System;
using System.Collections.Generic;
using System.IO;
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
        bool Confirm(string prompt, bool? defaultValue = null);
        string PromptFileName(string prompt, string defaultValue = null, bool validate = true);
    }

    public class SharpromptOptionsPrompt : IOptionsPrompt<BuildCommand.Settings> {
        public static class FileValidators {
            internal static Func<object, ValidationResult> ValidFileName() => (obj) => {
                return string.IsNullOrWhiteSpace(obj.ToString()) || FilePathHasInvalidChars(obj.ToString())
                    ? new ValidationResult("Name contains invalid characters!")
                    : null;
            };

            private static bool FilePathHasInvalidChars(string path) {

                return (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0);
            }
        }

        private Func<object, Sharprompt.Validations.ValidationResult> SemVerValid = (raw) => {
            var valid = (string.IsNullOrWhiteSpace(raw as string)) || (raw is string input && SemVersion.TryParse(input, out var _));
            return valid ? null : new ValidationResult($"{raw.ToString()} is not a valid version number!");
        };

        public bool Confirm(string prompt, bool? defaultValue = null) {
            var confirm = Prompt.Confirm(prompt, defaultValue);
            return confirm;
        }

        public string PromptFileName(string prompt, string defaultValue = null, bool validate = true) {
            var response = Prompt.Input<string>(prompt, defaultValue: defaultValue, validators: validate ? new List<Func<object, ValidationResult>>() : new[] { FileValidators.ValidFileName()});
            return response;
        }

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
            /* if (!settings.DetectMultipleSkins.IsSet && !isUnattended) {
                AnsiConsole.MarkupLine("If your pak files contain more than one skin, we can scan the [bold]whole[/] file instead of just the headers.");
                AnsiConsole.MarkupLine("However, this will take an [italic red]extremely[/] long time compared to just using the first texture we find.");
                settings.DetectMultipleSkins.Value = Prompt.Confirm("Do any of your files contain more than one skin?", false);
                settings.DetectMultipleSkins.IsSet = true;
            } */
            if (!settings.Description.IsSet && !isUnattended) {
                settings.Description.Value = Prompt.Input<string>("Optionally enter a description", string.Empty);
                settings.Description.IsSet = !string.IsNullOrWhiteSpace(settings.Description.Value);
            }
            System.Console.WriteLine();
            return settings;
        }
    }
}