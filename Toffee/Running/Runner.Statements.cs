using Toffee.Running.Operations;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public void Run(Statement statement, EnvironmentStack? environmentStack = null)
    {
        using var recursionGuard = IncrementRecursionGuarded();
        _currentPosition = statement.StartPosition;
        using var stackBackupGuard = OverwriteEnvironmentStackGuarded(environmentStack);
        try
        {
            RunDynamic(statement as dynamic);
        }
        catch (Exception e) when (IsEntryPoint(recursionGuard))
        {
            var error = e is RunnerException runnerException
                ? runnerException.Error
                : new ExceptionThrown(e.Message);
            EmitError(error with { Position = _currentPosition });
        }
    }

    private void RunDynamic(NamespaceImportStatement statement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(VariableInitializationListStatement statement)
    {
        foreach (var variable in statement.Items)
        {
            object? initialValue = null;
            if (variable.InitialValue is not null)
                initialValue = Calculate(variable.InitialValue);
            _environmentStack.Initialize(variable.Name, initialValue, variable.IsConst);
        }
        _environmentStack.FinalizeInitializationList();
    }

    private void RunDynamic(BreakStatement statement)
    {
        var conditionValue = statement.Condition is not null
            ? Casting.ToBool(Calculate(statement.Condition))
            : true;
        if (conditionValue is true)
            _environmentStack.RegisterBreak();
    }

    private void RunDynamic(ReturnStatement statement)
    {
        var returnValue = statement.Value is not null
            ? Calculate(statement.Value)
            : null;
        _environmentStack.RegisterReturn(returnValue);
    }

    private void RunDynamic(ExpressionStatement statement)
    {
        Calculate(statement.Expression);
    }
}
