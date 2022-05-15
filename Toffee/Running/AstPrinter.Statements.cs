using Humanizer;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public static partial class AstPrinter
{
    public static void Print(Statement statement, int indentLevel = 0)
    {
        Print(statement.GetType().Name.Humanize(LetterCasing.LowerCase), indentLevel);
        PrintDynamic(statement as dynamic, indentLevel + 1);
    }

    private static void Print(VariableInitialization initialization, int indentLevel)
    {
        var mutability = initialization.IsConst ? "immutable" : "mutable";
        Print($"{initialization.Name} ({mutability})", indentLevel);
        if (initialization.InitialValue is null)
            return;
        Print(initialization.InitialValue, indentLevel + 1);
    }

    private static void PrintDynamic(Statement statement, int indentLevel)
    {
    }

    private static void PrintDynamic(NamespaceImportStatement statement, int indentLevel)
    {
        foreach (var level in statement.NamespaceLevels)
            Print(level.Name, indentLevel);
    }

    private static void PrintDynamic(VariableInitializationListStatement statement, int indentLevel)
    {
        foreach (var initialization in statement.Items)
            Print(initialization, indentLevel);
    }

    private static void PrintDynamic(BreakIfStatement statement, int indentLevel)
    {
        Print(statement.Condition, indentLevel);
    }

    private static void PrintDynamic(ReturnStatement statement, int indentLevel)
    {
        if (statement.Value is null)
            return;
        Print(statement.Value, indentLevel);
    }

    private static void PrintDynamic(ExpressionStatement statement, int indentLevel)
    {
        Print(statement.Expression, indentLevel);
    }
}
