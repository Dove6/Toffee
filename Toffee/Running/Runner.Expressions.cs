using Toffee.Running.Operations;
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
        // TODO: describe order of evaluation
        var leftResult = Calculate(binaryExpression.Left);
        var rightResult = Calculate(binaryExpression.Right);

        if (binaryExpression.Operator == Operator.NamespaceAccess)
            throw new NotImplementedException();  // TODO: little sense

        if (binaryExpression.Operator == Operator.Addition)
            return Arithmetical.Add(CastToNumber(leftResult), CastToNumber(rightResult));
        if (binaryExpression.Operator == Operator.Subtraction)
            return Arithmetical.Subtract(CastToNumber(leftResult), CastToNumber(rightResult));
        if (binaryExpression.Operator == Operator.Multiplication)
            return Arithmetical.Multiply(CastToNumber(leftResult), CastToNumber(rightResult));
        if (binaryExpression.Operator == Operator.Division)
            return Arithmetical.Divide(CastToNumber(leftResult), CastToNumber(rightResult));
        if (binaryExpression.Operator == Operator.Remainder)
            return Arithmetical.Remainder(CastToNumber(leftResult), CastToNumber(rightResult));
        if (binaryExpression.Operator == Operator.Exponentiation)
            return Arithmetical.Exponentiate(CastToNumber(leftResult), CastToNumber(rightResult));

        if (binaryExpression.Operator == Operator.Concatenation)
            return Character.Concatenate(Cast(leftResult, DataType.String), Cast(rightResult, DataType.String));

        if (binaryExpression.Operator == Operator.LessThanComparison)
            return Relational.IsLessThan(leftResult, rightResult);
        if (binaryExpression.Operator == Operator.LessOrEqualComparison)
            return Relational.IsLessOrEqual(leftResult, rightResult);
        if (binaryExpression.Operator == Operator.GreaterThanComparison)
            return Relational.IsGreaterThan(leftResult, rightResult);
        if (binaryExpression.Operator == Operator.GreaterOrEqualComparison)
            return Relational.IsGreaterOrEqual(leftResult, rightResult);
        if (binaryExpression.Operator == Operator.EqualComparison)
            return Relational.IsEqualTo(leftResult, rightResult);
        if (binaryExpression.Operator == Operator.NotEqualComparison)
            return Relational.IsNotEqualTo(leftResult, rightResult);

        // TODO: little sense here
        if (binaryExpression.Operator == Operator.EqualTypeCheck)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.NotEqualTypeCheck)
            throw new NotImplementedException();

        if (binaryExpression.Operator == Operator.Disjunction)
            return Logical.Disjoin(Cast(leftResult, DataType.Bool), Cast(rightResult, DataType.Bool));
        if (binaryExpression.Operator == Operator.Conjunction)
            return Logical.Conjoin(Cast(leftResult, DataType.Bool), Cast(rightResult, DataType.Bool));

        // TODO: check if it makes sense
        if (binaryExpression.Operator == Operator.PatternMatchingDisjunction)
            return Logical.Disjoin(Cast(leftResult, DataType.Bool), Cast(rightResult, DataType.Bool));
        if (binaryExpression.Operator == Operator.PatternMatchingConjunction)
            return Logical.Conjoin(Cast(leftResult, DataType.Bool), Cast(rightResult, DataType.Bool));

        if (binaryExpression.Operator == Operator.NullCoalescing)
            return leftResult ?? rightResult;
        if (binaryExpression.Operator == Operator.NullSafePipe)
            throw new NotImplementedException();

        if (binaryExpression.Operator == Operator.Assignment)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.AdditionAssignment)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.SubtractionAssignment)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.MultiplicationAssignment)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.DivisionAssignment)
            throw new NotImplementedException();
        if (binaryExpression.Operator == Operator.RemainderAssignment)
            throw new NotImplementedException();

        throw new ArgumentOutOfRangeException(nameof(binaryExpression.Operator), binaryExpression.Operator, "Invalid operator");
    }

    private object? CalculateDynamic(UnaryExpression unaryExpression)
    {
        var innerResult = Calculate(unaryExpression.Expression);

        if (unaryExpression.Operator == Operator.NumberPromotion)
            return CastToNumber(innerResult);
        if (unaryExpression.Operator == Operator.ArithmeticNegation)
            return Arithmetical.Negate(CastToNumber(innerResult));

        if (unaryExpression.Operator == Operator.LogicalNegation)
            return Logical.Negate(Cast(innerResult, DataType.Bool));

        // TODO: little sense here
        if (unaryExpression.Operator == Operator.PatternMatchingLessThanComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingLessOrEqualComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingGreaterThanComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingGreaterOrEqualComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingEqualComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingNotEqualComparison)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingEqualTypeCheck)
            throw new NotImplementedException();
        if (unaryExpression.Operator == Operator.PatternMatchingNotEqualTypeCheck)
            throw new NotImplementedException();

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
