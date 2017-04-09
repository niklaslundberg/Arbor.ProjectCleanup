using System;

namespace Arbor.ProjectCleanup
{
    public class ExitCode
    {
        public static readonly ExitCode Failure = new ExitCode(1);

        public static readonly ExitCode Success = new ExitCode(0);

        public ExitCode(int result)
        {
            if (result < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(result));
            }

            Result = result;
        }

        public int Result { get; }
    }
}
