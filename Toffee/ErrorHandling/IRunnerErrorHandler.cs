using Toffee.Running;

namespace Toffee.ErrorHandling;

public interface IRunnerErrorHandler
{
    void Handle(RunnerError lexerError);
    void Handle(RunnerWarning lexerWarning);
}
