using System.Collections.Generic;
using AceCore;

namespace InstallerCreator
{
    public class SkinPack
    {
        public Dictionary<string, SkinIdentifier> Skins {get;set;} = new Dictionary<string, SkinIdentifier>();
        public Dictionary<string, IEnumerable<SkinIdentifier>> MultiSkinFiles {get;set;} = new Dictionary<string, IEnumerable<SkinIdentifier>>();
        public List<string> ExtraFiles {get;set;} = new List<string>();
        public List<string> ReadmeFiles {get;set;} = new List<string>();
    }
}