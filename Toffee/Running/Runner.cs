using Toffee.ErrorHandling;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private readonly IRunnerErrorHandler? _errorHandler;
    private EnvironmentStack _environmentStack;

    public Runner(IRunnerErrorHandler? errorHandler = null, EnvironmentStack? environmentStack = null)
    {
        _errorHandler = errorHandler;
        _environmentStack = environmentStack ?? new EnvironmentStack();
    }

    private void EmitError(RunnerError error) => _errorHandler?.Handle(error);

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);
}
