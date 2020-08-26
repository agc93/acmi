using System.Collections.Generic;
using System.Linq;
using AceCore;

namespace InstallerCreator
{
    public class SkinPack
    {
        public Dictionary<string, SkinIdentifier> Skins {get;set;} = new Dictionary<string, SkinIdentifier>();
        public Dictionary<string, IEnumerable<SkinIdentifier>> MultiSkinFiles {get;set;} = new Dictionary<string, IEnumerable<SkinIdentifier>>();
        public List<string> ExtraFiles {get;set;} = new List<string>();
        public List<string> ReadmeFiles {get;set;} = new List<string>();

        public Dictionary<string, IEnumerable<CrosshairIdentifier>> Crosshairs {get;set;} = new Dictionary<string, IEnumerable<CrosshairIdentifier>>();
        public Dictionary<string, IEnumerable<PortraitIdentifier>> Portraits {get;set;} = new Dictionary<string, IEnumerable<PortraitIdentifier>>();
        public Dictionary<string, IEnumerable<WeaponIdentifier>> Weapons {get;set;} = new Dictionary<string, IEnumerable<WeaponIdentifier>>();
        public Dictionary<string, IEnumerable<EffectsIdentifier>> Effects {get;set;} = new Dictionary<string, IEnumerable<EffectsIdentifier>>();
        public Dictionary<string, IEnumerable<CanopyIdentifier>> Canopies {get;set;} = new Dictionary<string, IEnumerable<CanopyIdentifier>>();

        public bool IsEmpty() {
            return GetFileCount(true) == 0;
        }

        

        public void Add(List<Identifier> idents, string relPath) {
            var ident = idents.FirstOrDefault();
            if (ident == null) {
                ExtraFiles.Add(relPath);
                return;
            }
            if (ident is SkinIdentifier skinId) {
                var skinIds = idents.Cast<SkinIdentifier>().ToList();
                if (skinIds.Count > 1) {
                    MultiSkinFiles.Add(relPath, skinIds);
                } else {
                    Skins.Add(relPath, ident as SkinIdentifier);
                }
            } else if (ident is PortraitIdentifier) {
                Portraits.Add(relPath, idents.Cast<PortraitIdentifier>());
            } else if (ident is CrosshairIdentifier) {
                Crosshairs.Add(relPath, idents.Cast<CrosshairIdentifier>());
            } else if (ident is WeaponIdentifier) {
                Weapons.Add(relPath, idents.Cast<WeaponIdentifier>());
            } else if (ident is EffectsIdentifier) {
                Effects.Add(relPath, idents.Cast<EffectsIdentifier>());
            } else if (ident is CanopyIdentifier) {
                Canopies.Add(relPath, idents.Cast<CanopyIdentifier>());
            }
        }

        public int GetFileCount(bool includeExtra = false) {
            var detected = Skins.Keys.Count + MultiSkinFiles.Count + Portraits.Keys.Count + Crosshairs.Keys.Count + Weapons.Keys.Count + Effects.Keys.Count + Canopies.Keys.Count;
            return includeExtra 
                ? detected + ExtraFiles.Count
                : detected;
        }
    }
}