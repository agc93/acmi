using System;
using AceCore.Tests.Infrastructure;
using Xunit;

namespace AceCore.Tests
{
    public class WeaponParserTests
    {
        [Theory]
        [JsonFileData("paths.json", "Weapons")]
        public void Should_Parse_Weapon_Details(string rawPath, string fullMatch, string weaponName, string slot, string type, string path)
        {
            var parsed = WeaponIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(ident.RawValue, fullMatch);
            Assert.Equal(ident.Weapon, weaponName);
            Assert.Equal(ident.BaseObjectName, weaponName);
            Assert.Equal(ident.Type, type);
            Assert.Equal(ident.ObjectPath, path);
        }
    }
    public class SkinParserTests
    {
        [Theory]
        [JsonFileData("paths.json", "Aircraft")]
        public void Should_Parse_Skin_Details(string rawPath, string fullMatch, string aircraft, string slot, string type)
        {
            var parsed = SkinIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(fullMatch, ident.RawValue);
            Assert.Equal(ident.Aircraft, aircraft);
            Assert.Equal(ident.Slot, slot);
            Assert.Equal(ident.Type, type);
        }
    }

    public class EffectsParserTests {
        [Theory]
        [JsonFileData("paths.json", "Effects")]
        public void Should_Parse_Effects_Path(string rawPath, string fullMatch, string objectPath, string objectName, string friendlyName) {
            var parsed = EffectsIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(ident.RawValue, fullMatch);
            Assert.Equal(objectPath, ident.ObjectPath);
            Assert.Equal(objectName, ident.BaseObjectName);
            Assert.Equal(friendlyName, ident.EffectsObject);
        }
    }

    public class CockpitParserTests {
        [Theory]
        [JsonFileData("paths.json", "Cockpits")]
        public void Should_Parse_Cockpits_Path(string rawPath, string fullMatch, string aircraft, string area, string slot, string type) {
            var parsed = CockpitIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(fullMatch, ident.RawValue);
            Assert.Equal(aircraft, ident.BaseObjectName);
            Assert.Equal(area, ident.CockpitArea.OrDefault(string.Empty));
            Assert.Equal(slot, ident.SpecificPart.OrDefault(string.Empty));
            Assert.Equal(type, ident.Type);
        }
    }

    public class EmblemParserTests {
        [Theory]
        [JsonFileData("paths.json", "Emblems")]
        public void Should_Parse_Emblems_Path(string rawPath, string fullMatch, string format, string number, string size) {
            var parsed = EmblemIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(fullMatch, ident.RawValue);
            Assert.Equal(format, ident.Format);
            Assert.Equal(number, ident.Slot);
            Assert.Equal(size, ident.Size.OrDefault(string.Empty));
        }
    }
}
