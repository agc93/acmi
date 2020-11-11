using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AceCore;
using static InstallerCreator.ModInstaller.XmlHelpers;

namespace InstallerCreator.ModInstaller {
    internal class GroupedSet<T> where T : Identifier {
        internal IEnumerable<KeyValuePair<string, List<T>>> UniqueSets {get;set;}
        internal Dictionary<string, List<KeyValuePair<string, List<T>>>> Groups {get;set;}
    }
    public class ModInstallerBuilder {
        private readonly string _rootPath;
        private readonly string _title;
        private string _description;
        private readonly AppInfoService _infoService;
        private readonly ImageLocatorService _imageLocator;

        private XComment GetCreatedComment() {
            return new XComment($"Created with ACMI {_infoService?.GetAppVersion() ?? "v???"} [https://github.com/agc93/acmi]");
        }

        public ModInstallerBuilder(string modRootPath, string title, string description = null, AppInfoService infoService = null, ImageLocatorService imageLocator = null)
        {
            _rootPath = modRootPath;
            _title = title;
            _description = description;
            _infoService = infoService;
            _imageLocator = imageLocator;
        }
        public XDocument GenerateInfoXml(string author, string version, IEnumerable<string> groups, string description = null, string website = null) {
            var children = new List<XElement> {
                new XElement("Name", _title),
                    new XElement("Author", author),
                    new XElement("Version", version),
                    new XElement("Groups", groups.Select(g => new XElement("element", g)))
            };
            var finalDescription = description ?? _description ?? null;
            if (!string.IsNullOrWhiteSpace(finalDescription)) {
                _description = finalDescription;
                children.Add(new XElement("Description", finalDescription));
            }
            if (!string.IsNullOrWhiteSpace(website)) {
                children.Add(new XElement("Website", website));
            }
            var xdoc = new XDocument(GetCreatedComment(), new XElement("fomod", children));
            return xdoc;
        }

        public XDocument GenerateModuleConfigXml(SkinPack skins) {
            string MakeSafePath(string input) {
                return Path.GetInvalidFileNameChars().Aggregate(input, (current, c) => current.Replace(c, '-'));
            }
            var aircraftLookup = skins.Skins.ToLookup(k => k.Value.GetAircraftName());
            var moduleChildren = new List<XElement> {
                new XElement("moduleName", _title),
            };
            var image = checkForPreview();
            if (image != null) {
                moduleChildren.Add(new XElement("moduleImage", new XAttribute("path", image)));
            }
            if (skins.ReadmeFiles.Count > 0) {
                moduleChildren.Add(new XElement("requiredInstallFiles", skins.ReadmeFiles.Select(r => new XElement("file", new XAttribute("source", r), new XAttribute("destination", Path.Combine(MakeSafePath(_title), new FileInfo(r).Name))))));
            }
            moduleChildren.Add(GenerateStepsXml(aircraftLookup, skins.ExtraFiles.ToList(), skins.MultiSkinFiles.EnumerateDictionary(), skins.Crosshairs.EnumerateDictionary(), skins.Portraits.EnumerateDictionary(), skins.Weapons.EnumerateDictionary(), skins.Effects.EnumerateDictionary(), skins.Canopies.EnumerateDictionary(), skins.Emblems.EnumerateDictionary()));
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var xdoc = new XDocument(GetCreatedComment(), new XElement("config", new XAttribute(XNamespace.Xmlns + "xsi", xsi), new XAttribute(xsi + "noNamespaceSchemaLocation", "http://qconsulting.ca/fo3/ModConfig5.0.xsd"), moduleChildren));
            return xdoc;
        }

        private GroupedSet<T> GetSets<T>(Dictionary<string, List<T>> identifiers, string joinStr = " + ", Func<T, string> groupFunc = null) where T : Identifier {
                groupFunc ??= s => s.GetSlotName();
                var pGroups = identifiers.ToList().GroupBy(g => g.Value.Select(s => s.GetSlotName()).JoinLines(joinStr));
                var uniqueSets = pGroups.Where(g => g.Count() == 1).SelectMany(g => g.ToList());
                var groupedSets = pGroups.Where(g => g.Count() != 1).ToDictionary(k => k.Key, v => v.ToList());
                return new GroupedSet<T> { UniqueSets = uniqueSets, Groups = groupedSets};
            }
            

        private XElement GenerateStepsXml(ILookup<string, KeyValuePair<string, SkinIdentifier>> lookup, List<string> extraPaks, Dictionary<string, List<SkinIdentifier>> multiSkins = null, Dictionary<string, List<CrosshairIdentifier>> crosshairs = null, Dictionary<string, List<PortraitIdentifier>> portraits = null, Dictionary<string, List<WeaponIdentifier>> weapons = null, Dictionary<string, List<EffectsIdentifier>> effects = null, Dictionary<string, List<CanopyIdentifier>> canopies = null, Dictionary<string, List<EmblemIdentifier>> emblems = null) {
            XElement GetPluginElement(string fileName, string description = null) {
                var children = new List<XElement> {
                    Description(description),
                };
                var matchingImage = _imageLocator == null ? GetImagePath(fileName) : _imageLocator.GetImagePath(fileName, _rootPath);
                if (!string.IsNullOrWhiteSpace(matchingImage)) {
                    children.Add(new XElement("image", new XAttribute("path", Path.IsPathRooted(matchingImage) ? Path.GetRelativePath(_rootPath, matchingImage) : matchingImage)));
                }
                children.Add(new XElement("files", new XElement("file", new XAttribute("source", fileName), new XAttribute("destination", new FileInfo(fileName).NormalizeName()), new XAttribute("priority", "0"))));
                children.Add(OptionalTypeDescriptor());
                return new XElement("plugin", new XAttribute("name", new FileInfo(fileName).Name), children);
            }
            List<XElement> BuildGroups<T>(GroupedSet<T> groupedSet, string topName, Func<T, string> labelFunc = null) where T : Identifier {
                var groups = new List<XElement>();
                labelFunc ??= pn => pn.ToString();
                var uniqueSets = groupedSet.UniqueSets;
                if (uniqueSets.Any() && uniqueSets.All(s => s.Value.Any())) {
                    var canopyGeneralGroup = new XElement("group", Name(topName), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), uniqueSets.Select(s => GetPluginElement(s.Key, Includes(s.Value.Select(pi => labelFunc(pi)))))));
                    groups.Add(canopyGeneralGroup);
                }
                var groupedSets = groupedSet.Groups;
                if (groupedSets.Any() && groupedSets.All(s => s.Value.Any())) {
                    var groupedGroups = groupedSets.Select(gs => new XElement("group", Name(gs.Key), Type(SelectType.SelectExactlyOne), new XElement("plugins", ExplicitOrder(), NonePlugin(), gs.Value.Select(gsi => GetPluginElement(gsi.Key, Includes(gsi.Value.Select(i => i.GetSlotName())))))));
                    groups.AddRange(groupedGroups);
                }
                return groups;
            }
            XElement GetSelectionStep<T> (Dictionary<string, List<T>> idents, string title, string groupName, Func<List<T>, string> descriptionFunc = null) where T : Identifier {
                descriptionFunc ??= (s) => string.Empty;
                return new XElement("installStep", Name(title), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name(groupName), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), idents.Select(cm => GetPluginElement(cm.Key, descriptionFunc(cm.Value)))))));
            }
            var steps = new List<XElement>();
            steps.Add(new XElement("installStep", new XAttribute("name", "Introduction"), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), new XElement("group", new XAttribute("name", "Introduction"), new XAttribute("type", "SelectAll"), new XElement("plugins", new XAttribute("order", "Explicit"), new XElement("plugin", new XAttribute("name", "Introduction"), new XElement("description", GetDescription()), OptionalTypeDescriptor()))))));
            foreach (var aircraft in lookup)
            {
                steps.Add(new XElement("installStep", new XAttribute("name", aircraft.Key), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), aircraft.GroupBy(a => a.Value.GetSlotName()).Select(gs => new XElement("group", new XAttribute("name", gs.Key), new XAttribute("type", "SelectExactlyOne"), new XElement("plugins", new XAttribute("order", "Explicit"), NonePlugin(), gs.Select(ssf => GetPluginElement(ssf.Key))))))));
            }
            // the below implementation is planned for future changes
            // that being said, it should not be keyed on there only being one aircraft, but there only being one option per slot
            /* if (lookup.Count > 1) {
                foreach (var aircraft in lookup)
                {
                    steps.Add(new XElement("installStep", new XAttribute("name", aircraft.Key), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), aircraft.GroupBy(a => a.Value.GetSlotName()).Select(gs => new XElement("group", new XAttribute("name", gs.Key), new XAttribute("type", "SelectExactlyOne"), new XElement("plugins", new XAttribute("order", "Explicit"), NonePlugin(), gs.Select(ssf => GetPluginElement(ssf.Key))))))));
                }
            } else if (lookup.Count == 1) {
                var aircraft = lookup.First();
                steps.Add(new XElement("installStep", Name(aircraft.Key), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name(aircraft.Key), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), aircraft.Select(a => GetPluginElement(a.Key, "Replaces " + a.Value.GetSlotName())))))));
            } */
            if (multiSkins != null && multiSkins.Count > 0) {
                steps.Add(new XElement("installStep", new XAttribute("name", "Merged Files"), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name("Combination Skin Files"), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), multiSkins.Select(ms => GetPluginElement(ms.Key, string.Join(System.Environment.NewLine, ms.Value.Select(v => v.ToString())))))))));
            }
            if (crosshairs != null && crosshairs.Any()) {
                steps.Add(new XElement("installStep", Name("Crosshairs"), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name("Crosshair Mods"), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), crosshairs.Select(cm => GetPluginElement(cm.Key, Includes(cm.Value.Select(v => v.ToString())))))))));
            }
            if (portraits != null && portraits.Any()) {
                var pSets = GetSets(portraits);
                var groups = BuildGroups(pSets, "Portrait Packs");
                if (groups.Any()) {
                    steps.Add(new XElement("installStep", Name("Portraits"), new XElement("optionalFileGroups", ExplicitOrder(), groups)));
                }
            }
            if (weapons != null && weapons.Any()) {
                var wSets = GetSets(weapons);
                var groups = BuildGroups(wSets, "Weapon Mods");
                if (groups.Any()) {
                    steps.Add(new XElement("installStep", Name("Weapons"), new XElement("optionalFileGroups", ExplicitOrder(), groups)));
                }
            }
            if (canopies != null && canopies.Any()) {
                var cSets = GetSets(canopies);
                var groups = BuildGroups(cSets, "Canopy Mods");
                if (groups.Any()) {
                    steps.Add(new XElement("installStep", Name("Canopy"), new XElement("optionalFileGroups", ExplicitOrder(), groups)));
                }
            }
            if (effects != null && effects.Any()) {
                var eSets = GetSets(effects, groupFunc: e => e.EffectsObject);
                var groups = BuildGroups(eSets, "Visual Effects");
                if (groups.Any()) {
                    steps.Add(new XElement("installStep", Name("Effects"), new XElement("optionalFileGroups", ExplicitOrder(), groups)));
                }
                // steps.Add(new XElement("installStep", Name("Effects"), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name("Visual Effects"), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), effects.Select(cm => GetPluginElement(cm.Key, Includes(cm.Value.Select(v => v.ToString())))))))));
            }
            if (emblems != null && emblems.Any()) {
                steps.Add(GetSelectionStep(emblems, "Emblems", "Emblems", (ids) => Replaces(ids.Select(i => i.ToString()))));
                // steps.Add(new XElement("installStep", Name("Emblems"), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name("Emblems"), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), emblems.Select(ce => GetPluginElement(ce.Key, Replaces(ce.Value.Select(v => v.ToString())))))))));
            }
            if (extraPaks != null && extraPaks.Any()) {
                steps.Add(new XElement("installStep", new XAttribute("name", "Other Files"), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), new XElement("group", new XAttribute("name", "Other mod files"), new XAttribute("type", "SelectAny"), new XElement("plugins", new XAttribute("order", "Explicit"), extraPaks.Select(ep => GetPluginElement(ep)))))));
            }
            return new XElement("installSteps", ExplicitOrder(), steps);
        }

        private string GetImagePath(string fileName) {
            var skinFileName = Path.GetFileNameWithoutExtension(fileName).TrimEnd('_', '.');
            var pakFileLocation = new FileInfo(Path.Combine(_rootPath, fileName)).Directory;
            var exts = new[] { ".png", ".jpg"};
            var possibleImages = exts.Select(e => Path.Join(pakFileLocation.FullName, skinFileName + e));
            possibleImages = possibleImages
                .Concat(exts.Select(e => Path.Join(pakFileLocation.FullName, "Screenshots", skinFileName + e)))
                .Concat(exts.Select(e => Path.Join(_rootPath, "Screenshots", skinFileName + e)));
            var firstValid = possibleImages.FirstOrDefault(pi => File.Exists(pi));
            return firstValid;
        }

        public string WriteToInstallerFiles(XDocument infoXml, XDocument moduleConfigXml) {
            var fomodPath = Path.Combine(_rootPath, "fomod");
            Directory.CreateDirectory(fomodPath);
            infoXml.Save(Path.Combine(fomodPath, "info.xml"));
            moduleConfigXml.Save(Path.Combine(fomodPath, "ModuleConfig.xml"));
            return fomodPath;
        }

        private string GetDescription(string forceDescription = null) {
            var desc = forceDescription ?? _description ?? null;
            return $"This installer will guide you through choosing which custom skins (and other files) you want to install for each aircraft and each slot available in this archive. You can choose as few or as many skins as you want from the choices available, but be warned that your choices can still conflict with other mods you may have already installed!\n\n{(string.IsNullOrWhiteSpace(desc) ? string.Empty : desc)}";
        }

        private List<string> checkForReadme() {
            var files = Directory.EnumerateFiles(_rootPath, "*.txt", SearchOption.TopDirectoryOnly);
            return files.Count() > 0 ? files.Select(f => Path.GetRelativePath(_rootPath, f)).ToList() : new List<string>();
        }

        private string checkForPreview() {
            var files = _rootPath.GetFiles(new[] { "*.png", "*.jpg"}, SearchOption.TopDirectoryOnly).ToList();
            if (files.Any()) {
                if (files.Count == 1) {
                    return Path.GetRelativePath(_rootPath, files[0]);
                } else {
                    var preview = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLower().Contains("preview"));
                    if (preview != null) {
                        return Path.GetRelativePath(_rootPath, preview);
                    }
                }
            }
            return null;
        }
    }
}