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

        public PackCommand(PythonService pyService, BuildService buildService, IEnumerable<IIdentifierParser> parser)
        {
            _pyService = pyService;
            _buildService = buildService;
            _parsers = parser;
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
            if (rootInfo.Name == "Nimbus") {
                rootInfo = rootInfo.Parent;
            }
            var prefix = Sharprompt.Prompt.Input<string>("Please enter a title/name for your new mod/pack", rootInfo.Name, validators: new[] { FileValidators.ValidFileName()});
            // var vehicles = rootInfo.GetLeafNodes().ToList();
            var vehicles = rootInfo.GetModFileNodes("Nimbus").ToList();
            var looseObjects = rootInfo.EnumerateFiles("*.uasset", SearchOption.TopDirectoryOnly);
            var availObjects = vehicles.ToDictionary(k => Path.GetRelativePath(rootInfo.FullName, k.FullName), v => (v.GetDirectories().Any() ? v.GetDirectories().Where(d => d.Name.IsSlotFolder()).Select(d => new AssetContext(d)) : new[] { new AssetContext(v)}).ToList());
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
            if (looseObjects.ToList().Any()) {
                AssetContext GetContext(AceCore.Vehicles.AircraftSlot ac, SkinIdentifier skin) {
                    return new AssetContext(rootInfo, ac.PathRoot + ac.ObjectName + "/" + skin.Slot) {FilePattern = $"{skin.Aircraft}_[0x]{skin.Slot.TrimStart('0')}_"};
                }
                AssetContext GetSkinContext(SkinIdentifier skin) {
                    return new AssetContext(rootInfo, skin.ObjectPath) {FilePattern = $"{skin.Aircraft}_[0x]{skin.Slot.TrimStart('0')}_"};
                }
                var looseNames = looseObjects.Select(o => Path.GetFileName(o.FullName)).Distinct().ToList();
                var looseSkins = looseNames.Select(ln => new SkinParser().TryParse(ln, true)).Where(pr => pr.IsValid).Select(pr => pr.identifier).Cast<SkinIdentifier>().ToList();
                foreach (var skin in looseSkins)
                {
                    var files = rootInfo.GetFiles($"{skin.Aircraft}_*{skin.Slot.TrimStart('0')}_*").ToList();
                    if (files.Any() && files.Count > 1 && files.Any(f => f.Extension.EndsWith("uexp"))) {
                        /* if (files.Any(f => f.Name.Contains("_MREC") && skin.Slot.Any(char.IsLetter))) {
                            AnsiConsole.MarkupLine("[red]Loose NPC MRECs found![/] NPC Skins using custom MRECs [bold]must[/] be in their proper object directory!");
                            continue;
                        }
                        var acObjName = skin.GetObjectName();
                        if (availObjects.Keys.Contains(acObjName)) {
                                availObjects[acObjName].Add(GetSkinContext(skin));
                            } else {
                                availObjects.Add(acObjName, new List<AssetContext> {GetSkinContext(skin)});
                            } */
                        /* if (Constants.AllVehicleNames.Keys.Contains(skin.Aircraft)) {
                            // *now* we know where it goes!
                            var ac = Constants.AllVehicles.GetAircraft().FirstOrDefault(a => a.ObjectName == skin.Aircraft);
                            if (availObjects.Keys.Contains(ac.ObjectName)) {
                                availObjects[ac.ObjectName].Add(GetContext(ac, skin));
                            } else {
                                availObjects.Add(ac.ObjectName, new List<AssetContext> {GetContext(ac, skin)});
                            }
                        } */
                        //valid loose files
                    }
                    
                }
            }
            var bulkNpcs = false;
            Identifier ParsePath(FileInfo file) {
                var matched = _parsers.Select(p => p.TryParse(file.Name, false)).FirstOrDefault(m => m.IsValid);
                if (matched.identifier != null) {
                    return matched.identifier;
                } else {
                    var pathMatched = _parsers.Select(p => p.TryParse(Path.GetRelativePath(rootInfo.FullName, file.FullName).Replace('\\', '/'), false)).FirstOrDefault(m => m.IsValid);
                    if (pathMatched.identifier != null) {
                        return pathMatched.identifier;
                    }
                }
                return null;
            }
            foreach (var obj in availObjects)
            {
                NPCPackMode? npcPackMode = null;
                // var vc = Constants.Vessels.FirstOrDefault(v => v.ObjectName == Path.GetFileName(obj.Key));
                AnsiConsole.WriteLine($"Indexing mod files for {GetFriendlyName(obj)}");
                foreach (var rawAsset in obj.Value)
                {
                    /* var firstIdent = rawAsset.SourcePath.GetFiles("*.uasset", SearchOption.AllDirectories).Select(n => ParsePath(n)).FirstOrDefault(n => n != null);
                    if (firstIdent != null && firstIdent is SkinIdentifier skin) {

                    } */
                    var extraAssets = new List<AssetContext>();
                    var ids = rawAsset.SourcePath.GetFiles("*", SearchOption.AllDirectories).Select(n => ParsePath(n)).Where(i => i != null).ToList();
                    //ids is now every recognised object in the current "root"
                    var skins = ids.Where(i => i is SkinIdentifier).Cast<SkinIdentifier>();
                    foreach (var skin in skins)
                    {
                        // var extraAssets = new List<AssetContext>();
                        List<AssetContext> GetSkinAssets() {
                            return obj.Value.Select(a => {
                                                a.FileFilter = $"{skin.Aircraft}_{skin.Slot}_*";
                                                return a;
                                            }).Concat(GetExtraAssets()).ToList();
                        }
                        IEnumerable<AssetContext> GetExtraAssets() {
                            return extraAssets.Where(a => a.SourcePath.FullName.Contains(skin.Aircraft));
                        }
                        void AddSingleSlot() {
                                    roots.AddOrUpdate(new PackTarget($"{skin.GetAircraftName().MakeSafe(true)}_{skin.GetSlotName()}", skin.BaseObjectName), GetSkinAssets().Concat(GetExtraAssets()).ToList());
                                }
                        //a skin file in the current context
                        if (skin.Type.StartsWith("I") && skin.IsNPC) {
                            foreach (var mrecDir in rawAsset.SourcePath.Parent.GetDirectories().Where(d => d.Name.ToLower() == "ex"))
                            {
                                // var slotId = regex.Match(instFile).Groups[1].Value;
                                extraAssets.Add(new AssetContext(mrecDir) { FilePattern = $"_[0x]?{skin.Slot.TrimStart('0')}_MREC"});
                            }
                        }
                        if (skin.Type.StartsWith("D") && Constants.NonPlayableAircraft.Any(a => a.ObjectName == skin.Aircraft)) {
                            // Non-playable skin, default to bulk packing
                            var name = $"{skin.GetAircraftName().MakeSafe(true)}_NPC";
                            roots.AddOrUpdate(new PackTarget(name, skin.Aircraft) { IsMerged = obj.Value.Count() > 1}, GetExtraAssets().Concat(new[] {rawAsset}).ToList());
                        } else if (skin.Type.StartsWith("D") && skin.Aircraft.IsPlayable()) {
                            var areNpcsAvailable = skins.Any(a => a.IsNPC);
                            if (areNpcsAvailable) {
                                void AddMergedAssets(IEnumerable<AssetContext> assets) {
                                    roots.AddOrUpdate(new PackTarget($"{skin.GetAircraftName().MakeSafe(true)}_NPC", skin.Aircraft + "_NPC"), assets.Concat(GetExtraAssets()).ToList());
                                }
                                if (skins.Where(a => a.Type == "D" && a.IsNPC).Count() > 1) { 
                                    npcPackMode ??= Sharprompt.Prompt.Select<NPCPackMode>($"Should we pack all {GetFriendlyName(obj)} NPC slots together?", valueSelector: e => e.GetEnumDescription());
                                    switch (npcPackMode)
                                    {
                                        case NPCPackMode.None:
                                            // roots.AddOrUpdate(new PackTarget($"{skin.GetAircraftName().MakeSafe(true)}_{skin.GetSlotName()}", skin.ObjectPath + "/" + skin.Aircraft), obj.Value.Concat(targetAssets).ToList());
                                            AddSingleSlot();
                                            break;
                                        case NPCPackMode.PerAircraft:
                                            AddMergedAssets(GetSkinAssets());
                                            // PackMerged();
                                            break;
                                        case NPCPackMode.Global:
                                            bulkNpcs = true;
                                            AddMergedAssets(GetSkinAssets());
                                            // roots.AddOrUpdate(new PackTarget($"{skin.GetAircraftName().MakeSafe(true)}_NPC", skin.ToObjectPath()), targetAssets);
                                            break;
                                        default:
                                            break;
                                    }
                                } else {
                                    //there's only 1 NPC skin in this context
                                    // roots.AddOrUpdate(new PackTarget($"{skin.GetAircraftName().MakeSafe(true)}_{skin.GetSlotName()}", skin.ObjectPath + "/" + skin.Aircraft), obj.Value.Concat(targetAssets).ToList());
                                    AddSingleSlot();
                                }
                            } else {
                                AddSingleSlot();
                            }
                            // add player slots
                        }
                    }
                    var ships = ids.Where(i => i is VesselIdentifier).Cast<VesselIdentifier>();
                    foreach (var ship in ships)
                    {
                        void AddSingleSlot() {
                                    roots.AddOrUpdate(
                                        new PackTarget(
                                            // $"{ship.GetVesselName(true).MakeSafe(true)}{(string.IsNullOrWhiteSpace(ship.Slot) ? string.Empty : "_" + ship.Slot)}", 
                                            $"{ship.GetVesselName(true).MakeSafe(true)}", 
                                            ship.ToObjectPath()
                                        ), obj.Value.Select(a => {
                                                a.FileFilter = $"*{ship.Vessel}*";
                                                return a;
                                            }).ToList());
                                }
                        if (ship.Type == "D") {
                            AddSingleSlot();
                        }
                    }
                    var portraits = ids.Where(i => i is PortraitIdentifier).Cast<PortraitIdentifier>();
                    if (portraits.Any()) {
                        roots.AddOrUpdate(
                            new PackTarget(
                                $"RadioPortraits",
                                portraits.First().ObjectPath
                            ), obj.Value.ToList());
                    }
                    var weapons = ids.Where(i => i is WeaponIdentifier).Cast<WeaponIdentifier>();
                    foreach (var weapon in weapons)
                    {
                        roots.AddOrUpdate(
                            new PackTarget(
                                $"{weapon.RawValue}",
                                weapon.BaseObjectName
                            ), obj.Value.Select(a => {
                                a.FileFilter = $"*{weapon.BaseObjectName}*";
                                return a;
                            }).ToList());
                    }
                    var canopies = ids.Where(i => i is CanopyIdentifier).Cast<CanopyIdentifier>();
                    foreach (var canopy in canopies)
                    {
                        roots.AddOrUpdate(
                            new PackTarget(
                                canopy.ToString().MakeSafe(),
                                canopy.BaseObjectName
                            ), obj.Value.Select(a => {
                                a.FileFilter = $"*{canopy.BaseObjectName}_Canopy*";
                                return a;
                            }).ToList());
                    }
                    var crosshairs = ids.Where(i => i is CrosshairIdentifier).Cast<CrosshairIdentifier>();
                    foreach (var crosshair in crosshairs)
                    {
                        
                    }


                    if (!ids.Any()) {
                        unhandledObjects.Add(obj.Key, obj.Value);
                    }
                }
                
                var ac = Constants.AllVehicles.GetAircraft().FirstOrDefault(a => a.ObjectName == Path.GetFileName(obj.Key));
                
                // var idents = obj.Value.SelectMany(ctx => ctx.SourcePath.GetFiles().Select(n => ParsePath(n.Name))).ToList();
                // var identset = obj.Value.ToDictionary(ctx => ctx, ctx => ctx.SourcePath.GetFiles().Select(n => ParsePath(n.Name)).ToList());
                // if (idents.All(a => a is SkinIdentifier)) {
                //     // the current context target only contains skin files
                //     var skinIdents = idents.Cast<SkinIdentifier>().ToList();
                //     foreach (var instanceFile in skinIdents.Where(s => s.Type.StartsWith("I")))
                //     {
                //             if (instanceFile.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")) {
                //                 var instFile = mrecObj.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).First().Name;
                //                 var mrecDir = mrecObj.SourcePath.Parent.GetDirectories().First(d => d.Name.ToLower() == "ex");
                //                 var regex = new Regex(@"_(\d{2}a?)_");
                //                 if (regex.IsMatch(instFile)) {
                //                     var slotId = regex.Match(instFile).Groups[1].Value;
                //                     obj.Value.Add(new AssetContext(mrecDir) { FilePattern = $"_[0x]{slotId.TrimStart('0')}_MREC"});
                //                 }
                                
                //                 // var mrecDir = new DirectoryInfo(Path.Combine(mrecParent.FullName, "ex"));
                                
                //             }
                //     }
                //     var usesInstance = skinIdents.Any(a => a.Type.StartsWith("I"));
                //     if (mrecObj.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")) {
                //         var instFile = mrecObj.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).First().Name;
                //         var mrecDir = mrecObj.SourcePath.Parent.GetDirectories().First(d => d.Name.ToLower() == "ex");
                //         var regex = new Regex(@"_(\d{2}a?)_");
                //         if (regex.IsMatch(instFile)) {
                //             var slotId = regex.Match(instFile).Groups[1].Value;
                //             obj.Value.Add(new AssetContext(mrecDir) { FilePattern = $"_[0x]{slotId.TrimStart('0')}_MREC"});
                //         }
                        
                //         // var mrecDir = new DirectoryInfo(Path.Combine(mrecParent.FullName, "ex"));
                        
                //     }
                // }
                // foreach (var id in idents)
                // {
                //     //we found an ident of *some sort* in the current target's files
                //     if (id is SkinIdentifier skin) {

                //     }
                // }
                // if (ac != null && Constants.NonPlayableAircraft.Any(a => a.ObjectName == ac.ObjectName)) {
                //     //default to bulk packing these
                //     foreach (var mrecObj in obj.Value.Where(f => f.SourcePath.GetFiles("*_Inst.uasset").Any()))
                //     {
                //         if (mrecObj.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")) {
                //             var instFile = mrecObj.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).First().Name;
                //             var mrecDir = mrecObj.SourcePath.Parent.GetDirectories().First(d => d.Name.ToLower() == "ex");
                //             var regex = new Regex(@"_(\d{2}a?)_");
                //             if (regex.IsMatch(instFile)) {
                //                 var slotId = regex.Match(instFile).Groups[1].Value;
                //                 obj.Value.Add(new AssetContext(mrecDir) { FilePattern = $"_[0x]{slotId.TrimStart('0')}_MREC"});
                //             }
                            
                //             // var mrecDir = new DirectoryInfo(Path.Combine(mrecParent.FullName, "ex"));
                            
                //         }
                //     }
                //     if (obj.Value.Any(f => f.SourcePath.GetFiles("*_Inst.uasset").Any() && obj.Value.Any(f => f.SourcePath.Parent.GetDirectories().Any(d => d.Name.ToLower() == "ex")))) {
                //         //hello again my old friend custom MRECs
                        
                        
                        
                //     }
                //     var name = $"{ac.Name.MakeSafe(true)}_NPC";
                //     roots.Add(new PackTarget(name, ac.PathRoot + ac.ObjectName) { IsMerged = obj.Value.Count() > 1}, obj.Value);
                // } else if (ac != null && Constants.PlayerAircraft.Any(a => a.ObjectName == ac.ObjectName)) {
                //     var areNpcsAvailable = obj.Value.Any(a => a.HasNpc);
                //     if (areNpcsAvailable) {
                //         var extraAssets = new List<AssetContext>();
                //         foreach (var exContext in obj.Value.Where(d => d.SourcePath.Parent.GetDirectories().Any(d => d.Name == "ex")))
                //         {
                //             //the current context *could* have files in ex/
                //             var regex = new Regex(@"_(\d{2}a?)_");
                //             var instFile = exContext.SourcePath.GetFiles("*_Inst.uasset", SearchOption.AllDirectories).FirstOrDefault()?.Name;
                //             if (instFile != null && regex.IsMatch(instFile)) {
                //                 var slotId = regex.Match(instFile).Groups[1].Value;
                //                 extraAssets.Add(new AssetContext(exContext.SourcePath.Parent.GetDirectories().First(d => d.Name == "ex")) { FilePattern = $"_[0x]?{slotId.TrimStart('0')}_"});
                //             }
                //         }
                //         obj.Value.AddRange(extraAssets);
                //         /* void PackSeparate() {
                //             foreach (var slot in obj.Value.Where(a => a.HasNpc))
                //             {
                //                 roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_{slot.SlotName}", ac), new[] {slot});
                //             }
                //         }
                //         void PackMerged() {
                //             roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_NPC", ac) { IsMerged = true }, obj.Value.Where(a => a.HasNpc));
                //         }
                //         if (obj.Value.Where(a => a.HasNpc).Count() > 1) { 
                //             var response = bulkNpcs ? NPCPackMode.Global : Sharprompt.Prompt.Select<NPCPackMode>($"Should we pack all {GetFriendlyName(obj)} NPC slots together?", valueSelector: e => e.GetEnumDescription());
                //             switch (response)
                //             {
                //                 case NPCPackMode.None:
                //                     PackSeparate();
                //                     break;
                //                 case NPCPackMode.PerAircraft:
                //                     PackMerged();
                //                     break;
                //                 case NPCPackMode.Global:
                //                     bulkNpcs = true;
                //                     PackMerged();
                //                     break;
                //                 default:
                //                     break;
                //             }
                //         } else {
                            
                //         } */
                //     }
                //     //detected player skin
                //     foreach (var skinDir in obj.Value.Where(d => d.HasPlayer)) {
                //         var skinId = skinDir.SlotName;
                //         roots.Add(new PackTarget($"{GetFriendlyName(obj).MakeSafe(true)}_{skinId}", ac, skinId), new[] {skinDir}.ToList());
                //     }
                // } else if (vc != null) {
                //     //default to bulk packing these
                //     var name = $"{vc.GetName().MakeSafe(true)}";
                //     roots.Add(new PackTarget(name, vc) { IsMerged = true}, obj.Value);
                // } else {
                //     unhandledObjects.Add(obj.Key, obj.Value);
                // }
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
            if (roots.Count(r => r.Key.IsMerged) > 1) {
                var mergedTargets = roots.Where(r => r.Key.IsMerged).ToList();
                while (mergedTargets.Any())
                {
                    AnsiConsole.MarkupLine("[deepskyblue2]Would you like to combine any of your merged packs together?[/]");
                    AnsiConsole.MarkupLine("Select any paths below that should be packed together, then press [bold grey]<ENTER>[/], or press [bold grey]<ENTER>[/] without selecting any to pack them individually.");
                    while (mergedTargets.Any() && mergedTargets.Where(r => !string.IsNullOrWhiteSpace(r.Key?.TargetFileName)).Any()) {
                        var candidates = Sharprompt.Prompt.MultiSelect<KeyValuePair<PackTarget, List<AssetContext>>>("Choose the paths to include in the next PAK file", mergedTargets, minimum: 0, valueSelector: t => t.Key?.TargetFileName);
                        if (candidates.Any()) {
                            // var commonRoot = candidates.Select(o => o.Key).FindCommonPath("/");
                            var commonRoot = candidates.SelectMany(o => o.Value).Select(o => o.GetTargetPath(rootInfo.FullName, s => s.Replace('\\', '/'))).FindCommonPath("/");
                            var selectedObjs = candidates;
                            var name = Sharprompt.Prompt.Input<string>("Enter a name for this pak file", defaultValue: Path.GetFileName(commonRoot), validators: new[] { FileValidators.ValidFileName()});
                            roots.Add(new PackTarget(name, commonRoot), selectedObjs.SelectMany(o => o.Value).ToList());
                            foreach (var cand in candidates)
                            {
                                roots.Remove(cand.Key);
                                mergedTargets.Remove(cand);
                            }
                        } else {
                            mergedTargets = new List<KeyValuePair<PackTarget, List<AssetContext>>>();
                        }
                    }
                }
            }
            var finalFiles = new List<string>();
            foreach (var buildRoot in roots)
            {
                var ctxName = Path.GetFileNameWithoutExtension(buildRoot.Key.TargetFileName);
                // buildRoot.Key.TargetAssets.AddRange(metaObjects);
                var bResult = await _buildService.RunBuild(ctxName, rootInfo.FullName, buildRoot.Value.Concat(metaObjects).ToArray());
                if (bResult == null) {
                    //well shit
                } else {
                    var finalName = $"{prefix}_{buildRoot.Key.TargetFileName}{(buildRoot.Key.IsMerged ? "_MULTI" : string.Empty)}_P.pak";
                    string NestedFolders(string finalName) {
                        var targetDir = Path.Combine(rootInfo.FullName, "Packed Files", buildRoot.Key.GetOutputPath());
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