using System;
using AceCore.Tests.Infrastructure;
using Xunit;

namespace AceCore.Tests
{
    public class WeaponParserTests
    {
        [Theory]
        [JsonFileData("paths.json", "Weapons")]
        public void Should_Parse_Weapon_Details(string rawPath, string fullMatch, string weaponName, string slot, string type)
        {
            var parsed = WeaponIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(ident.RawValue, fullMatch);
            Assert.Equal(ident.Weapon, weaponName);
            Assert.Equal(ident.BaseObjectName, weaponName);
            Assert.Equal(ident.Type, type);
        }
    }
    public class SkinParserTests
    {
        /* [Theory]
        [JsonFileData("paths.json", "Aircraft")]
        public void Should_Parse_Aircraft(string rawPath, string fullMatch, string aircraft, string slot, string type)
        {
            var parsed = SkinIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
        } */

        [Theory]
        [JsonFileData("paths.json", "Aircraft")]
        public void Should_Parse_Skin_Details(string rawPath, string fullMatch, string aircraft, string slot, string type)
        {
            var parsed = SkinIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            Assert.Equal(ident.RawValue, fullMatch);
            Assert.Equal(ident.Aircraft, aircraft);
            Assert.Equal(ident.Slot, slot);
            Assert.Equal(ident.Type, type);
        }
    }

    public class EFfectsParserTests {
        [Theory]
        [JsonFileData("paths.json", "Effects")]
        public void Should_Parse_Effects_Path(string rawPath, string fullMatch, string objectPath, string objectName, string friendlyName) {
            var parsed = EffectsIdentifier.TryParse(rawPath, out var ident);
            Assert.True(parsed);
            // Assert.Equal(ident.RawValue, fullMatch);
            Assert.Equal(objectPath, ident.ObjectPath);
            Assert.Equal(objectName, ident.BaseObjectName);
            Assert.Equal(friendlyName, ident.EffectsObject);
        }
    }
}
