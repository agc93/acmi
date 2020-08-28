using System.IO;
using Sharprompt;
using Sharprompt.Validations;

namespace PackCreator
{
    public static class InputHelpers
    {
        public static PackCommand.Settings PromptMissing(PackCommand.Settings settings, bool isUnattended = false) {
            if (!settings.TargetFilePath.IsSet) {
                settings.TargetFilePath.Value = Prompt.Input<string>("Please enter a name (or path) for the output file", validators: new[] {Validators.Required()});
                settings.TargetFilePath.IsSet = true;
            }
            /* if (!settings.DetectMultipleSkins.IsSet && !isUnattended) {
                AnsiConsole.MarkupLine("If your pak files contain more than one skin, we can scan the [bold]whole[/] file instead of just the headers.");
                AnsiConsole.MarkupLine("However, this will take an [italic red]extremely[/] long time compared to just using the first texture we find.");
                settings.DetectMultipleSkins.Value = Prompt.Confirm("Do any of your files contain more than one skin?", false);
                settings.DetectMultipleSkins.IsSet = true;
            } */
            // if (!settings.Description.IsSet && !isUnattended) {
            //     settings.Description.Value = Prompt.Input<string>("Optionally enter a description", string.Empty);
            //     settings.Description.IsSet = !string.IsNullOrWhiteSpace(settings.Description.Value);
            // }
            System.Console.WriteLine();
            return settings;
        }
    }
}