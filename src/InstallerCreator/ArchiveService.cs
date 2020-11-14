using System;
using System.IO;
using System.IO.Compression;
using AceCore;

namespace InstallerCreator
{
    public class ArchiveService
    {
        public (bool Success, FileInfo Result) MakeZip(ModRootPath rootPath, string targetFileName, string extension = ".zip") {
            var tempFile = Path.Combine(Path.GetTempPath(), targetFileName);
            targetFileName = Path.GetFileNameWithoutExtension(targetFileName) + (extension ?? ".zip");
            var absoluteTarget = Path.Combine(rootPath.AbsolutePath, targetFileName);
            ZipFile.CreateFromDirectory(rootPath.AbsolutePath, tempFile);
            File.Move(tempFile, absoluteTarget);
            Console.WriteLine($"Created archive file at ${absoluteTarget}");
            return (File.Exists(absoluteTarget), new FileInfo(absoluteTarget));
        }
    }
}