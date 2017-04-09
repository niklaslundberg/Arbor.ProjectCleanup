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

            string sourceRootPath = VcsPathHelper.FindVcsRootPath();

            var sourceRootDirectory = new DirectoryInfo(sourceRootPath);

            var exclusions = new[] { ".git", ".vs", "packages", ".nuget" };

            ImmutableArray<DirectoryInfo> tempDirectories =
                sourceRootDirectory.GetSubDirectories(SearchOption.AllDirectories, "obj", "bin", "temp")
                    .Where(directory => !directory.FullName.Split(Path.DirectorySeparatorChar)
                        .Any(path => exclusions.Any(
                            exclusion => path.Equals(exclusion, StringComparison.OrdinalIgnoreCase))))
                    .OrderByDescending(directory => directory.FullName.Count(c => c == Path.DirectorySeparatorChar))
                    .ThenByDescending(directory => directory.FullName.Length)
                    .ToImmutableArray();

            try
            {
                foreach (DirectoryInfo tempDirectory in tempDirectories)
                {
                    TryDeleteRecursiveWithRetry(tempDirectory, whatIf);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ExitCode.Failure;
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
