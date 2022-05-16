using System.Globalization;
using Humanizer;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class AstPrinter
{
    public void Print(Expression expression, int indentLevel = 0)
    {
        var position = $"{_inputName}:{expression.Position.Line}:{expression.Position.Column}";
        var header = $"{expression.GetType().Name.Humanize(LetterCasing.LowerCase)} [{position}]";
        Print(header, indentLevel);
        PrintDynamic(expression as dynamic, indentLevel + 1);
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

    private static void Print(Operator @operator, int indentLevel)
    {
        Print($"operator: {@operator.ToString().Humanize(LetterCasing.LowerCase)}", indentLevel);
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
        foreach (var substatement in expression.Statements)
        {
            Print("terminated", indentLevel);
            Print(substatement, indentLevel + 1);
        }
        if (expression.UnterminatedStatement is null)
            return;
        Print("unterminated", indentLevel);
        Print(expression.UnterminatedStatement, indentLevel + 1);
    }

    private void PrintDynamic(ConditionalExpression expression, int indentLevel)
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

    private void PrintDynamic(ForLoopExpression expression, int indentLevel)
    {
        Print(expression.CounterName is not null ? $"counter: {expression.CounterName}" : "no counter",
            indentLevel);
        Print("range", indentLevel);
        Print(expression.Range, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private void PrintDynamic(WhileLoopExpression expression, int indentLevel)
    {
        Print("condition", indentLevel);
        Print(expression.Condition, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private void PrintDynamic(FunctionDefinitionExpression expression, int indentLevel)
    {
        Print($"{(expression.Parameters.Count == 0 ? "no " : "")}parameters", indentLevel);
        foreach (var parameter in expression.Parameters)
            Print(parameter, indentLevel + 1);
        Print("body", indentLevel);
        Print(expression.Body, indentLevel + 1);
    }

    private void PrintDynamic(PatternMatchingExpression expression, int indentLevel)
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

    private void PrintDynamic(GroupingExpression expression, int indentLevel)
    {
        Print(expression.Expression, indentLevel);
    }

    private void PrintDynamic(BinaryExpression expression, int indentLevel)
    {
        // TODO: shorter form (operator)
        Print(expression.Operator, indentLevel);
        Print(expression.Left, indentLevel);
        Print(expression.Right, indentLevel);
    }

    private void PrintDynamic(UnaryExpression expression, int indentLevel)
    {
        // TODO: shorter form (operator)
        Print(expression.Operator, indentLevel);
        Print(expression.Expression, indentLevel);
    }

    private void PrintDynamic(FunctionCallExpression expression, int indentLevel)
    {
        Print("expression", indentLevel);
        Print(expression.Expression, indentLevel + 1);
        foreach (var argument in expression.Arguments)
        {
            Print("argument", indentLevel);
            Print(argument, indentLevel + 1);
        }
    }

    private static void PrintDynamic(IdentifierExpression expression, int indentLevel)
    {
        // TODO: shorter form (name)
        Print(expression.Name, indentLevel);
    }

    private static void PrintDynamic(LiteralExpression expression, int indentLevel)
    {
        // TODO: shorter form (type, value)
        var description = expression.Type switch
        {
            DataType.Null => null,
            DataType.String => $"\"{expression.Value}\"",
            DataType.Float => ((float)expression.Value!).ToString(CultureInfo.InvariantCulture),
            DataType.Integer => expression.Value!.ToString(),
            DataType.Bool => expression.Value is true ? "true" : "false",
            // TODO: exclude function from literal types
            _ => throw new ArgumentOutOfRangeException(nameof(expression.Type), expression.Type, null)
        };
        if (description is not null)
            description = $": {description}";
        Print($"{expression.Type.Humanize(LetterCasing.LowerCase)}{description}", indentLevel);
    }

    private void PrintDynamic(TypeCastExpression expression, int indentLevel)
    {
        Print(expression.Type, indentLevel);
        Print(expression.Expression, indentLevel);
    }

    private static void PrintDynamic(TypeExpression expression, int indentLevel)
    {
        // TODO: shorter form (type)
        Print(expression.Type, indentLevel);
    }
}
