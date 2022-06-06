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
        Print(conditional.Condition!, indentLevel + 1);
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
            Print("regular statements", indentLevel + 1);
            Print(substatement, indentLevel + 2);
        }
        if (expression.ResultExpression is null)
            return;
        Print("result expression", indentLevel + 1);
        Print(expression.ResultExpression, indentLevel + 2);
    }

    private void PrintDynamic(ConditionalExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
        Print("if", indentLevel + 1);
        Print(expression.Branches[0], indentLevel + 2);
        foreach (var elifPart in expression.Branches.Skip(1))
        {
            Print("elif", indentLevel + 1);
            Print(elifPart, indentLevel + 2);
        }
        if (expression.ElseBranch is null)
            return;
        Print("else", indentLevel + 1);
        Print(expression.ElseBranch, indentLevel + 2);
    }

    private void PrintDynamic(ForLoopExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel);
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
        Print(expression.Callee, indentLevel + 2);
        foreach (var argument in expression.Arguments)
        {
            Print("argument", indentLevel + 1);
            Print(argument, indentLevel + 2);
        }
    }

    private void PrintDynamic(IdentifierExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.Name);
        if (expression.NamespaceLevels.Count > 0)
            Print($"namespace: {string.Join('.', expression.NamespaceLevels)}", indentLevel + 1);
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
        PrintHeader(expression, indentLevel, expression.Type.ToString());
        Print(expression.Expression, indentLevel + 1);
    }

    private void PrintDynamic(TypeCheckExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.IsInequalityCheck
            ? $"is not {expression.Type}" : $"is {expression.Type}");
        Print(expression.Expression, indentLevel + 1);
    }

    private void PrintDynamic(PatternTypeCheckExpression expression, int indentLevel)
    {
        PrintHeader(expression, indentLevel, expression.IsInequalityCheck
            ? $"is not {expression.Type}" : $"is {expression.Type}");
    }
}
