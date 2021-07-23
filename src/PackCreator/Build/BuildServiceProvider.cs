using System.Threading.Tasks;
using BuildEngine;
using Microsoft.Extensions.Logging;
using UnPak.Core;

namespace PackCreator.Build
{
    public class BuildServiceProvider : IBuildServiceProvider
    {
        private readonly DirectoryBuildContextFactory _buildContextFactory;
        private readonly ILogger<PackBuildService> _buildLogger;
        private readonly PakFileProvider _pakFileProvider;


        public BuildServiceProvider(DirectoryBuildContextFactory buildContextFactory, ILogger<PackBuildService> buildLogger, PakFileProvider pakFileProvider) {
            _buildContextFactory = buildContextFactory;
            _buildLogger = buildLogger;
            _pakFileProvider = pakFileProvider;
        }
        public async Task<IBuildService> GetBuild(string? buildId) {
            return new PackBuildService(_buildContextFactory.CreateContext(buildId), _buildLogger, _pakFileProvider);
        }
    }
}