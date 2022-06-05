using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public void Run(Statement statement)
    {
        RunDynamic(statement as dynamic);
    }

    private void RunDynamic(Statement statement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(NamespaceImportStatement namespaceImportStatement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(VariableInitializationListStatement variableInitializationListStatement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(BreakStatement breakStatement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(BreakIfStatement breakIfStatement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ReturnStatement returnStatement)
    {
        throw new NotImplementedException();
    }

    private void RunDynamic(ExpressionStatement expressionStatement)
    {
        Calculate(expressionStatement.Expression);
    }
}
