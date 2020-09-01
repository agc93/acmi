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

namespace PackCreator
{
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly PythonService _pyService;
        private readonly BuildContextFactory _contextFactory;
        private readonly CommandRunner _runner;

        public PackCommand(PythonService pyService, BuildContextFactory contextFactory, ExecEngine.CommandRunner runner)
        {
            _pyService = pyService;
            _contextFactory = contextFactory;
            _runner = runner;
        }

        private string GetFriendlyName(KeyValuePair<string, List<AssetContext>> obj) {
            return GetFriendlyName(Path.GetFileName(obj.Key));
                // return Constants.AllVehicleNames.TryGetValue(Path.GetFileName(obj.Key), out var fn) ? fn : obj.Key;
            }
            private string GetFriendlyName(string objName) {
                return Constants.AllVehicleNames.TryGetValue(objName, out var fn) ? fn : objName;
            }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var rootInfo = new DirectoryInfo(settings.FileRootPath);
            var prefix = Sharprompt.Prompt.Input<string>("Please enter a title/name for your new mod/pack", rootInfo.Name, validators: new[] { FileValidators.ValidFileName()});
            // var vehicles = rootInfo.GetLeafNodes().ToList();
            var vehicles = rootInfo.GetModFileNodes("Nimbus").ToList();
            var looseObjects = rootInfo.EnumerateFiles("*.uexp", SearchOption.TopDirectoryOnly);
            var availObjects = vehicles.ToDictionary(k => Path.GetRelativePath(settings.FileRootPath, k.FullName), v => (v.GetDirectories().Any() ? v.GetDirectories().Where(d => d.Name.IsSlotFolder()).Select(d => new AssetContext(d)) : new[] { new AssetContext(v)}).ToList());
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
            var roots = new Dictionary<PackTarget, IEnumerable<AssetContext>>();
            if (looseObjects.ToList().Any()) {
                AssetContext GetContext(AceCore.Vehicles.AircraftSlot ac, SkinIdentifier skin) {
                    return new AssetContext(rootInfo, ac.PathRoot + ac.ObjectName + "/" + skin.Slot) {FilePattern = $"{skin.Aircraft}_[0x]{skin.Slot.TrimStart('0')}_"};
                }
                var looseNames = looseObjects.Select(o => Path.GetFileNameWithoutExtension(o.FullName)).Distinct().ToList();
                var looseSkins = looseNames.Select(ln => new SkinParser().TryParse(ln)).Where(pr => pr.IsValid).Select(pr => pr.identifier).Cast<SkinIdentifier>().ToList();
                foreach (var skin in looseSkins)
                {
                    var files = rootInfo.GetFiles($"{skin.Aircraft}_*{skin.Slot.TrimStart('0')}_*").ToList();
                    if (files.Any() && files.Count > 1 && files.Any(f => f.Extension.EndsWith("uexp"))) {
                        if (files.Any(f => f.Name.Contains("_MREC") && skin.Slot.Any(char.IsLetter))) {
                            AnsiConsole.MarkupLine("[red]Loose NPC MRECs found![/] NPC Skins using custom MRECs [bold]must[/] be in their proper object directory!");
                            continue;
                        }
                        if (Constants.AllVehicleNames.Keys.Contains(skin.Aircraft)) {
                            // *now* we know where it goes!
                            var ac = Constants.AllVehicles.GetAircraft().FirstOrDefault(a => a.ObjectName == skin.Aircraft);
                            if (availObjects.Keys.Contains(ac.ObjectName)) {
                                availObjects[ac.ObjectName].Add(GetContext(ac, skin));
                            } else {
                                availObjects.Add(ac.ObjectName, new List<AssetContext> {GetContext(ac, skin)});
                            }
                        }
                        //valid loose files
                    }
                    
                }
            }
            var bulkNpcs = false;
            foreach (var obj in availObjects)
            {
                AnsiConsole.WriteLine($"Indexing mod files for {GetFriendlyName(obj)}");
                var ac = Constants.AllVehicles.GetAircraft().FirstOrDefault(a => a.ObjectName == Path.GetFileName(obj.Key));
                var vc = Constants.Vessels.FirstOrDefault(v => v.ObjectName == Path.GetFileName(obj.Key));
                if (ac != null && Constants.NonPlayableAircraft.Any(a => a.ObjectName == ac.ObjectName)) {
                    //default to bulk packing these
                    foreach (var mrecObj in obj.Value.Where(f => f.SourcePath.GetFiles("*_Inst.uasset").Any()))
                    {
                        if (mrecObj.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")) {
                            var instFile = mrecObj.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).First().Name;
                            var mrecDir = mrecObj.SourcePath.Parent.GetDirectories().First(d => d.Name.ToLower() == "ex");
                            var regex = new Regex(@"_(\d{2}a?)_");
                            if (regex.IsMatch(instFile)) {
                                var slotId = regex.Match(instFile).Groups[1].Value;
                                obj.Value.Add(new AssetContext(mrecDir) { FilePattern = $"_[0x]{slotId.TrimStart('0')}_MREC"});
                            }
                            
                            // var mrecDir = new DirectoryInfo(Path.Combine(mrecParent.FullName, "ex"));
                            
                        }
                    }
                    if (obj.Value.Any(f => f.SourcePath.GetFiles("*_Inst.uasset").Any() && obj.Value.Any(f => f.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")))) {
                        //hello again my old friend custom MRECs
                        
                        
                        
                    }
                    var name = $"{ac.Name.MakeSafe(true)}_NPC";
                    roots.Add(new PackTarget(name, ac.PathRoot + ac.ObjectName) { IsMerged = obj.Value.Count() > 1}, obj.Value);
                } else if (ac != null && Constants.PlayerAircraft.Any(a => a.ObjectName == ac.ObjectName)) {
                    var areNpcsAvailable = obj.Value.Any(a => a.HasNpc);
                    if (areNpcsAvailable) {
                        var extraAssets = new List<AssetContext>();
                        foreach (var exContext in obj.Value.Where(d => d.SourcePath.Parent.GetDirectories().Any(d => d.Name == "ex")))
                        {
                            //the current context *could* have files in ex/
                            var regex = new Regex(@"_(\d{2}a?)_");
                            var instFile = exContext.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).FirstOrDefault()?.Name;
                            if (instFile != null && regex.IsMatch(instFile)) {
                                var slotId = regex.Match(instFile).Groups[1].Value;
                                extraAssets.Add(new AssetContext(exContext.SourcePath.Parent.GetDirectories().First(d => d.Name == "ex")) { FilePattern = $"_[0x]?{slotId.TrimStart('0')}_"});
                            }

                        }
                        obj.Value.AddRange(extraAssets);

                        void PackSeparate() {
                            foreach (var slot in obj.Value.Where(a => a.HasNpc))
                            {
                                roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_{slot.SlotName}", ac), new[] {slot});
                            }
                        }
                        void PackMerged() {
                            roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_NPC", ac) { IsMerged = true }, obj.Value.Where(a => a.HasNpc));
                        }
                        if (obj.Value.Where(a => a.HasNpc).Count() > 1) { 
                            var response = bulkNpcs ? NPCPackMode.Global : Sharprompt.Prompt.Select<NPCPackMode>($"Should we pack all {GetFriendlyName(obj)} NPC slots together?", valueSelector: e => e.GetEnumDescription());
                            switch (response)
                            {
                                case NPCPackMode.None:
                                    PackSeparate();
                                    break;
                                case NPCPackMode.PerAircraft:
                                    PackMerged();
                                    break;
                                case NPCPackMode.Global:
                                    bulkNpcs = true;
                                    PackMerged();
                                    break;
                                default:
                                    break;
                            }
                        } else {
                            
                        }
                    }
                    //detected player skin
                    foreach (var skinDir in obj.Value.Where(d => d.HasPlayer)) {
                        var skinId = skinDir.SlotName;
                        roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_{skinId}", ac, skinId), new[] {skinDir});
                    }
                } else if (vc != null) {
                    //default to bulk packing these
                    var name = $"{vc.GetName().MakeSafe(true)}";
                    roots.Add(new PackTarget(name, vc) { IsMerged = true}, obj.Value);
                } else {
                    unhandledObjects.Add(obj.Key, obj.Value);
                }
                //TODO: handle a fallback
            }
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
                        roots.Add(new PackTarget(name, commonRoot), selectedObjs.SelectMany(o => o.Value));
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
                subDir = Path.Combine(settings.FileRootPath, subDir);
                Directory.CreateDirectory(subDir);
            }
            if (roots.Count(r => r.Key.IsMerged) > 1) {
                var mergedTargets = roots.Where(r => r.Key.IsMerged).ToList();
                while (mergedTargets.Any())
                {
                    AnsiConsole.MarkupLine("[deepskyblue2]Would you like to combine any of your merged packs together?[/]");
                    AnsiConsole.MarkupLine("Select any paths below that should be packed together, then press [bold grey]<ENTER>[/], or press [bold grey]<ENTER>[/] without selecting any to pack them individually.");
                    while (mergedTargets.Any() && mergedTargets.Where(r => !string.IsNullOrWhiteSpace(r.Key?.TargetFileName)).Any()) {
                        var candidates = Sharprompt.Prompt.MultiSelect<KeyValuePair<PackTarget, IEnumerable<AssetContext>>>("Choose the paths to include in the next PAK file", mergedTargets, minimum: 0, valueSelector: t => t.Key?.TargetFileName);
                        if (candidates.Any()) {
                            var commonRoot = candidates.Select(o => o.Key.ObjectPath).FindCommonPath("/");
                            var selectedObjs = candidates;
                            var name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", defaultValue: Path.GetFileName(commonRoot), validators: new[] { FileValidators.ValidFileName()});
                            roots.Add(new PackTarget(name, commonRoot), selectedObjs.SelectMany(o => o.Value));
                            foreach (var cand in candidates)
                            {
                                roots.Remove(cand.Key);
                                mergedTargets.Remove(cand);
                            }
                        } else {
                            mergedTargets = new List<KeyValuePair<PackTarget, IEnumerable<AssetContext>>>();
                        }
                    }
                }
            }
            var finalFiles = new List<string>();
            foreach (var buildRoot in roots)
            {
                var ctxName = Path.GetFileNameWithoutExtension(buildRoot.Key.TargetFileName);
                // buildRoot.Key.TargetAssets.AddRange(metaObjects);
                var bResult = await RunBuild(ctxName, settings.FileRootPath, buildRoot.Value.Concat(metaObjects).ToArray());
                if (bResult == null) {
                    //well shit
                } else {
                    var finalName = $"{prefix}_{buildRoot.Key.TargetFileName}{(buildRoot.Key.IsMerged ? "_MULTI" : string.Empty)}_P.pak";
                    string NestedFolders(string finalName) {
                        var targetDir = Path.Combine(settings.FileRootPath, "Packed Files", buildRoot.Key.GetOutputPath());
                        /* if (!string.IsNullOrWhiteSpace(buildRoot.Key.ObjectPath)) {
                            targetDir = Path.Combine(targetDir, new DirectoryInfo(buildRoot.Key.ObjectPath).Parent.Name, Path.GetFileName(buildRoot.Key.ObjectPath));
                        } */
                        Directory.CreateDirectory(targetDir);
                        return targetDir;
                    }
                    var finalTarget = output switch {
                        OutputModes.SubDirectory => subDir,
                        OutputModes.NestedFolders => NestedFolders(finalName),
                        _ => settings.FileRootPath
                    };
                    var finalFile = Path.Join(finalTarget, finalName);
                    File.Copy(bResult.FullName, finalFile, true);
                    finalFiles.Add(Path.GetRelativePath(settings.FileRootPath, finalFile));
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

        private async Task<FileInfo> RunBuild(string objName, string rootPath, params AssetContext[] contextTargets) {
            var targets = contextTargets.ToList();
            using (var ctx = await _contextFactory.Create(objName))
            {
                foreach (var target in targets)
                {
                    /* var relPath = Path.GetRelativePath(rootPath, target.SourcePath.FullName);
                    if (!string.IsNullOrWhiteSpace(target.PackTargetOverride)) {
                        relPath = (relPath == "." ? target.PackTargetOverride : Path.Combine(target.PackTargetOverride, relPath).Replace('\\', '/')).Replace("/_meta/", "");
                    } */
                    var relPath = target.GetTargetPath(rootPath, s => s.Replace("_meta\\", ""));
                    var linked = string.IsNullOrWhiteSpace(target.FilePattern)
                        ? ctx.AddFolder(relPath, target.SourcePath, target.FileFilter)
                        : ctx.AddFolder(relPath, target.SourcePath, fi => System.Text.RegularExpressions.Regex.IsMatch(fi.Name, target.FilePattern ?? ".*"));
                    if (!linked) {
                        AnsiConsole.MarkupLine("[bold red]Failed to add folders to context directory![/]");
                        // Console.ReadLine();
                        return null;
                    }
                }
                var buildResult = ctx.RunBuild(_runner, "packed-files.pak");
                if (buildResult.Success) {
                    AnsiConsole.MarkupLine($"[bold green]Success![/] Files for {GetFriendlyName(objName)} successfully packed from {targets.Count} folders");
                    var tempFile = Path.GetTempFileName();
                    buildResult.Output.CopyTo(tempFile, true);
                    return new FileInfo(tempFile);
                } else {
                    AnsiConsole.MarkupLine($"[bold white on red]Failed![/] Files from {Directory.GetParent(contextTargets.First().SourcePath.FullName)} not packed successfully. Continuing...");
                    return null;
                }
            }
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

    public enum OutputModes {
        [Description("Directly output to the mods folder")]
        None,
        [Description("A separate folder in the mods folder")]
        SubDirectory,
        [Description("Auto-organized into a complete set of folders")]
        NestedFolders
    }

    public enum NPCPackMode {
        [Description("Separately")]
        None,
        [Description("For this aircraft")]
        PerAircraft,
        [Description("For all aircraft")]
        Global
    }
}