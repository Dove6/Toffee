using Toffee.ErrorHandling;
using Toffee.Running.Functions;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private readonly IRunnerErrorHandler? _errorHandler;
    private EnvironmentStack _environmentStack;

    public bool ShouldQuit { get; set; }

    public bool ExecutionInterrupted =>
        _environmentStack.ReturnEncountered || _environmentStack.BreakEncountered || ShouldQuit;

    public Runner(IRunnerErrorHandler? errorHandler = null, EnvironmentStack? environmentStack = null)
    {
        _errorHandler = errorHandler;
        _environmentStack = environmentStack ?? new EnvironmentStack(new Environment(new Dictionary<string, Variable>
        {
            { "print", new Variable(new PrintFunction(), false) },
            { "quit", new Variable(new QuitFunction(), false) }
        }));
    }

    private void EmitError(RunnerError error) => _errorHandler?.Handle(error);

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);
}
