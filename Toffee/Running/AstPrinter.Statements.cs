using Humanizer;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class AstPrinter
{
    public void Print(Statement statement, int indentLevel = 0)
    {
        var startPosition = $"{_inputName}:{statement.StartPosition.Line}:{statement.StartPosition.Column}";
        var endPosition = $"{statement.EndPosition.Line}:{statement.EndPosition.Column}";
        var header = $"{statement.GetType().Name.Humanize(LetterCasing.LowerCase)} [{startPosition} - {endPosition}]";
        Print(header, indentLevel);
        PrintDynamic(statement as dynamic, indentLevel + 1);
    }

    private void Print(VariableInitialization initialization, int indentLevel)
    {
        var mutability = initialization.IsConst ? "immutable" : "mutable";
        Print($"{initialization.Name} ({mutability})", indentLevel);
        if (initialization.InitialValue is null)
            return;
        Print(initialization.InitialValue, indentLevel + 1);
    }

    private void PrintDynamic(NamespaceImportStatement statement, int indentLevel)
    {
        foreach (var level in statement.NamespaceLevels)
            Print(level, indentLevel);
    }

    private void PrintDynamic(VariableInitializationListStatement statement, int indentLevel)
    {
        foreach (var initialization in statement.Items)
            Print(initialization, indentLevel);
    }

    private void PrintDynamic(BreakStatement statement, int indentLevel)
    {
        if (statement.Condition is not null)
            Print(statement.Condition, indentLevel);
    }

    private void PrintDynamic(ReturnStatement statement, int indentLevel)
    {
        if (statement.Value is null)
            return;
        Print(statement.Value, indentLevel);
    }

    private void PrintDynamic(ExpressionStatement statement, int indentLevel)
    {
        Print(statement.Expression, indentLevel);
    }
}
