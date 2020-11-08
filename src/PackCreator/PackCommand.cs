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

namespace PackCreator {
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly PythonService _pyService;
        private readonly BuildService _buildService;
        private readonly IEnumerable<IIdentifierParser> _parsers;
        private readonly FileNameService _nameService;

        public PackCommand(PythonService pyService, BuildService buildService, IEnumerable<IIdentifierParser> parser, FileNameService nameService)
        {
            _pyService = pyService;
            _buildService = buildService;
            _parsers = parser;
            _nameService = nameService;
        }

        private string GetFriendlyName(KeyValuePair<string, List<AssetContext>> obj) {
            return GetFriendlyName(Path.GetFileName(obj.Key));
                // return Constants.AllVehicleNames.TryGetValue(Path.GetFileName(obj.Key), out var fn) ? fn : obj.Key;
            }
            private string GetFriendlyName(string objName) {
                return Constants.AllVehicleNames.TryGetValue(objName, out var fn) ? fn : objName;
            }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            Identifier ParseMatch(string rawString) {
                var matched = _parsers.Select(p => p.TryParse(rawString, false)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    return matched.identifier;
                }
                return null;
            }
            var rootInfo = new DirectoryInfo(settings.FileRootPath);
            if (rootInfo.Name == "Nimbus") {
                rootInfo = rootInfo.Parent;
            }
            var prefix = Sharprompt.Prompt.Input<string>("Please enter a title/name for your new mod/pack", rootInfo.Name, validators: new[] { FileValidators.ValidFileName()});
            var allFiles = rootInfo.EnumerateFiles("*.uasset", SearchOption.AllDirectories);
            // var vehicles = rootInfo.GetModFileNodes("Nimbus").ToList();
            var metaObjects = rootInfo.GetModFileNodes("_meta").Select(fn => new AssetContext(fn) { PackTargetOverride = "Nimbus/Content"} ).ToList();
            var unhandledObjects = new Dictionary<string, List<AssetContext>>();
            // settings = InputHelpers.PromptMissing(settings, false);
            var pythonReady = CheckPython();
            if (!pythonReady) {
                AnsiConsole.MarkupLine("[bold white on red]ERROR[/]: It looks like Python isn't installed on your PC. Install Python [bold]3[/] and try again!");
                Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                Console.ReadLine();
                return 3;
            }
            var roots = new Dictionary<PackTarget, List<AssetContext>>();
            var instructions = new List<BuildInstruction>();
            foreach (var file in allFiles)
            {
                var ident = ParseMatch(file.Name);
                if (ident == null) {
                    var gInstr = new GenericInstruction() {
                        SourceGroup = file.Directory.Name,
                        TargetPath = Path.GetRelativePath(rootInfo.FullName, file.Directory.FullName),
                        SourceFiles = file.Directory.GetFiles($"{Path.GetFileNameWithoutExtension(file.FullName)}.*").ToList()
                    };
                    instructions.Add(gInstr);
                    //follow earlier notes, this is an unknown
                } else if (ident is SkinIdentifier sIdent) {
                    var instr = new BuildInstruction<SkinIdentifier>(sIdent) {
                        SourceFiles = file.Directory.GetFiles($"{sIdent.BaseObjectName}_{sIdent.Type}.*").ToList()
                    };
                    if (sIdent.Type == "MREC") {
                        continue;
                    } else if (sIdent.Type == "Inst") {
                        var iReader = new InstanceReader(_parsers);
                        var mrecs = iReader.FindMREC(file.FullName).ToList();
                        if (mrecs.Any()) {
                            var mrec = mrecs.First();
                            var mrecInstr = new BuildInstruction<SkinIdentifier>(mrec.Identifier);
                            mrecInstr.TargetPath = Identifier.BaseObjectPath + mrec.Path.TrimEnd('/');
                            // mrecInstr.SourceFiles.AddFiles(file.Directory, Path.GetFileName(mrec.Path) + ".*");
                            mrecInstr.SourceFiles.AddFiles(file.Directory, mrec.Identifier.BaseObjectName + "_MREC.*");
                            instructions.Add(mrecInstr);
                            // if (File.Exists(Path.GetFileName()))
                        }
                        instructions.Add(instr);
                    } else {
                        var mrecPath = Path.Combine(file.Directory.FullName, $"{sIdent.BaseObjectName}_MREC.uasset");
                        var instPath = Path.Combine(file.Directory.FullName, $"{sIdent.BaseObjectName}_Inst.uasset");
                        if (File.Exists(mrecPath) && !File.Exists(instPath)) {
                            instr.SourceFiles.AddFiles(file.Directory, $"{sIdent.BaseObjectName}_MREC.*");
                        }
                        instructions.Add(instr);
                    }
                }
            }
            var groups = instructions.GroupBy(i => i.SourceGroup).ToList();
            Console.WriteLine(groups.Count());
            var packs = groups.ToDictionary(k => k.Key, v => v.ToList());
            if (unhandledObjects.Any()) {
                AnsiConsole.MarkupLine("[orange3]We found some objects that weren't automatically detected[/]. We can automatically pack these into a single PAK file, or multiple");
                // AnsiConsole.MarkupLine("Select any paths below that should be packed [underline]together[/], then press [bold grey]<ENTER>[/].");
                AnsiConsole.MarkupLine("If you want to the remaining files to be packed [underline]separately[/], press [bold grey]<ENTER>[/] without selecting any options");
                var fileIndex = 1;
                while (unhandledObjects.Any()) {
                    var candidates = Sharprompt.Prompt.MultiSelect<string>("Select any paths below that should be packed together[/], then press <ENTER>", unhandledObjects.Keys, minimum: 0);
                    if (candidates.Any()) {
                        var selectedObjs = candidates.ToDictionary(c => c, c => unhandledObjects[c]);
                        var commonRoot = selectedObjs.Select(o => o.Key).FindCommonPath("\\");
                        var commonDi = new DirectoryInfo(commonRoot);
                        var name = string.Empty;
                        if (commonDi.Name == "Content" || commonDi.Parent.Name == "Content") {
                            name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", validators: new[] { Validators.Required(), FileValidators.ValidFileName()});
                        } else {
                            name = $"{commonDi.Parent.Name}_{commonDi.Name}_{fileIndex}";
                        }
                        fileIndex++;
                        roots.Add(new PackTarget(name, commonRoot), selectedObjs.SelectMany(o => o.Value).ToList());
                        foreach (var cand in candidates)
                        {
                            unhandledObjects.Remove(cand);
                        }
                    } else {
                        foreach (var unhandled in unhandledObjects)
                        {
                            var objName = Path.GetFileName(unhandled.Key);
                            var name = Constants.AllItemNames.TryGetValue(objName, out var _name)
                                ? _name
                                : objName;
                            roots.Add(new PackTarget(name, unhandled.Key), unhandled.Value);
                        }
                        break;
                    }
                }
            }
            OutputModes output = Sharprompt.Prompt.Select<OutputModes>("Choose how you'd like the PAK files to be output", valueSelector: e => e.GetEnumDescription());
            var subDir = string.Empty;
            if (output == OutputModes.SubDirectory) {
                subDir = Sharprompt.Prompt.Input<string>("What folder name should the output file be put in", "Output Files");
                subDir = Path.Combine(rootInfo.FullName, subDir);
                Directory.CreateDirectory(subDir);
            }
            if (packs.Count > 1) {
                var mergeOptions = packs;
                while (mergeOptions.Any())
                {
                    AnsiConsole.MarkupLine("[deepskyblue2]Would you like to combine any of your mod files together?[/]");
                    AnsiConsole.MarkupLine("Select any paths below that should be packed together, then press [bold grey]<ENTER>[/], or press [bold grey]<ENTER>[/] without selecting any to pack them individually.");
                    while (mergeOptions.Any()) {
                        var candidates = Sharprompt.Prompt.MultiSelect<KeyValuePair<string, List<BuildInstruction>>>("Choose the paths to include in the next PAK file", mergeOptions, minimum: 0, valueSelector: t => t.Key);
                        if (candidates.Any()) {
                            // var commonRoot = candidates.Select(o => o.Key).FindCommonPath("/");
                            var commonRoot = candidates.Select(o => o.Value).SelectMany(o => o.Select(oi => oi.TargetPath)).FindCommonPath();
                            var selectedObjs = candidates;
                            var name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", defaultValue: Path.GetFileName(commonRoot), validators: new[] { FileValidators.ValidFileName()});
                            packs.Add(name, selectedObjs.SelectMany(o => o.Value).ToList());
                            // roots.Add(new PackTarget(name, commonRoot), selectedObjs.SelectMany(o => o.Value).ToList());
                            foreach (var cand in candidates)
                            {
                                packs.Remove(cand.Key);
                                mergeOptions.Remove(cand.Key);
                            }
                        } else {
                            mergeOptions = new Dictionary<string, List<BuildInstruction>>();
                        }
                    }
                }
            }
            var finalFiles = new List<string>();
            foreach (var pakBuild in packs)
            {
                var ctxName = pakBuild.Key;
                // buildRoot.Key.TargetAssets.AddRange(metaObjects);
                var bResult = await _buildService.RunBuild(ctxName, rootInfo.FullName, pakBuild.Value.ToArray());
                if (bResult == null) {
                    AnsiConsole.MarkupLine($"[bold white on red]ERROR[/]: Failed to build the pak file for {ctxName}! This can mean a lot of things, including an incorrect folder structure or failed build.");
                    //well shit
                } else {
                    var finalName = $"{_nameService.GetNameFromGroup(pakBuild.Key, prefix)}_P.pak";
                    string NestedFolders(string finalName) {
                        var targetDir = Path.Combine(rootInfo.FullName, "Packed Files", _nameService.GetOutputPathForGroup(pakBuild.Key));
                        Directory.CreateDirectory(targetDir);
                        return targetDir;
                    }
                    var finalTarget = output switch {
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
            Console.WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
            Console.ReadLine();
            return 0;
        }

        private bool CheckPython() {
            try
            {
                var check =  _pyService.TestPython();
                if (!check) {
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                AnsiConsole.MarkupLine(ex.Message);
                return false;
            }
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "[fileRoot]")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandOption("--target [fileName]")]
            [Description("The target pak file to generate.")]
            public FlagValue<string> TargetFilePath {get;set;}

            [CommandOption("--allow-any")]
            [Description("Allows any folder to be packed, not just 'Nimbus'")]
            public bool AllowNonNimbus {get;set;} = false;
        }
    }
}