﻿namespace Toffee.Running;

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

    private bool _isInFunction;
    public bool IsInLoop { get; private set; }
    private EnvironmentType? _currentNonBlockType;

    private Environment GetEnvironmentAt(int indexFromTop) => _stack[new Index(indexFromTop + 1, true)];

    private int? LocateOnStack(string identifier) => _stack.Reverse()
        .Select((environment, index) => (index, environment))
        .Where(indexedEnvironment => indexedEnvironment.environment.Has(identifier))
        .Select(indexedEnvironment => (int?)indexedEnvironment.index)
        .FirstOrDefault();

    public void RegisterReturn(object? value)
    {
        if (!_isInFunction)
            throw new RunnerException(new ReturnOutsideOfFunction());
        ReturnValue = value;
        ReturnEncountered = true;
    }

    public void RegisterBreak()
    {
        if (_currentNonBlockType != EnvironmentType.Loop)
            throw new RunnerException(new BreakOutsideOfLoop());
        BreakEncountered = true;
    }

    public EnvironmentStack(Environment? initialEnvironment = null)
    {
        _stack = new List<Environment> { initialEnvironment ?? new Environment() };
    }

    public object? Access(string identifier)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null ||
                !GetEnvironmentAt(environmentIndex.Value).TryAccess(identifier, out var value, out _))
            throw new RunnerException(new VariableUndefined(identifier));
        return value;
    }

    public void Assign(string identifier, object? value, bool ignoreImmutability = false)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null
                || !GetEnvironmentAt(environmentIndex.Value).TryAccess(identifier, out _, out var isConst))
            throw new RunnerException(new VariableUndefined(identifier));
        if (isConst!.Value && !ignoreImmutability)
            throw new RunnerException(new AssignmentToConst(identifier));
        GetEnvironmentAt(environmentIndex.Value).TryAssign(identifier, value);
    }

    public void Initialize(string identifier, object? initialValue = null, bool isConst = false)
    {
        if (CurrentEnvironment.Has(identifier))
            throw new RunnerException(new VariableAlreadyDefined(identifier));
        CurrentEnvironment.TryInitialize(identifier, initialValue, isConst);
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
            IsInLoop = true;
            BreakEncountered = false;
        }
    }

    private void Pop()
    {
        if (_stack.Count <= 1)
            throw new NotSupportedException();
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
            IsInLoop = IsInLoop
                ? _stack.Reverse().FirstOrDefault(x => x.Type == EnvironmentType.Loop) is not null
                : IsInLoop;
        }
        _currentNonBlockType = _isInFunction switch
        {
            true when !IsInLoop => EnvironmentType.Function,
            false when IsInLoop => EnvironmentType.Loop,
            false when !IsInLoop => null,
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

    public bool TryInitialize(string identifier, object? value, bool isConst)
    {
        return _variables.TryAdd(identifier, new Variable(value, isConst));
    }

    public bool TryAccess(string identifier, out object? value, out bool? isConst)
    {
        (value, isConst) = (null, null);
        if (!_variables.ContainsKey(identifier))
            return false;
        var variable = _variables[identifier];
        (value, isConst) = (variable.Value, variable.IsConst);
        return true;
    }

    public bool TryAssign(string identifier, object? value)
    {
        if (!_variables.ContainsKey(identifier))
            return false;
        _variables[identifier].Value = value;
        return true;
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
