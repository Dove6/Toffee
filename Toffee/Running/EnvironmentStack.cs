using System.Collections.Immutable;

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
        .Where(indexedEnvironment => indexedEnvironment.environment.Variables.ContainsKey(identifier))
        .Select(indexedEnvironment => (int?)indexedEnvironment.index)
        .FirstOrDefault();

    public EnvironmentStack(Environment? initialEnvironment = null)
    {
        _stack = new List<Environment>
            { initialEnvironment ?? new Environment(ImmutableDictionary<string, Variable>.Empty) };
    }

    public object? Access(string identifier)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null)
            throw new NotImplementedException();
        return GetEnvironmentAt(environmentIndex.Value).Variables[identifier].Value;
    }

    public void Assign(string identifier, object? value)
    {
        var environmentIndex = LocateOnStack(identifier);
        if (environmentIndex is null)
            throw new NotImplementedException();
        SetEnvironmentAt(environmentIndex.Value,
            GetEnvironmentAt(environmentIndex.Value).WithAssigned(identifier, value));
    }

    public void Initialize(string identifier, object? initialValue = null, bool isConst = false)
    {
        if (CurrentEnvironment.Variables.ContainsKey(identifier))
            throw new NotImplementedException();
        CurrentEnvironment = CurrentEnvironment.WithInitialized(identifier, new Variable(initialValue, isConst));
    }

    public void Push(Environment? environment = null)
    {
        _stack.Add(environment ?? new Environment(ImmutableDictionary<string, Variable>.Empty));
    }

    public Environment Pop()
    {
        if (_stack.Count <= 1)
            throw new NotImplementedException();
        var environment = CurrentEnvironment;
        _stack.RemoveAt(_stack.Count - 1);
        return environment;
    }

    public EnvironmentStack Clone()
    {
        return new EnvironmentStack { _stack = new List<Environment>(_stack) };
    }
};

public record Environment(ImmutableDictionary<string, Variable> Variables)
{
    public Environment WithInitialized(string identifier, Variable variable) => this with
    {
        Variables = Variables.Add(identifier, variable)
    };

    public Environment WithAssigned(string identifier, object? value)
    {
        var variable = Variables[identifier];
        if (variable.IsConst)
            throw new NotImplementedException();
        return this with
        {
            Variables = Variables.SetItem(identifier, variable with { Value = value })
        };
    }
}

public record Variable(object? Value, bool IsConst = false);
