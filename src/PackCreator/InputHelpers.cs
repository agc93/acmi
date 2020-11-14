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
            System.Console.WriteLine();
            return settings;
        }
    }
}