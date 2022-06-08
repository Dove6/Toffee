using Toffee.ErrorHandling;
using Toffee.Running.Functions;
using Toffee.Scanning;

namespace Toffee.Running;

public partial class Runner : IRunner
{
    private const uint RecursionLimit = 1000;

    private readonly IRunnerErrorHandler? _errorHandler;
    private readonly TextWriter _writer;

    private EnvironmentStack _environmentStack;
    private uint _recursionCounter;
    private Position _currentPosition = new();

    private bool IsEntryPoint(IDisposable guard)
    {
        guard.Dispose();
        return _recursionCounter == 0;
    }

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

    private void EmitError(RunnerError error)
    {
        _errorHandler?.Handle(error);
        if (_environmentStack.IsInLoop)
            _environmentStack.RegisterBreak();
    }

    private void EmitWarning(RunnerWarning warning) => _errorHandler?.Handle(warning);

    private RecursionCounterGuard IncrementRecursionGuarded() => new(this);

    private EnvironmentStackBackupGuard OverwriteEnvironmentStackGuarded(EnvironmentStack? newEnvironmentStack) =>
        new(this, newEnvironmentStack);

    private class RecursionCounterGuard : IDisposable
    {
        private Runner? _runner;

        public RecursionCounterGuard(Runner runner)
        {
            _runner = runner;
            if (_runner._recursionCounter >= RecursionLimit)
                throw new RunnerException(new RecursionLimitExceeded(RecursionLimit));
            _runner._recursionCounter++;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_runner is null)
                return;
            _runner._recursionCounter--;
            _runner = null;
        }
    }

    private class EnvironmentStackBackupGuard : IDisposable
    {
        private Runner? _runner;
        private EnvironmentStack? _environmentStackBackup;

        public EnvironmentStackBackupGuard(Runner runner, EnvironmentStack? newEnvironmentStack)
        {
            _runner = runner;
            if (newEnvironmentStack is null)
                return;
            _environmentStackBackup = _runner._environmentStack;
            _runner._environmentStack = newEnvironmentStack;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_runner is null)
                return;
            if (_environmentStackBackup is not null)
                _runner._environmentStack = _environmentStackBackup;
            _environmentStackBackup = null;
            _runner = null;
        }
    }
}
