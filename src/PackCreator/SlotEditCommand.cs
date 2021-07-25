using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AceCore;
using AceCore.Assets;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Console;

namespace PackCreator
{
    public class SlotEditCommand : Command<SlotEditCommand.Settings>
    {
        private readonly IAnsiConsole _console;
        private readonly ParserService _parserService;
        private readonly ILogger<SlotEditCommand> _logger;

        public SlotEditCommand(IAnsiConsole console, ParserService parserService, ILogger<SlotEditCommand> logger) {
            _console = console;
            _parserService = parserService;
            _logger = logger;
        }
        
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[filePath]")]
            [Description("Path to an cooked texture file to read from")]
            public string[] FileRootPath {get;set;}
        }

        public override int Execute(CommandContext context, Settings settings) {
            var modifiedFiles = new List<FileInfo>();
            var assetFiles = settings.FileRootPath.Select(f => new FileInfo(f))
                .Where(f => f is {Exists: true, Extension: ".uasset"});
            var parsedAssets = assetFiles.ToDictionary(fi => fi, fi => _parserService.ParseFilePath(fi))
                .Where(id => id.Value is SkinIdentifier {} skinIdentifier && skinIdentifier.Type != "Inst").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as SkinIdentifier);
            var groupedAssets = parsedAssets.GroupBy(p => p.Value.BaseObjectName);
            foreach (var group in groupedAssets) {
                _console.MarkupLine($"The '{group.Key}' files have been cooked for the '{group.Key.Split('_').Last()}' slot.");
                var result = _console.Prompt(
                    new TextPrompt<string>(
                        "Please enter the slots you would like to recook it for, separated by commas"));
                var slots = result.Split(new[] {',', ';'}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => char.IsDigit(s[0]));
                foreach (var (fi, ident) in group) {
                    var editor = new SlotEditor(fi, _logger, _parserService);
                    foreach (var slot in slots) {
                        var newFile = editor.RecookToSlot(ident, slot);
                        modifiedFiles.Add(newFile);
                    }
                }
            }
            AnsiConsole.MarkupLine($"[bold underline]Complete![/] {modifiedFiles.Count} new files were created:");
            foreach (var generatedFile in modifiedFiles)
            {
                AnsiConsole.MarkupLine($"- {generatedFile.Name}");
            }
            Console.WriteLine();
            Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
            Console.ReadLine();
            return 0;
        }
    }
}