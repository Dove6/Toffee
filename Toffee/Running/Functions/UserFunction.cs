﻿using Toffee.SyntacticAnalysis;

namespace Toffee.Running.Functions;

public class UserFunction : IFunction
{
    private readonly FunctionDefinitionExpression _source;
    private readonly EnvironmentStack _closure;

    public UserFunction(FunctionDefinitionExpression source, EnvironmentStack closure)
    {
        _source = source;
        _closure = closure;
    }

    public object? Call(IRunner runner, IList<object?> arguments)
    {
        if (arguments.Count != _source.Parameters.Count)
            throw new NotImplementedException();

        using var closureGuard = _closure.PushGuard();
        // TODO: make sure parameters have unique names
        for (var i = 0; i < _source.Parameters.Count; i++)
        {
            _closure.Initialize(_source.Parameters[i].Name, arguments[i], _source.Parameters[i].IsConst);
            if (!_source.Parameters[i].IsNullAllowed && arguments[i] is null)
                throw new NotImplementedException();
        }

        foreach (var statement in _source.Body.Statements)
            runner.Run(statement, _closure);
        return _source.Body.ResultExpression is not null
            ? runner.Calculate(_source.Body.ResultExpression, _closure)
            : null;
    }
}
