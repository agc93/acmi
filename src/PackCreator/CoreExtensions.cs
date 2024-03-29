using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using AceCore;
using BuildEngine.Builder;

namespace PackCreator {
    public static class CoreExtensions {
        public static string ToArgument(this string path) {
            return path.Contains(' ')
                ? $"\"{path}\""
                : path;
        }

        public static IEnumerable<string> ToArguments(this IEnumerable<string> paths) {
            return paths.Select(p => p.ToArgument());
        }

        public static IEnumerable<DirectoryInfo> GetLeafNodes(this DirectoryInfo root, bool requireFiles = true) {
            var folderWithoutSubfolder = Directory.EnumerateDirectories(root.FullName, "*.*", SearchOption.AllDirectories)
                .Where(f => (!Directory.EnumerateDirectories(f, "*.*", SearchOption.TopDirectoryOnly).Any()) && (requireFiles ? Directory.EnumerateFiles(f, "*.*", SearchOption.AllDirectories).Any() : true));
            return folderWithoutSubfolder.Select(fs => Path.GetFileName(fs).Any(char.IsDigit) ? Directory.GetParent(fs) : new DirectoryInfo(fs)).Select(f => f.FullName).Distinct().Select(p => new DirectoryInfo(p)).ToList();
            // return folderWithoutSubfolder;
        }

        public static BuildInstruction GetInstruction(this SkinIdentifier skinIdentifier, FileInfo srcFile) {
            var instr = new SkinInstruction(skinIdentifier) {
                SourceFiles = srcFile.Directory.GetFiles($"{skinIdentifier.BaseObjectName}_{skinIdentifier.Type}.*").ToList()
            };
            return instr;
        }

        public static void CopyTo(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, string fileFilter) {

            CopyAll(sourceDirectory, targetDirectory, fileFilter);
        }

        public static void CopyTo(this DirectoryInfo sourceDirectory, string targetDirectory, string fileFilter) {

            CopyAll(sourceDirectory, new DirectoryInfo(targetDirectory), fileFilter);
        }

        public static void CopyTo(this DirectoryInfo sourceDirectory, string targetDirectory, Func<FileInfo, bool> fileFilter) {

            CopyAllFiltered(sourceDirectory, new DirectoryInfo(targetDirectory), fileFilter);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, string filter) {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles(filter ?? "*")) {
                // System.Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, filter);
            }
        }

        private static void CopyAllFiltered(DirectoryInfo source, DirectoryInfo target, Func<FileInfo, bool> filterFunc) {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.EnumerateFiles().Where(f => filterFunc(f))) {
                // System.Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAllFiltered(diSourceSubDir, nextTargetSubDir, filterFunc);
            }
        }

        public static string MakeSafe(this string input, bool removeChars = false) {
            return removeChars
                ? Path.GetInvalidFileNameChars().Aggregate(input, (current, c) => current.IndexOf(c) == -1 ? current : current.Remove(current.IndexOf(c), 1))
                : Path.GetInvalidFileNameChars().Aggregate(input, (current, c) => current.Replace(c, '-'));
        }

        public static string GetEnumDescription(this Enum value) {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any()) {
                return attributes.First().Description;
            }
            return value.ToString();
        }

        public static string FindCommonPath(this IEnumerable<string> paths, string separator = "/") {
            var commonPath = string.Empty;
            // var separator = "/";
            List<string> separatedPath = paths
                .First(str => str.Length == paths.Max(st2 => st2.Length))
                .Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            foreach (string pathSegment in separatedPath.AsEnumerable()) {
                if (commonPath.Length == 0 && paths.All(str => str.StartsWith(pathSegment))) {
                    commonPath = pathSegment;
                } else if (paths.All(str => str.StartsWith(commonPath + separator + pathSegment))) {
                    commonPath += separator + pathSegment;
                } else {
                    break;
                }
            }

            return commonPath;
        }

        internal static string ToObjectPath(this AceCore.VesselIdentifier vessel) {
            return vessel.ObjectPath + "/" + vessel.ObjectPath;
        }

        internal static List<FileInfo> AddFiles(this List<FileInfo> files, DirectoryInfo rootPath, string pattern) {
            var matchFiles = rootPath.GetFiles(pattern);
            if (matchFiles.Any()) {
                files.AddRange(matchFiles);
            }
            return files;
        }
    }
}