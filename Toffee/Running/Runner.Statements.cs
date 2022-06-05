using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public void Run(Statement statement, EnvironmentStack? environmentStack = null)
    {
        var environmentStackBackup = _environmentStack;
        if (environmentStack is not null)
            _environmentStack = environmentStack;
        RunDynamic(statement as dynamic);
        _environmentStack = environmentStackBackup;
    }

    private void RunDynamic(Statement statement)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    private void RunDynamic(BreakIfStatement statement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ReturnStatement statement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ExpressionStatement statement)
    {
        Calculate(statement.Expression);
    }
}
