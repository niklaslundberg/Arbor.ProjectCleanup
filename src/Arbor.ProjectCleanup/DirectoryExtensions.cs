using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Arbor.ProjectCleanup
{
    public static class DirectoryExtensions
    {
        public static ImmutableArray<DirectoryInfo> GetSubDirectories(
            this DirectoryInfo dir,
            SearchOption searchOption,
            params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            ImmutableArray<DirectoryInfo> subDirectories = names
                .SelectMany(name => dir.GetDirectories(name, searchOption))
                .ToImmutableArray();

            return subDirectories;
        }

        public static void DeleteEmptyDirectoriesRecursive(
            this DirectoryInfo currentDirectory,
            Logger logger = null,
            bool whatIf = false)
        {
            if (currentDirectory == null)
            {
                return;
            }

            foreach (DirectoryInfo subDirectory in currentDirectory.GetDirectories())
            {
                subDirectory.DeleteEmptyDirectoriesRecursive(logger, whatIf);
            }

            if (currentDirectory.EnumerateFiles().Any())
            {
                return;
            }

            if (currentDirectory.EnumerateDirectories().Any())
            {
                return;
            }

            try
            {
                if (!whatIf)
                {
                    currentDirectory.Delete(false);
                }
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Could not delete directory '{currentDirectory.FullName}', exception {ex}");
                throw;
            }

            logger?.Invoke($"Deleted empty directory '{currentDirectory.FullName}'");
        }

        public static void DeleteRecursive(
            this DirectoryInfo currentDiretory,
            Logger logger = null,
            bool whatIf = false)
        {
            if (currentDiretory == null)
            {
                return;
            }

            foreach (DirectoryInfo subDirectory in currentDiretory.GetDirectories())
            {
                subDirectory.DeleteRecursive(logger, whatIf);
            }

            foreach (FileInfo currentFile in currentDiretory.GetFiles().OrderBy(file => file.Name))
            {
                logger?.Invoke($"Deleting file.... '{currentFile.FullName}'");

                try
                {
                    if (!whatIf)
                    {
                        currentFile.Delete();
                    }

                    logger?.Invoke($"Deleted file..... '{currentFile.FullName}'");
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"Could not delete file '{currentFile.FullName}', exception {ex}");
                    throw;
                }
            }

            try
            {
                if (!whatIf)
                {
                    currentDiretory.Delete(true);
                }
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Could not delete file '{currentDiretory.FullName}', exception {ex}");
                throw;
            }

            logger?.Invoke($"Deleted directory '{currentDiretory.FullName}'");
        }
    }
}
