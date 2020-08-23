using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AceCore;
using static InstallerCreator.ModInstaller.XmlHelpers;

namespace InstallerCreator.ModInstaller {
    public class ModInstallerBuilder {
        private readonly string _rootPath;
        private readonly string _title;
        private string _description;

        public ModInstallerBuilder(string modRootPath, string title, string description = null)
        {
            _rootPath = modRootPath;
            _title = title;
            _description = description;
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
            var xdoc = new XDocument(new XElement("fomod", children));
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
            moduleChildren.Add(GenerateStepsXml(aircraftLookup, skins.ExtraFiles.ToList(), skins.MultiSkinFiles));
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var xdoc = new XDocument(new XElement("config", new XAttribute(XNamespace.Xmlns + "xsi", xsi), new XAttribute(xsi + "noNamespaceSchemaLocation", "http://qconsulting.ca/fo3/ModConfig5.0.xsd"), moduleChildren));
            return xdoc;
        }

        private XElement GenerateStepsXml(ILookup<string, KeyValuePair<string, SkinIdentifier>> lookup, List<string> extraPaks, Dictionary<string, IEnumerable<SkinIdentifier>> multiSkins = null) {
            
            
            
            XElement GetPluginElement(string fileName, string description = null) {
                var children = new List<XElement> {
                    Description(description),
                };
                var matchingImage = GetImagePath(fileName);
                if (!string.IsNullOrWhiteSpace(matchingImage)) {
                    children.Add(new XElement("image", new XAttribute("path", Path.GetRelativePath(_rootPath, matchingImage))));
                }
                children.Add(new XElement("files", new XElement("file", new XAttribute("source", fileName), new XAttribute("destination", new FileInfo(fileName).Name), new XAttribute("priority", "0"))));
                children.Add(OptionalTypeDescriptor());
                return new XElement("plugin", new XAttribute("name", new FileInfo(fileName).Name), children);
            }
            var steps = new List<XElement>();
            steps.Add(new XElement("installStep", new XAttribute("name", "Introduction"), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), new XElement("group", new XAttribute("name", "Introduction"), new XAttribute("type", "SelectAll"), new XElement("plugins", new XAttribute("order", "Explicit"), new XElement("plugin", new XAttribute("name", "Introduction"), new XElement("description", GetDescription()), OptionalTypeDescriptor()))))));
            foreach (var aircraft in lookup) {
                steps.Add(new XElement("installStep", new XAttribute("name", aircraft.Key), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), aircraft.GroupBy(a => a.Value.GetSlotName()).Select(gs => new XElement("group", new XAttribute("name", gs.Key), new XAttribute("type", "SelectExactlyOne"), new XElement("plugins", new XAttribute("order", "Explicit"), NonePlugin(), gs.Select(ssf => GetPluginElement(ssf.Key))))))));
            }
            if (multiSkins != null && multiSkins.Count > 0) {
                steps.Add(new XElement("installStep", new XAttribute("name", "Merged Files"), new XElement("optionalFileGroups", ExplicitOrder(), new XElement("group", Name("Combination Skin Files"), Type(SelectType.SelectAny), new XElement("plugins", ExplicitOrder(), multiSkins.Select(ms => GetPluginElement(ms.Key, string.Join(System.Environment.NewLine, ms.Value.Select(v => v.ToString())))))))));
            }
            if (extraPaks != null && extraPaks.Any()) {
                steps.Add(new XElement("installStep", new XAttribute("name", "Other Files"), new XElement("optionalFileGroups", new XAttribute("order", "Explicit"), new XElement("group", new XAttribute("name", "Other mod files"), new XAttribute("type", "SelectAny"), new XElement("plugins", new XAttribute("order", "Explicit"), extraPaks.Select(ep => GetPluginElement(ep)))))));
            }
            return new XElement("installSteps", ExplicitOrder(), steps);
        }

        private string GetImagePath(string fileName) {
            var skinFileName = Path.GetFileNameWithoutExtension(fileName).TrimEnd('_', '.');
            var pakFileLocation = new FileInfo(Path.Combine(_rootPath, fileName)).Directory;
            var possibleImages = new[] {".png", ".jpg"}.Select(e => Path.Join(pakFileLocation.FullName, skinFileName + e));
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
            return $"This installer will guide you through choosing which custom skins you want to install for each aircraft and each slot available in this archive. You can choose as few or as many skins as you want from the choices available, but be warned that your choices can still conflict with other mods you may have already installed!\n\n{(string.IsNullOrWhiteSpace(desc) ? string.Empty : desc)}";
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