namespace Arbor.ProjectCleanup
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            ExitCode exitCode = App.RunAsync(args.SafeToImmutableArray()).Result;

            return exitCode.Result;
        }
    }
}
