namespace Toffee.Running;

public class EnvironmentStack
{
    private IList<Environment> _stack;

    private Environment CurrentEnvironment
    {
        get => _stack[^1];
        set => _stack[^1] = value;
    }

    private Environment GetEnvironmentAt(int indexFromTop) => _stack[new Index(indexFromTop + 1, true)];

    private void SetEnvironmentAt(int indexFromTop, Environment environment) =>
        _stack[new Index(indexFromTop + 1, true)] = environment;

    private int? LocateOnStack(string identifier) => _stack.Reverse()
        .Select((environment, index) => (index, environment))
        .Where(indexedEnvironment => indexedEnvironment.environment.Has(identifier))
        .Select(indexedEnvironment => (int?)indexedEnvironment.index)
        .FirstOrDefault();

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

    private void Push(Environment? environment = null)
    {
        _stack = _stack.Append(environment ?? new Environment()).ToList();
    }

    private void Pop()
    {
        if (_stack.Count <= 1)
            throw new NotImplementedException();
        _stack = _stack.SkipLast(1).ToList();
    }

    public EnvironmentStack Clone()
    {
        return new EnvironmentStack { _stack = new List<Environment>(_stack) };
    }

    public EnvironmentGuard PushGuard(Environment? environment = null)
    {
        Push(environment);
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
    private IDictionary<string, Variable> _variables = new Dictionary<string, Variable>();

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

public record Variable(object? Value, bool IsConst)
{
    public object? Value { get; set; } = Value;
}
