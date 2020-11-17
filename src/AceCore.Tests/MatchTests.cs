using AceCore.Tests.Infrastructure;
using Xunit;

namespace AceCore.Tests
{
    public class MatchTests
    {
        private readonly ParserService _parser;

        public MatchTests(ParserService parser)
        {
            _parser = parser;
        }
        [Theory]
        [JsonFileData("types.json", "Names")]
        public void Should_Parse_Correct_Match(string rawPath, string _, string fullMatch)
        {
            var ident = _parser.ParseMatch(rawPath, false);
            Assert.False(ident == null);
            Assert.Equal(ident.RawValue, fullMatch);
        }

        [Theory]
        [JsonFileData("types.json", "Names")]
        public void Should_Match_Correct_Type(string rawPath, string identType, string _) {
            var ident = _parser.ParseMatch(rawPath, false);
            Assert.True(ident != null);
            Assert.Equal($"AceCore.{identType}Identifier", ident.GetType().ToString());
        }
    }
}