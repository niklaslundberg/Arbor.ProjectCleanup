using System.Text;

namespace Arbor.ProjectCleanup
{
    class Program
    {
        static int Main(string[] args)
        {
            ExitCode exitCode = App.RunAsync(args.Add(ConfigurationKeys.WhatIf).SafeToImmutableArray()).Result;

            return exitCode.Result;
        }
    }
}
