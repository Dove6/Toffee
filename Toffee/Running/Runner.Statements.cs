using Toffee.Running.Operations;
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
        catch (RunnerException e)
        {
            Console.WriteLine(e.Error);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            _environmentStack = environmentStackBackup;
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
