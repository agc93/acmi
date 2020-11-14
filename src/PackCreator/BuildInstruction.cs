using System.Collections.Generic;
using System.IO;
using AceCore;

namespace PackCreator {
    public abstract class BuildInstruction {
        protected BuildInstruction()
        {
            
        }
        public string TargetPath {get;set;}
        // public string PackGroup {get;set;}
        public SourceGroup SourceGroup {get;set;}
        public List<FileInfo> SourceFiles {get;set;} = new List<FileInfo>();
        public virtual string GetOutputName(string separator = "_") => null;
    }

    public class BuildInstruction<T> : BuildInstruction where T : Identifier {
        public BuildInstruction(T identifier)
        {
            this.Identifier = identifier;
            // this.SourceFile = sourceFile;
            this.SourceGroup = identifier.BaseObjectName;
            this.TargetPath = identifier.ObjectPath;
        }
        public T Identifier {get;set;}

        public override string GetOutputName(string separator = "_") {
            return Identifier.BaseObjectName;
        }
    }

    public class SkinInstruction : BuildInstruction<SkinIdentifier> {
        public SkinInstruction(SkinIdentifier identifier) : base(identifier) {

        }

        public override string GetOutputName(string separator = "_") {
            var parts = new List<string>();
            parts.Add(Identifier.GetAircraftName());
            parts.Add(Identifier.GetSlotNumber("Skin"));
            return string.Join(separator, parts);
        }
    }

    public class GenericInstruction : BuildInstruction {
        public GenericInstruction()
        {
            
        }
    }
}