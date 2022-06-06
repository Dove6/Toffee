using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public void Run(Statement statement, EnvironmentStack? environmentStack = null)
    {
        var environmentStackBackup = _environmentStack;
        if (environmentStack is not null)
            _environmentStack = environmentStack;
        try
        {
            RunDynamic(statement as dynamic);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _environmentStack = environmentStackBackup;
        }
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
        _environmentStack.RegisterBreak();
    }

    private void RunDynamic(BreakIfStatement statement)
    {
        // TODO: desugar
        throw new NotImplementedException();
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
