namespace Arbor.ProjectCleanup
{
    public class ExitCode
    {
        public static readonly ExitCode Failure = new ExitCode(1);

        public static readonly ExitCode Success = new ExitCode(0);

        public int Result { get; }

        public ExitCode(int result)
        {
            Result = result;
        }
    }
}