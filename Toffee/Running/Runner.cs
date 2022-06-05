using Toffee.ErrorHandling;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private readonly IRunnerErrorHandler? _errorHandler;

    public Runner(IRunnerErrorHandler? errorHandler = null)
    {
        _errorHandler = errorHandler;
    }

    private void EmitError(RunnerError error) => _errorHandler?.Handle(error);

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);
}
