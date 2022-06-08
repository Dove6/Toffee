namespace Toffee.Running;

public class RunnerException : Exception
{
    public RunnerError Error { get; }

    public RunnerException(RunnerError error) =>
        Error = error;

    public RunnerException(RunnerError error, string message) : base(message) =>
        Error = error;

    public RunnerException(RunnerError error, string message, Exception inner) : base(message, inner) =>
        Error = error;
}
