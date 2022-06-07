using Toffee.ErrorHandling;
using Toffee.Running.Functions;
using Toffee.Scanning;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private readonly IRunnerErrorHandler? _errorHandler;
    private readonly TextWriter _writer;
    private EnvironmentStack _environmentStack;

    private Position _currentPosition = new();

    public bool ShouldQuit { get; set; }

    public bool ExecutionInterrupted =>
        _environmentStack.ReturnEncountered || _environmentStack.BreakEncountered || ShouldQuit;

    public Runner(IRunnerErrorHandler? errorHandler = null, EnvironmentStack? environmentStack = null, TextWriter? writer = null)
    {
        _errorHandler = errorHandler;
        _writer = writer ?? Console.Out;
        _environmentStack = environmentStack ?? new EnvironmentStack(new Environment(new Dictionary<string, Variable>
        {
            { "print", new Variable(new PrintFunction(_writer), false) },
            { "quit", new Variable(new QuitFunction(), false) }
        }));
    }

    private event Action ErrorEmitted;

    private void EmitError(RunnerError error)
    {
        _errorHandler?.Handle(error);
        if (_environmentStack.IsInLoop)
            _environmentStack.RegisterBreak();
    }

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);
}
