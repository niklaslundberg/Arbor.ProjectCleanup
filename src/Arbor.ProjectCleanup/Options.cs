using System.Collections.Immutable;

namespace Arbor.ProjectCleanup
{
    public class Options
    {
        public Options(
            string basePath,
            ImmutableArray<string> exclusions,
            ImmutableArray<string> targetDirectories,
            ImmutableArray<string> fileExtensionsToDelete,
            bool whatIf,
            bool deleteEmptyDirectories)
        {
            if (basePath != null)
            {
                BasePath = basePath;
            }

            Exclusions = exclusions;
            TargetDirectories = targetDirectories;
            FileExtensionsToDelete = fileExtensionsToDelete;
            WhatIf = whatIf;
            DeleteEmptyDirectories = deleteEmptyDirectories;
        }

        public string BasePath { get; }

        public ImmutableArray<string> Exclusions { get; }

        public ImmutableArray<string> TargetDirectories { get; }

        public ImmutableArray<string> FileExtensionsToDelete { get; }

        public bool WhatIf { get; }

        public bool DeleteEmptyDirectories { get; }
    }
}
