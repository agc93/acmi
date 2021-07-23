using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BuildEngine;
using BuildEngine.Builder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PackCreator
{
    public class FileBuildRequest : IRequest<FileInfo?>
    {
        public string Name { get; set; }
        public List<BuildInstruction> Instructions { get; set; }
        public string GroupName { get; set; }
    }
    
    public class FileBuildRequestHandler : IRequestHandler<FileBuildRequest, FileInfo?>
    {
        private readonly IBuildServiceProvider _buildServiceProvider;
        private readonly ILogger<FileBuildRequestHandler> _logger;
        private readonly FileNameService _fileNameService;

        public FileBuildRequestHandler(IBuildServiceProvider buildServiceProvider, ILogger<FileBuildRequestHandler> logger, FileNameService fileNameService) {
            _buildServiceProvider = buildServiceProvider;
            _logger = logger;
            _fileNameService = fileNameService;
        }
        public async Task<FileInfo?> Handle(FileBuildRequest request, CancellationToken cancellationToken) {
            var buildService = await _buildServiceProvider.GetBuild(request.Name);
            foreach (var target in request.Instructions)
            {
                var linked = buildService.BuildContext.AddFromInstruction(target);
                if (!linked) {
                    _logger.LogError("[bold red]Failed to add folders to context directory![/]");
                }
            }

            var tempOut = Path.GetTempFileName();
            tempOut = Path.ChangeExtension(tempOut, ".pak");
            var bResult = await buildService.RunBuildAsync(tempOut);
            if (bResult.Success) {
                /*var fileName = _fileNameService.GetNameFromBuildGroup(
                    new KeyValuePair<SourceGroup, List<BuildInstruction>>(request.Name, request.Instructions),
                    request.GroupName);*/
                return bResult.Output as FileInfo;
            }

            return null;
        }
    }
}