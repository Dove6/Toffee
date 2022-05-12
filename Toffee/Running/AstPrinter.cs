using Humanizer;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public static class AstPrinter
{
    private static void Print(string text, int indentLevel = 0)
    {
        var indentation = new string(' ', indentLevel * 2);
        text.Split(new[] { '\r', '\n', '\xe1' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            .ForEach(x =>
            {
                Console.Write(indentation);
                Console.WriteLine(x);
            });
    }

    public static void Print(IStatement statement, int indentLevel = 0)
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
        Print("initial value", indentLevel + 1);
        Print(initialization.InitialValue, indentLevel + 2);
    }

    private static void PrintDynamic(IStatement statement, int indentLevel)
    { }

    private static void PrintDynamic(VariableInitializationListStatement statement, int indentLevel)
    {
        Print("items", indentLevel);
        foreach (var initialization in statement.Items)
            Print(initialization, indentLevel + 1);
    }

    private static void PrintDynamic(BreakIfStatement statement, int indentLevel)
    {
        Print("condition", indentLevel);
        Print(statement.Condition, indentLevel + 1);
    }

    private static void PrintDynamic(ReturnStatement statement, int indentLevel)
    {
        if (statement.Value is null)
            return;
        Print("value", indentLevel);
        Print(statement.Value, indentLevel + 1);
    }

    private static void PrintDynamic(ExpressionStatement statement, int indentLevel)
    {
        Print("expression", indentLevel);
        Print(statement.Expression, indentLevel + 1);
    }

    public static void Print(IExpression expression, int indentLevel = 0)
    {
        Print(expression.GetType().Name.Humanize(LetterCasing.LowerCase), indentLevel);
        PrintDynamic(expression as dynamic, indentLevel + 1);
    }

    private static void Print(ConditionalElement conditional, int indentLevel)
    {
        Print("condition", indentLevel);
        Print(conditional.Condition, indentLevel + 1);
        Print("consequent", indentLevel);
        Print(conditional.Consequent, indentLevel + 1);
    }

    private static void Print(ForLoopRange range, int indentLevel)
    {
        if (range.Start is not null)
        {
            Print("start", indentLevel);
            Print(range.Start, indentLevel + 1);
        }
        Print("stop", indentLevel);
        Print(range.PastTheEnd, indentLevel + 1);
        if (range.Step is null)
            return;
        Print("step", indentLevel);
        Print(range.Step, indentLevel + 1);
    }

    private static void Print(FunctionParameter parameter, int indentLevel)
    {
        var mutability = parameter.IsConst ? "immutable" : "mutable";
        var optionality = parameter.IsNullAllowed ? "nullable" : "not nullable";
        Print($"{parameter.Name} ({mutability}, {optionality})", indentLevel);
    }

    private static void Print(PatternMatchingBranch branch, int indentLevel)
    {
        Print("pattern", indentLevel);
        Print(branch.Pattern, indentLevel + 1);
        Print("consequent", indentLevel);
        Print(branch.Consequent, indentLevel + 1);
    }

    private static void Print(Operator @operator, int indentLevel)
    {
        Print($"operator: {@operator.ToString().Humanize(LetterCasing.LowerCase)}", indentLevel);
    }

    private static void PrintDynamic(IExpression expression, int indentLevel)
    { }

    private static void PrintDynamic(BlockExpression expression, int indentLevel)
    {
        Print("terminated", indentLevel);
        foreach (var substatement in expression.Statements)
            Print(substatement, indentLevel + 1);
        if (expression.UnterminatedStatement is null)
            return;
        Print("unterminated", indentLevel);
        Print(expression.UnterminatedStatement, indentLevel + 1);
    }

    private static void PrintDynamic(ConditionalExpression expression, int indentLevel)
    {
        Print("if", indentLevel);
        Print(expression.IfPart, indentLevel + 1);
        foreach (var elifPart in expression.ElifParts)
        {
            Print("elif", indentLevel);
            Print(elifPart, indentLevel + 1);
        }
        if (expression.ElsePart is null)
            return;
        Print("else", indentLevel);
        Print(expression.ElsePart, indentLevel + 1);
    }

    private static void PrintDynamic(ForLoopExpression expression, int indentLevel)
    {
        Print(expression.CounterName is not null ? $"counter: {expression.CounterName}" : "no counter",
            indentLevel);
        Print("range", indentLevel);
        Print(expression.Range, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private static void PrintDynamic(WhileLoopExpression expression, int indentLevel)
    {
        Print("condition", indentLevel);
        Print(expression.Condition, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private static void PrintDynamic(FunctionDefinitionExpression expression, int indentLevel)
    {
        Print($"{(expression.Parameters.Count == 0 ? "no " : "")}parameters", indentLevel);
        foreach (var parameter in expression.Parameters)
            Print(parameter, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private static void PrintDynamic(PatternMatchingExpression expression, int indentLevel)
    {
        Print("argument", indentLevel);
        Print(expression.Argument, indentLevel + 1);
        foreach (var branch in expression.Branches)
        {
            Print("branch", indentLevel);
            Print(branch, indentLevel + 1);
        }
        if (expression.Default is null)
            return;
        Print("default", indentLevel);
        Print(expression.Default, indentLevel);
    }

    private static void PrintDynamic(BinaryExpression expression, int indentLevel)
    {
        Print(expression.Operator, indentLevel);
        Print("left", indentLevel);
        Print(expression.Left, indentLevel + 1);
        Print("right", indentLevel);
        Print(expression.Right, indentLevel + 1);
    }

    private static void PrintDynamic(UnaryExpression expression, int indentLevel)
    {
        Print(expression.Operator, indentLevel);
        Print("expression", indentLevel);
        Print(expression.Expression, indentLevel + 1);
    }

    private static void PrintDynamic(FunctionCallExpression expression, int indentLevel)
    {
        Print("name", indentLevel);
        Print(expression.Name, indentLevel + 1);
        foreach (var argument in expression.Arguments)
        {
            Print("argument", indentLevel);
            Print(argument, indentLevel + 1);
        }
    }

    private static void PrintDynamic(IdentifierExpression expression, int indentLevel)
    {
        Print(expression.Name, indentLevel);
    }

    private static void PrintDynamic(LiteralExpression expression, int indentLevel)
    {
        Print($"{expression.Value}", indentLevel);
    }
}
