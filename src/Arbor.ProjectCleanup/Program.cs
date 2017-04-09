using System.Collections.Immutable;

namespace Arbor.ProjectCleanup
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            ImmutableArray<string> allArgs = args?.Add(ConfigurationKeys.WhatIf).SafeToImmutableArray() ?? ImmutableArray<string>.Empty;

            ExitCode exitCode = App.RunAsync(allArgs).Result;

            return exitCode.Result;
        }
    }
}
