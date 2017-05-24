using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Aesculus.Core;
using Autofac;

namespace Arbor.ProjectCleanup
{
    public sealed class App
    {
        private static IContainer _container;
        private readonly Logger _logger;

        public App(Logger logger)
        {
            _logger = logger;
        }

        public static async Task<ExitCode> RunAsync(ImmutableArray<string> args)
        {
            try
            {
                var builder = new ContainerBuilder();

                builder.RegisterType<App>().AsSelf().SingleInstance();

                builder.Register(context => (Logger)Console.WriteLine).AsSelf().SingleInstance();

                _container = builder.Build();

                var instance = _container.Resolve<App>();

                return await instance.ExecuteAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ExitCode.Failure;
            }
        }

        private async Task<ExitCode> ExecuteAsync(ImmutableArray<string> args)
        {
            bool whatIf = args.Any(arg => arg.Equals(ConfigurationKeys.WhatIf, StringComparison.OrdinalIgnoreCase));

            bool deleteEmptyDirectories = args.Any(arg => arg.Equals(
                ConfigurationKeys.DeleteEmptyDirectories,
                StringComparison.OrdinalIgnoreCase));

            ImmutableArray<string> exclusions = new[] { ".git", ".vs" }.ToImmutableArray();

            ImmutableArray<string> targets = new[] { "obj", "bin", "temp", "tmp", "artifacts", "arbor.x" }
                .ToImmutableArray();

            string baseDirectory = args.FirstOrDefault(arg => !arg.StartsWith(
                                       "-",
                                       StringComparison.OrdinalIgnoreCase)) ??
                                   VcsPathHelper.FindVcsRootPath(Directory.GetCurrentDirectory());

            ImmutableArray<string> fileExtensionsToDelete = new[] { ".tmp", ".cache", ".orig" }.ToImmutableArray();

            var options = new Options(
                baseDirectory,
                exclusions,
                targets,
                fileExtensionsToDelete,
                whatIf,
                deleteEmptyDirectories);

            return await ExecuteAsync(options);
        }

        private async Task<ExitCode> ExecuteAsync(Options options)
        {
            var baseDirectory = new DirectoryInfo(options.BasePath);

            Console.WriteLine("Using options: ");
            Console.WriteLine($"Base path:. {options.BasePath}");
            Console.WriteLine($"What if:... {options.WhatIf}");
            Console.WriteLine($"Targets:... {string.Join(", ", options.TargetDirectories)}");
            Console.WriteLine($"Exclusions: {string.Join(", ", options.Exclusions)}");

            if (!baseDirectory.Exists)
            {
                Console.WriteLine($"Could not find base directory '{options.BasePath}'");
                return ExitCode.Failure;
            }

            ImmutableArray<DirectoryInfo> tempDirectories =
                baseDirectory.GetSubDirectories(SearchOption.AllDirectories, options.TargetDirectories.ToArray())
                    .Where(directory => !directory.FullName.Split(Path.DirectorySeparatorChar)
                        .Any(path => options.Exclusions.Any(
                            exclusion => path.Equals(exclusion, StringComparison.OrdinalIgnoreCase))))
                    .OrderByDescending(directory => directory.FullName.Count(c => c == Path.DirectorySeparatorChar))
                    .ThenByDescending(directory => directory.FullName.Length)
                    .ToImmutableArray();

            try
            {
                foreach (DirectoryInfo tempDirectory in tempDirectories)
                {
                    tempDirectory.DeleteRecursive(_logger, options.WhatIf);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ExitCode.Failure;
            }

            foreach (string fileExtension in options.FileExtensionsToDelete)
            {
                ImmutableArray<FileInfo> filesToDelete = baseDirectory
                    .GetFiles($"*{fileExtension}", SearchOption.AllDirectories)
                    .Where(file => !file.FullName.Split(Path.DirectorySeparatorChar)
                        .Any(path => options.Exclusions.Any(
                            exclusion => path.Equals(exclusion, StringComparison.OrdinalIgnoreCase))))
                    .ToImmutableArray();

                foreach (FileInfo fileInfo in filesToDelete)
                {
                    if (!options.WhatIf)
                    {
                        fileInfo.Delete();
                    }

                    _logger?.Invoke($"Deleted file..... '{fileInfo.FullName}'");
                }
            }

            if (options.DeleteEmptyDirectories)
            {
                baseDirectory.DeleteEmptyDirectoriesRecursive(_logger, options.WhatIf);
            }

            return ExitCode.Success;
        }

        private void TryDeleteRecursiveWithRetry(DirectoryInfo tempDirectory, bool whatIf)
        {
            const int retryTimeoutInMilliseconds = 10;

            int maxAttempts = 5;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    tempDirectory.DeleteRecursive(_logger, whatIf);
                }
                catch (Exception innerEx)
                {
                    if (i == maxAttempts - 1)
                    {
                        throw;
                    }

                    _logger?.Invoke($"Exception thrown, trying again {innerEx.Message}");

                    Thread.Sleep(retryTimeoutInMilliseconds);
                }
            }
        }
    }
}
