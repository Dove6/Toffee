namespace Toffee.Running;

public class EnvironmentStack
{
    private IList<Environment> _stack;

    private Environment CurrentEnvironment
    {
        get => _stack[^1];
        set => _stack[^1] = value;
    }

    public object? ReturnValue { get; private set; }
    public bool ReturnEncountered { get; private set; }
    public bool BreakEncountered { get; private set; }

    // TODO: move to parser
    private bool _isInFunction;
    private bool _isInLoop;
    private EnvironmentType? _currentNonBlockType;

    private Environment GetEnvironmentAt(int indexFromTop) => _stack[new Index(indexFromTop + 1, true)];

    private void SetEnvironmentAt(int indexFromTop, Environment environment) =>
        _stack[new Index(indexFromTop + 1, true)] = environment;

    private int? LocateOnStack(string identifier) => _stack.Reverse()
        .Select((environment, index) => (index, environment))
        .Where(indexedEnvironment => indexedEnvironment.environment.Has(identifier))
        .Select(indexedEnvironment => (int?)indexedEnvironment.index)
        .FirstOrDefault();

    public void RegisterReturn(object? value)
    {
        if (!_isInFunction)
            throw new NotImplementedException();
        ReturnValue = value;
        ReturnEncountered = true;
    }

    public void RegisterBreak()
    {
        if (_currentNonBlockType != EnvironmentType.Loop)
            throw new NotImplementedException();
        BreakEncountered = true;
    }

    public EnvironmentStack(Environment? initialEnvironment = null)
    {
        _stack = new List<Environment> { initialEnvironment ?? new Environment() };
    }

    public object? Access(string identifier)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null)
            throw new NotImplementedException();
        return GetEnvironmentAt(environmentIndex.Value).Access(identifier);
    }

    public void Assign(string identifier, object? value)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null)
            throw new NotImplementedException();
        GetEnvironmentAt(environmentIndex.Value).Assign(identifier, value);
    }

    public void Initialize(string identifier, object? initialValue = null, bool isConst = false)
    {
        if (CurrentEnvironment.Has(identifier))
            throw new NotImplementedException();
        CurrentEnvironment.Initialize(identifier, initialValue, isConst);
    }

    public void FinalizeInitializationList()
    {
        CurrentEnvironment = CurrentEnvironment.Clone();
    }

    private void Push(EnvironmentType type)
    {
        _stack = _stack.Append(new Environment { Type = type }).ToList();
        if (type == EnvironmentType.Function)
        {
            _currentNonBlockType = EnvironmentType.Function;
            _isInFunction = true;
            ReturnEncountered = false;
            ReturnValue = null;
        }
        if (type == EnvironmentType.Loop)
        {
            _currentNonBlockType = EnvironmentType.Loop;
            _isInLoop = true;
            BreakEncountered = false;
        }
    }

    private void Pop()
    {
        if (_stack.Count <= 1)
            throw new NotImplementedException();
        var removedEnvironment = CurrentEnvironment;
        _stack = _stack.SkipLast(1).ToList();
        if (removedEnvironment.Type == EnvironmentType.Function)
        {
            ReturnEncountered = false;
            ReturnValue = null;
            _isInFunction = _isInFunction
                ? _stack.Reverse().FirstOrDefault(x => x.Type == EnvironmentType.Function) is not null
                : _isInFunction;
        }
        if (removedEnvironment.Type == EnvironmentType.Loop)
        {
            BreakEncountered = false;
            _isInLoop = _isInLoop
                ? _stack.Reverse().FirstOrDefault(x => x.Type == EnvironmentType.Loop) is not null
                : _isInLoop;
        }
        _currentNonBlockType = _isInFunction switch
        {
            true when !_isInLoop => EnvironmentType.Function,
            false when _isInLoop => EnvironmentType.Loop,
            false when !_isInLoop => null,
            _ => _stack.Reverse().FirstOrDefault(x => x.Type != EnvironmentType.Block)?.Type
        };
    }

    public EnvironmentStack Clone()
    {
        return new EnvironmentStack { _stack = new List<Environment>(_stack) };
    }

    public EnvironmentGuard PushGuard(EnvironmentType type = EnvironmentType.Block)
    {
        Push(type);
        return new EnvironmentGuard(this);
    }

    public class EnvironmentGuard : IDisposable
    {
        private EnvironmentStack? _environmentStack;

        public EnvironmentGuard(EnvironmentStack environmentStack) => _environmentStack = environmentStack;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_environmentStack == null)
                return;
            _environmentStack.Pop();
            _environmentStack = null;
        }
    }
}

public class Environment
{
    private IDictionary<string, Variable> _variables;

    public EnvironmentType Type { get; init; } = EnvironmentType.Block;

    public Environment(IDictionary<string, Variable>? variables = null) =>
        _variables = variables ?? new Dictionary<string, Variable>();

    public bool Has(string identifier) => _variables.ContainsKey(identifier);

    public void Initialize(string identifier, object? value, bool isConst)
    {
        _variables.Add(identifier, new Variable(value, isConst));
    }

    public object? Access(string identifier)
    {
        if (!_variables.ContainsKey(identifier))
            throw new NotImplementedException();
        return _variables[identifier].Value;
    }

    public void Assign(string identifier, object? value)
    {
        if (!_variables.ContainsKey(identifier))
            throw new NotImplementedException();
        var variable = _variables[identifier];
        if (variable.IsConst)
            throw new NotImplementedException();
        _variables[identifier].Value = value;
    }

    public Environment Clone() => new() { _variables = new Dictionary<string, Variable>(_variables) };
}

public enum EnvironmentType
{
    Function,
    Loop,
    Block
}

public record Variable(object? Value, bool IsConst)
{
    public object? Value { get; set; } = Value;
}
