using System.Collections.Generic;
using System.IO;
using AceCore;
using BuildEngine.Builder;

namespace PackCreator {

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