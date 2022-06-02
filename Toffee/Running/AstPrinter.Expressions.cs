using System.Globalization;
using Humanizer;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class AstPrinter
{
    public void Print(Expression expression, int indentLevel = 0)
    {
        PrintDynamic(expression as dynamic, indentLevel);
    }

    private void PrintHeader(Expression expression, int indentLevel = 0, string? details = null)
    {
        var startPosition = $"{_inputName}:{expression.StartPosition.Line}:{expression.StartPosition.Column}";
        var endPosition = $"{expression.EndPosition.Line}:{expression.EndPosition.Column}";
        var header = $"{expression.GetType().Name.Humanize(LetterCasing.LowerCase)}";
        if (!string.IsNullOrEmpty(details))
            header += $" ({details})";
        header += $" [{startPosition} - {endPosition}]";
        Print(header, indentLevel);
    }

    private void Print(ConditionalElement conditional, int indentLevel)
    {
        Print("condition", indentLevel);
        Print(conditional.Condition, indentLevel + 1);
        Print("consequent", indentLevel);
        Print(conditional.Consequent, indentLevel + 1);
    }

    private void Print(ForLoopRange range, int indentLevel)
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

    private void Print(PatternMatchingBranch branch, int indentLevel)
    {
        Print("pattern", indentLevel);
        if (branch.Pattern is null)
            Print("default", indentLevel + 1);
        else
            Print(branch.Pattern, indentLevel + 1);
        Print("consequent", indentLevel);
        Print(branch.Consequent, indentLevel + 1);
    }

    private static void Print(DataType type, int indentLevel)
    {
        Print($"type: {type.Humanize(LetterCasing.LowerCase)}", indentLevel);
    }

    private static void PrintDynamic(Expression expression, int indentLevel)
    {
    }

    private void PrintDynamic(BlockExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        foreach (var substatement in expression.Statements)
        {
            Print("terminated", indentLevel + 1);
            Print(substatement, indentLevel + 2);
        }
        if (expression.UnterminatedStatement is null)
            return;
        Print("unterminated", indentLevel + 1);
        Print(expression.UnterminatedStatement, indentLevel + 2);
    }

    private void PrintDynamic(ConditionalExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print("if", indentLevel + 1);
        Print(expression.IfPart, indentLevel + 2);
        foreach (var elifPart in expression.ElifParts)
        {
            Print("elif", indentLevel + 1);
            Print(elifPart, indentLevel + 2);
        }
        if (expression.ElsePart is null)
            return;
        Print("else", indentLevel + 1);
        Print(expression.ElsePart, indentLevel + 2);
    }

    private void PrintDynamic(ForLoopExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print(expression.CounterName is not null ? $"counter: {expression.CounterName}" : "no counter",
            indentLevel + 1);
        Print("range", indentLevel + 1);
        Print(expression.Range, indentLevel + 2);
        Print("body", indentLevel + 1);
        Print(expression.Body, indentLevel + 2);
    }

    private void PrintDynamic(WhileLoopExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print("condition", indentLevel + 1);
        Print(expression.Condition, indentLevel + 2);
        Print("body", indentLevel + 1);
        Print(expression.Body, indentLevel + 2);
    }

    private void PrintDynamic(FunctionDefinitionExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print($"{(expression.Parameters.Count == 0 ? "no " : "")}parameters", indentLevel + 1);
        foreach (var parameter in expression.Parameters)
            Print(parameter, indentLevel + 2);
        Print("body", indentLevel + 1);
        Print(expression.Body, indentLevel + 2);
    }

    private void PrintDynamic(PatternMatchingExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print("argument", indentLevel + 1);
        Print(expression.Argument, indentLevel + 2);
        foreach (var branch in expression.Branches)
        {
            Print("branch", indentLevel + 1);
            Print(branch, indentLevel + 2);
        }
        if (expression.Default is null)
            return;
        Print("default", indentLevel + 1);
        Print(expression.Default, indentLevel + 2);
    }

    private void PrintDynamic(GroupingExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print(expression.Expression, indentLevel + 1);
    }

    private void PrintDynamic(BinaryExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.Operator.ToString().Humanize(LetterCasing.LowerCase));
        Print(expression.Left, indentLevel + 1);
        Print(expression.Right, indentLevel + 1);
    }

    private void PrintDynamic(UnaryExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.Operator.ToString().Humanize(LetterCasing.LowerCase));
        Print(expression.Expression, indentLevel + 1);
    }

    private void PrintDynamic(FunctionCallExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print("expression", indentLevel + 1);
        Print(expression.Expression, indentLevel + 2);
        foreach (var argument in expression.Arguments)
        {
            Print("argument", indentLevel + 1);
            Print(argument, indentLevel + 2);
        }
    }

    private void PrintDynamic(IdentifierExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.Name);
    }

    private void PrintDynamic(LiteralExpression expression, int indentLevel)
    {
        var description = expression.Type switch
        {
            DataType.Null => null,
            DataType.String => $"\"{expression.Value}\"",
            DataType.Float => ((double)expression.Value!).ToString(CultureInfo.InvariantCulture),
            DataType.Integer => expression.Value!.ToString(),
            DataType.Bool => expression.Value is true ? "true" : "false",
            _ => throw new ArgumentOutOfRangeException(nameof(expression.Type), expression.Type, null)
        };
        if (description is not null)
            description = $": {description}";
        PrintHeader(expression, indentLevel, $"{expression.Type.Humanize(LetterCasing.LowerCase)}{description}");
    }

    private void PrintDynamic(TypeCastExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print(expression.Type, indentLevel + 1);
        Print(expression.Expression, indentLevel + 1);
    }

    private void PrintDynamic(TypeExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.Type.Humanize(LetterCasing.LowerCase));
    }
}
