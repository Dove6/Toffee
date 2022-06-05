using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public object? Calculate(Expression expression)
    {
        return CalculateDynamic(expression as dynamic);
    }

    private object? CalculateDynamic(Expression expression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(BlockExpression blockExpression)
    {
        foreach (var statement in blockExpression.Statements)
            Run(statement);
        return blockExpression.ResultExpression is not null
            ? Calculate(blockExpression.ResultExpression)
            : null;
    }

    private object? CalculateDynamic(ConditionalExpression conditionalExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(ForLoopExpression forLoopExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(WhileLoopExpression whileLoopExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(FunctionDefinitionExpression functionDefinitionExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(PatternMatchingExpression patternMatchingExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(GroupingExpression groupingExpression)
    {
        return Calculate(groupingExpression);
    }

    private object? CalculateDynamic(BinaryExpression binaryExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(UnaryExpression unaryExpression)
    {
        var innerResult = Calculate(unaryExpression.Expression);
        if (unaryExpression.Operator == Operator.NumberPromotion)
        {
            // TODO: detect float/int
            var castedResult =  Cast(innerResult, DataType.Integer);
            return castedResult;
        }
        if (unaryExpression.Operator == Operator.ArithmeticNegation)
        {
            // TODO: detect float/int
            var castedResult = Cast(innerResult, DataType.Integer);
            return castedResult switch
            {
                long integerValue => -integerValue,
                double floatValue => -floatValue,
                _ => null
            };
        }
        if (unaryExpression.Operator == Operator.LogicalNegation)
        {
            return Cast(innerResult, DataType.Bool) switch
            {
                bool boolValue => !boolValue,
                _ => null
            };
        }
        throw new ArgumentOutOfRangeException(nameof(unaryExpression.Operator), unaryExpression.Operator, "Invalid operator");
    }

    private object? CalculateDynamic(FunctionCallExpression functionCallExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(IdentifierExpression identifierExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(LiteralExpression literalExpression)
    {
        if (literalExpression.Value is ulong integerValue)
            /* at this point unsigned literal value can be no greater than 9223372036854775807
             * or 9223372036854775808 which overflows to -9223372036854775808 (in case of later negation)
             */
            return unchecked((long)integerValue);
        return literalExpression.Value;
    }

    private object? CalculateDynamic(TypeCastExpression typeCastExpression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(TypeExpression typeExpression)
    {
        throw new NotImplementedException();
    }
}
