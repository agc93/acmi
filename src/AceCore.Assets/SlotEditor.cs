using System;
using System.Collections.Generic;
using System.IO;
using AceCore.Parsers;
using Microsoft.Extensions.Logging;
using UAssetAPI;

namespace AceCore.Assets
{
    public class SlotEditor
    {
        private readonly FileInfo _sourceFile;
        private readonly ILogger _logger;
        private readonly IParserService _parserService;

        public SlotEditor(FileInfo srcFile, ILogger logger, IParserService parserService) {
            _sourceFile = srcFile;
            _logger = logger;
            _parserService = parserService;
        }

        public FileInfo RecookToSlot(SkinIdentifier skinIdentifier, string slot) {
            var baseObj = skinIdentifier.RawValue;
            var newObj = skinIdentifier.RawValue.Replace(skinIdentifier.Slot, slot);
            var newAssetPath = Path.Combine(_sourceFile.Directory.FullName, newObj + ".uasset");
            var sourceInstancePath = Path.Combine(_sourceFile.Directory.FullName,
                $"{skinIdentifier.Aircraft}_{skinIdentifier.Slot}_Inst.uasset");
            var targetInstancePath = Path.Combine(_sourceFile.Directory.FullName,
                $"{skinIdentifier.Aircraft}_{slot}_Inst.uasset");
            switch (skinIdentifier.Type) {
                case "D":
                {
                    var ac0 = new AssetWriter(Path.ChangeExtension(_sourceFile.FullName, ".uasset"), null, null);
                    try {
                        _logger.LogInformation($"Changing '{baseObj}' to '{newObj}'");
                        var assetPath = ac0.data.GetHeaderReference(0);
                        ac0.data.SetHeaderReference(0, assetPath.Replace(skinIdentifier.RawValue, newObj));
                        var nameRef = ac0.data.SearchHeaderReference(baseObj);
                        ac0.data.SetHeaderReference(nameRef, newObj);
                        ac0.Write(newAssetPath);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
                }
                case "MREC" or "N":
                {
                    if (File.Exists(targetInstancePath)) {
                        var reader = new InstanceAssetReader(_parserService);
                        var mrec = skinIdentifier.Type == "MREC"
                            ? reader.FindMREC(targetInstancePath, skinIdentifier)
                            : reader.FindNormal(targetInstancePath, skinIdentifier);
                        if (mrec is {Identifier: { }}) {
                            newObj = mrec.Value.Identifier.RawValue;
                            newAssetPath = newAssetPath.ChangeFileName(Path.GetFileNameWithoutExtension(mrec.Value.Path));
                        }
                    }
                    else {
                        newObj = $"{skinIdentifier.Aircraft}_{slot}_{skinIdentifier.Type}";
                        newAssetPath = Path.Combine(_sourceFile.Directory.FullName, newObj + ".uasset");
                    }
                    var ac0 = new AssetWriter(Path.ChangeExtension(_sourceFile.FullName, ".uasset"), null, null);
                    try {
                        _logger.LogInformation($"Changing '{baseObj}' to '{newObj}'");
                        var assetPath = ac0.data.GetHeaderReference(0);
                        ac0.data.SetHeaderReference(0, assetPath.Replace(skinIdentifier.RawValue, newObj));
                        var nameRef = ac0.data.SearchHeaderReference(baseObj);
                        ac0.data.SetHeaderReference(nameRef, newObj);
                        ac0.Write(newAssetPath);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
                }
            }

            
            
            foreach (var ubulkFile in _sourceFile.Directory.GetFiles($"{baseObj}.ubulk")) {
                ubulkFile.CopyTo(Path.Combine(_sourceFile.Directory.FullName, newObj + ".ubulk"));
            }

            return new FileInfo(newAssetPath);
        }

        private void GetPaths(string filePath, SkinIdentifier sourceIdentifier) {
            var iReader = new InstanceReader(_parserService);
            var instancePath = Path.Combine(new FileInfo(filePath).Directory.FullName,
                $"{sourceIdentifier.Aircraft}_{sourceIdentifier.Slot}_Inst.uasset");
            if (File.Exists(instancePath)) {
                var reader = new InstanceAssetReader(_parserService);
                var normal = reader.FindNormal(instancePath, sourceIdentifier);
                var mrec = reader.FindMREC(instancePath, sourceIdentifier);
            }
            
        }
    }
}