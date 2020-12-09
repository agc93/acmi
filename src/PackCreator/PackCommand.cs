using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Cli;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using AceCore;
using ExecEngine;
using System.Collections.Generic;
using Sharprompt.Validations;
using AceCore.Parsers;
using System.Text.RegularExpressions;
using static PackCreator.PackingHelpers;
using Microsoft.Extensions.Logging;

namespace PackCreator {
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly ILogger<PackCommand> _logger;
        private readonly PythonService _pyService;
        private readonly BuildService _buildService;
        private readonly ParserService _parser;
        private readonly FileNameService _nameService;
        private readonly IAnsiConsole _console;

        public PackCommand(ILogger<PackCommand> logger, PythonService pyService, BuildService buildService, ParserService parser, FileNameService nameService, IAnsiConsole console)
        {
            _logger = logger;
            _pyService = pyService;
            _buildService = buildService;
            _parser = parser;
            _nameService = nameService;
            _console = console;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var rootInfo = new DirectoryInfo(settings.FileRootPath);
            var buildSettings = new BuildSettings();
            if (rootInfo.Name == "Nimbus") {
                rootInfo = rootInfo.Parent;
            }
            if (!rootInfo.Exists) {
                _logger.LogError("Specified directory does not exist!");
                return 404;
            }
            // _logger.LogInformation($"Starting packing for {rootInfo.Name}");
            var prefixName = Sharprompt.Prompt.Input<string>("Please enter a title/name for your new mod/pack", rootInfo.Name.MakeSafe(), validators: new[] { FileValidators.ValidFileName(true)});
            prefixName = prefixName.OrDefault(rootInfo.Name.MakeSafe()); //workaround for shibayan/Sharprompt#84
            buildSettings.Prefix = prefixName;
            var allFiles = rootInfo.EnumerateFiles("*.uasset", SearchOption.AllDirectories);
            // settings = InputHelpers.PromptMissing(settings, false);
            var pythonReady = _pyService.IsPythonAvailable();
            if (!pythonReady) {
                AnsiConsole.MarkupLine("[bold white on red]ERROR[/]: It looks like Python isn't installed on your PC. Install Python [bold]3[/] and try again!");
                var forceContinue = _console.Confirm("We can try and run the build anyway using UnrealPak, but this has not been as well-tested as Python/u4pak. Do you want to continue?");
                if (!forceContinue) {
                    return 3;
                }
            }
            var instructions = new List<BuildInstruction>();
            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace($"Running pack operation on {rootInfo.Name} with {allFiles.Count()} files.");
            }
            foreach (var file in allFiles)
            {
                var ident = _parser.ParseFilePath(file, rootInfo);
                if (ident == null) {
                    var relPath = Path.GetRelativePath(rootInfo.FullName, file.Directory.FullName);
                    _logger.LogTrace($"Adding {file.Name} as generic build instruction with target path: {relPath}");
                    var gInstr = new GenericInstruction() {
                        SourceGroup = file.Directory.Name,
                        TargetPath = relPath,
                        SourceFiles = file.Directory.GetFiles($"{Path.GetFileNameWithoutExtension(file.FullName)}.*").ToList()
                    };
                    instructions.Add(gInstr);
                    //follow earlier notes, this is an unknown
                } else if (ident is SkinIdentifier sIdent) {
                    _logger.LogTrace($"Detected {file.Name} as skin file: {sIdent.ToString()} for {sIdent.BaseObjectName}");
                    var instr = new SkinInstruction(sIdent) {
                        SourceFiles = file.Directory.GetFiles($"{sIdent.BaseObjectName}_{sIdent.Type}.*").ToList()
                    };
                    if (sIdent.Type == "MREC" || sIdent.Type == "N") {
                        continue;
                    } else if (sIdent.Type == "Inst") {
                        void AddRelativeSource(BuildInstruction<SkinIdentifier> targetInstr, SkinIdentifier ident) {
                            try
                            {
                                var rp = Path.GetRelativePath(instr.TargetPath, targetInstr.TargetPath);
                                var absPath = Path.GetFullPath(Path.Combine(file.Directory.FullName, rp));
                                if (rp != "." && Directory.Exists(absPath)) { // assume they're being added elsewhere for '.'
                                    targetInstr.SourceFiles.AddFiles(new DirectoryInfo(absPath), ident.RawValue + ".*");
                                }
                            }
                            catch (System.Exception)
                            {
                                _logger.LogDebug($"Error while searching for relative path: {instr.TargetPath}/{targetInstr.TargetPath}");
                            }
                        }
                        var iReader = new InstanceReader(_parser);
                        var mrecs = iReader.FindMREC(file.FullName).ToList();
                        if (mrecs.Any()) {
                            var mrec = mrecs.First();
                            _logger.LogTrace($"Detected MREC for {mrec.Identifier.BaseObjectName} at {mrec.Path}");
                            var mrecInstr = new SkinInstruction(mrec.Identifier);
                            mrecInstr.TargetPath = Identifier.BaseObjectPath + mrec.Path.TrimEnd('/');
                            mrecInstr.SourceFiles.AddFiles(file.Directory, mrec.Identifier.RawValue + ".*");
                            AddRelativeSource(mrecInstr, mrec.Identifier);
                            instructions.Add(mrecInstr);
                        }
                        var normals = iReader.FindNormal(file.FullName).ToList();
                        if (normals.Any()) {
                            var normal = normals.First();
                            _logger.LogTrace($"Detected Normals for {normal.Identifier.BaseObjectName} at {normal.Path}");
                            var normalInstr = new SkinInstruction(normal.Identifier);
                            normalInstr.TargetPath = Identifier.BaseObjectPath + normal.Path.TrimEnd('/');
                            normalInstr.SourceFiles.AddFiles(file.Directory, normal.Identifier.RawValue + ".*");
                            AddRelativeSource(normalInstr, normal.Identifier);
                            instructions.Add(normalInstr);
                        }
                        instructions.Add(instr);
                    } else if (sIdent.Type == "D") {
                        _logger.LogTrace($"Detected Diffuse for {sIdent.BaseObjectName} at {file.Name}");
                        var mrecPath = Path.Combine(file.Directory.FullName, $"{sIdent.BaseObjectName}_MREC.uasset");
                        var normPath = Path.Combine(file.Directory.FullName, $"{sIdent.BaseObjectName}_N.uasset");
                        var instPath = Path.Combine(file.Directory.FullName, $"{sIdent.BaseObjectName}_Inst.uasset");
                        if (File.Exists(mrecPath) && !File.Exists(instPath)) {
                            instr.SourceFiles.AddFiles(file.Directory, $"{sIdent.BaseObjectName}_MREC.*");
                        }
                        if (File.Exists(normPath) && !File.Exists(instPath)) {
                            instr.SourceFiles.AddFiles(file.Directory, $"{sIdent.BaseObjectName}_N.*");
                        }
                        instructions.Add(instr);
                    } else {
                        _logger.LogWarning("[bold white on red]Found an unmatched skin mod file![/] This shouldn't happen!");
                    }
                } else {
                    var instr = new BuildInstruction<Identifier>(ident) {
                        SourceFiles = file.Directory.GetFiles($"{ident.RawValue}.*").ToList()
                    };
                    instructions.Add(instr);
                }
            }
            _logger.LogDebug($"Grouping {instructions.Count} build instructions by source group");
            var groups = instructions.Where(i => i.SourceFiles.Any()).GroupBy(i => i.SourceGroup.GetName()).ToList();
            var packs = groups.ToDictionary(k => new SourceGroup(k.Key), v => v.ToList());
            _logger.LogDebug($"Preparing to build {packs.Count} packs");
            buildSettings.OutputMode = Sharprompt.Prompt.Select<OutputModes>("Choose how you'd like the PAK files to be output", valueSelector: e => e.GetEnumDescription());
            var subDir = string.Empty;
            if (buildSettings.OutputMode == OutputModes.SubDirectory) {
                subDir = Sharprompt.Prompt.Input<string>("What folder name should the output file be put in", "Output Files");
                subDir = Path.Combine(rootInfo.FullName, subDir);
                Directory.CreateDirectory(subDir);
            }
            if (packs.Count > 1) {
                // var mergeOptions = packs;
                // var mergeOptions = new Dictionary<SourceGroup, List<BuildInstruction>>();
                var mergeOptions = packs.ToDictionary(k => k.Key, v => v.Value);
                while (mergeOptions.Any())
                {
                    AnsiConsole.MarkupLine("[deepskyblue2]Would you like to combine any of your mod files together?[/]");
                    AnsiConsole.MarkupLine("Select any paths below that should be packed together, then press [bold grey]<ENTER>[/], or press [bold grey]<ENTER>[/] without selecting any to pack the listed files individually.");
                    while (mergeOptions.Any()) {
                        var candidates = Sharprompt.Prompt.MultiSelect<KeyValuePair<SourceGroup, List<BuildInstruction>>>("Choose the paths to include in the next PAK file", mergeOptions, minimum: 0, valueSelector: t => t.Key.GetName());
                        if (candidates.Any()) {
                            if (candidates.Count() == 1) {
                                var selection = candidates.First();
                                var name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", defaultValue: selection.Key.RawValue.MakeSafe(), validators: new[] { FileValidators.ValidFileName(true)});
                                name = name.OrDefault(selection.Key.RawValue).MakeSafe(true);
                                packs.Remove(selection.Key);
                                mergeOptions.Remove(selection.Key);
                                selection.Key.Name = name;
                                packs.Add(selection.Key, selection.Value);
                            } else {
                            // var commonRoot = candidates.Select(o => o.Key).FindCommonPath("/");
                                var commonRoot = candidates.Select(o => o.Value).SelectMany(o => o.Select(oi => oi.TargetPath)).FindCommonPath();
                                var selectedObjs = candidates;
                                var name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", defaultValue: Path.GetFileName(commonRoot), validators: new[] { FileValidators.ValidFileName(true)});
                                name = name.OrDefault(Path.GetFileName(commonRoot)).MakeSafe(true);
                                foreach (var cand in candidates)
                                {
                                    packs.Remove(cand.Key);
                                    mergeOptions.Remove(cand.Key);
                                }
                                // var getGroup = selectedObjs.GroupBy(x => x.Key).OrderBy(o => o.Count()).First().First().Key;
                                var rawGroup = Path.GetFileName(commonRoot);
                                packs.Add(new SourceGroup(rawGroup, name), selectedObjs.SelectMany(o => o.Value).ToList());
                            }
                        } else {
                            mergeOptions = new Dictionary<SourceGroup, List<BuildInstruction>>();
                        }
                    }
                }
            }
            var finalFiles = new List<string>();
            _logger.LogTrace($"Final build set ready for {packs.Count} packs");
            // buildSettings.UseSlotNames = Sharprompt.Prompt.Confirm("Do you want to use detailed slot names instead of slot numbers?", false);
            foreach (var pakBuild in packs)
            {
                var ctxName = pakBuild.Key;
                _logger.LogDebug($"Running asset build for {ctxName}");
                // buildRoot.Key.TargetAssets.AddRange(metaObjects);
                var bResult = await _buildService.RunBuild(ctxName, rootInfo.FullName, pakBuild.Value.ToArray());
                if (bResult == null) {
                    AnsiConsole.MarkupLine($"[bold white on red]ERROR[/]: Failed to build the pak file for {ctxName}! This can mean a lot of things, including an incorrect folder structure or failed build.");
                    //well shit
                } else {
                    
                    var finalName = $"{_nameService.GetNameFromBuildGroup(pakBuild, buildSettings)}_P.pak";
                    string NestedFolders(string finalName) {
                        var targetDir = Path.Combine(rootInfo.Parent.FullName, "Packed Files", _nameService.GetOutputPathForGroup(pakBuild.Key));
                        Directory.CreateDirectory(targetDir);
                        return targetDir;
                    }
                    var finalTarget = buildSettings.OutputMode switch {
                        OutputModes.SubDirectory => subDir,
                        OutputModes.NestedFolders => NestedFolders(finalName),
                        _ => rootInfo.FullName
                    };
                    var finalFile = Path.Join(finalTarget, finalName);
                    File.Copy(bResult.FullName, finalFile, true);
                    finalFiles.Add(Path.GetRelativePath(rootInfo.FullName, finalFile));
                }
            }
            AnsiConsole.MarkupLine($"[bold underline]Complete![/] {finalFiles.Count} PAK files were built:");
            foreach (var generatedFile in finalFiles)
            {
                AnsiConsole.MarkupLine($"- {generatedFile}");
            }
            Console.WriteLine();
            AnsiConsole.MarkupLine("Make any changes you want to your PAK files and remember to run your mod files through [green]acmi.exe[/] [bold]before[/] uploading!");
            Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
            Console.ReadLine();
            return 0;
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "[fileRoot]")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandOption("--target [fileName]")]
            [Description("The target pak file to generate.")]
            public FlagValue<string> TargetFilePath {get;set;}
        }
    }
}