using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public void Run(Statement statement, EnvironmentStack? environmentStack)
    {
        RunDynamic(statement as dynamic, environmentStack ?? new EnvironmentStack());
    }

    private void RunDynamic(Statement statement, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(NamespaceImportStatement statement, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(VariableInitializationListStatement statement, EnvironmentStack environmentStack)
    {
        foreach (var variable in statement.Items)
        {
            object? initialValue = null;
            if (variable.InitialValue is not null)
                initialValue = Calculate(variable.InitialValue, environmentStack);
            environmentStack.Initialize(variable.Name, initialValue, variable.IsConst);
        }
    }

    private void RunDynamic(BreakStatement statement, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(BreakIfStatement statement, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ReturnStatement statement, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ExpressionStatement statement, EnvironmentStack environmentStack)
    {
        Calculate(statement.Expression, environmentStack);
    }
}
