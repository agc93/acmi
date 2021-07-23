using System.Collections.Generic;
using System.IO;
using System.Linq;
using AceCore;
using BuildEngine.Builder;
using Microsoft.Extensions.Logging;

namespace PackCreator
{
    public interface IPackedFileHandler
    {
        public bool Supports(Identifier ident);
        public IEnumerable<BuildInstruction>? HandleIdentifier(Identifier ident, FileInfo sourceFile);
    }

    public class IgnoredSkinFileHandler : IPackedFileHandler
    {
        public bool Supports(Identifier ident) {
            return ident is SkinIdentifier sIdent && (sIdent.Type == "N" || sIdent.Type == "MREC");
        }

        public IEnumerable<BuildInstruction>? HandleIdentifier(Identifier ident, FileInfo sourceFile) {
            return null;
        }
    }

    public abstract class SkinFileHandler : IPackedFileHandler
    {
        private readonly ILogger _logger;

        public SkinFileHandler() {
        }

        public SkinFileHandler(ILogger logger) {
            _logger = logger;
        }

        public abstract bool Supports(Identifier ident);

        public IEnumerable<BuildInstruction>? HandleIdentifier(Identifier ident, FileInfo sourceFile) {
            if (ident is SkinIdentifier skinIdentifier) {
                _logger?.LogTrace($"Detected {sourceFile.Name} as skin file: {skinIdentifier} for {skinIdentifier.BaseObjectName}");
                return HandleSkinIdentifier(skinIdentifier, sourceFile);
            }

            //this shouldn't be possible
            return null;
        }

        protected abstract IEnumerable<BuildInstruction>? HandleSkinIdentifier(SkinIdentifier skinIdentifier,
            FileInfo sourceFile);

        protected void AddRelativeSource(BuildInstruction targetInstr, Identifier ident, FileInfo srcFile) {
            try {
                var rp = Path.GetRelativePath(targetInstr.TargetPath, targetInstr.TargetPath);
                var absPath = Path.GetFullPath(Path.Combine(srcFile.Directory.FullName, rp));
                if (rp != "." && Directory.Exists(absPath)) {
                    // assume they're being added elsewhere for '.'

                    targetInstr.SourceFiles.AddFiles(new DirectoryInfo(absPath), ident.RawValue + ".*");
                }
            }
            catch (System.Exception) {
                _logger?.LogDebug(
                    $"Error while searching for relative path: {targetInstr.TargetPath}/{targetInstr.TargetPath}");
            }
        }
    }
    
    public class DiffuseFileHandler : SkinFileHandler
    {
        private readonly ILogger<DiffuseFileHandler> _logger;

        public DiffuseFileHandler(ILogger<DiffuseFileHandler> logger): base(logger) {
            _logger = logger;
        }
        public override bool Supports(Identifier ident) {
            return ident is SkinIdentifier {Type: "D"};
        }

        protected override IEnumerable<BuildInstruction>? HandleSkinIdentifier(SkinIdentifier skinIdentifier, FileInfo sourceFile) {
            _logger.LogTrace($"Detected Diffuse for {skinIdentifier.BaseObjectName} at {sourceFile.Name}");
            var instr = skinIdentifier.GetInstruction(sourceFile);
            var instructions = new List<BuildInstruction> {instr};
            var mrecPath = Path.Combine(sourceFile.Directory.FullName, $"{skinIdentifier.BaseObjectName}_MREC.uasset");
            var normPath = Path.Combine(sourceFile.Directory.FullName, $"{skinIdentifier.BaseObjectName}_N.uasset");
            // var instPath = Path.Combine(sourceFile.Directory.FullName, $"{skinIdentifier.BaseObjectName}_Inst.uasset");
            if (File.Exists(mrecPath)) {
                instr.SourceFiles.AddFiles(sourceFile.Directory, $"{skinIdentifier.BaseObjectName}_MREC.*");
            }
            if (File.Exists(normPath)) {
                instr.SourceFiles.AddFiles(sourceFile.Directory, $"{skinIdentifier.BaseObjectName}_N.*");
            }
            return instructions;
        }
    }

    public class InstanceFileHandler : SkinFileHandler
    {
        private readonly ILogger<InstanceFileHandler> _logger;
        private readonly ParserService _parser;

        public InstanceFileHandler(ILogger<InstanceFileHandler> logger, ParserService parser) : base(logger) {
            _logger = logger;
            _parser = parser;
        }

        public override bool Supports(Identifier ident) {
            return ident is SkinIdentifier {Type: "MREC"};
        }

        protected override IEnumerable<BuildInstruction>? HandleSkinIdentifier(SkinIdentifier skinIdentifier,
            FileInfo sourceFile) {
            var instr = skinIdentifier.GetInstruction(sourceFile);
            var instructions = new List<BuildInstruction> {instr};

            var iReader = new InstanceReader(_parser);
            var mrecs = iReader.FindMREC(sourceFile.FullName).ToList();
            if (mrecs.Any()) {
                var (mrecPath, mrecIdent) = mrecs.First();
                _logger.LogTrace($"Detected MREC for {mrecIdent.BaseObjectName} at {mrecPath}");
                var mrecInstr = new SkinInstruction(mrecIdent);
                mrecInstr.TargetPath = Identifier.BaseObjectPath + mrecPath.TrimEnd('/');
                mrecInstr.SourceFiles.AddFiles(sourceFile.Directory, mrecIdent.RawValue + ".*");
                AddRelativeSource(mrecInstr, mrecIdent, sourceFile);
                instructions.Add(mrecInstr);
            }

            var normals = iReader.FindNormal(sourceFile.FullName).ToList();
            if (normals.Any()) {
                var (normalPath, normalIdent) = normals.First();
                _logger.LogTrace($"Detected Normals for {normalIdent.BaseObjectName} at {normalPath}");
                var normalInstr =
                    new SkinInstruction(normalIdent) {TargetPath = Identifier.BaseObjectPath + normalPath.TrimEnd('/')};
                normalInstr.SourceFiles.AddFiles(sourceFile.Directory, normalIdent.RawValue + ".*");
                AddRelativeSource(normalInstr, normalIdent, sourceFile);
                instructions.Add(normalInstr);
            }
            
            return instructions;
        }
    }
}