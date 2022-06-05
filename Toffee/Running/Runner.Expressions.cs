using Toffee.Running.Operations;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public object? Calculate(Expression expression, EnvironmentStack? environmentStack)
    {
        return CalculateDynamic(expression as dynamic, environmentStack ?? new EnvironmentStack());
    }

    private object? CalculateDynamic(Expression expression)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(BlockExpression expression, EnvironmentStack environmentStack)
    {
        environmentStack.Push();
        foreach (var statement in expression.Statements)
            Run(statement, environmentStack);
        var result = expression.ResultExpression is not null
            ? Calculate(expression.ResultExpression, environmentStack)
            : null;
        environmentStack.Pop();
        return result;
    }

    private object? CalculateDynamic(ConditionalExpression expression, EnvironmentStack environmentStack)
    {
        var consequentToRun = expression.Branches.FirstOrDefault(x =>
        {
            var conditionValue = Calculate(x.Condition, environmentStack);
            return Casting.ToBool(conditionValue) is true;
        })?.Consequent;
        consequentToRun ??= expression.ElseBranch;
        return consequentToRun is not null
            ? Calculate(consequentToRun, environmentStack)
            : null;
    }

    private object? CalculateDynamic(ForLoopExpression expression, EnvironmentStack environmentStack)
    {
        object? startValue = 0L;
        object? stepValue = 1L;
        var stopValue = Casting.ToNumber(Calculate(expression.Range.PastTheEnd, environmentStack));

        if (expression.Range.Start is not null)
            startValue = Casting.ToNumber(Calculate(expression.Range.Start, environmentStack));
        if (expression.Range.Step is not null)
            stepValue = Casting.ToNumber(Calculate(expression.Range.Step, environmentStack));
        if (startValue is double || stepValue is double || stopValue is double)
        {
            startValue = Casting.ToFloat(startValue);
            stepValue = Casting.ToFloat(stepValue);
            stopValue = Casting.ToFloat(stopValue);
        }

        if (startValue is null || stepValue is null || stopValue is null)
            throw new NotImplementedException();

        var counter = startValue;
        Func<object?, object?, bool?> rangePredicate = Relational.IsGreaterThan(stepValue, 0L) is true
            ? Relational.IsLessThan
            : Relational.IsGreaterThan;

        while (rangePredicate(counter, stopValue) is true)
        {
            Calculate(expression.Body, environmentStack);
            counter = Arithmetical.Add(counter, stepValue);
        }
        return counter;
    }

    private object? CalculateDynamic(WhileLoopExpression expression, EnvironmentStack environmentStack)
    {
        while (Casting.ToBool(Calculate(expression.Condition, environmentStack)) is true)
            Calculate(expression.Body, environmentStack);
        return Calculate(expression.Condition, environmentStack);
    }

    private object? CalculateDynamic(FunctionDefinitionExpression expression, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(PatternMatchingExpression expression, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(GroupingExpression expression, EnvironmentStack environmentStack)
    {
        return Calculate(expression, environmentStack);
    }

    private object? CalculateDynamic(BinaryExpression expression, EnvironmentStack environmentStack)
    {
        // TODO: describe order of evaluation
        var leftResult = Calculate(expression.Left, environmentStack);
        var rightResult = Calculate(expression.Right, environmentStack);

        if (expression.Operator == Operator.NamespaceAccess)
            throw new NotImplementedException();  // TODO: little sense

        if (expression.Operator == Operator.Addition)
            return Arithmetical.Add(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));
        if (expression.Operator == Operator.Subtraction)
            return Arithmetical.Subtract(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));
        if (expression.Operator == Operator.Multiplication)
            return Arithmetical.Multiply(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));
        if (expression.Operator == Operator.Division)
            return Arithmetical.Divide(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));
        if (expression.Operator == Operator.Remainder)
            return Arithmetical.Remainder(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));
        if (expression.Operator == Operator.Exponentiation)
            return Arithmetical.Exponentiate(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult));

        if (expression.Operator == Operator.Concatenation)
            return Character.Concatenate(Casting.ToString(leftResult), Casting.ToString(rightResult));

        if (expression.Operator == Operator.LessThanComparison)
            return Relational.IsLessThan(leftResult, rightResult);
        if (expression.Operator == Operator.LessOrEqualComparison)
            return Relational.IsLessOrEqual(leftResult, rightResult);
        if (expression.Operator == Operator.GreaterThanComparison)
            return Relational.IsGreaterThan(leftResult, rightResult);
        if (expression.Operator == Operator.GreaterOrEqualComparison)
            return Relational.IsGreaterOrEqual(leftResult, rightResult);
        if (expression.Operator == Operator.EqualComparison)
            return Relational.IsEqualTo(leftResult, rightResult);
        if (expression.Operator == Operator.NotEqualComparison)
            return Relational.IsNotEqualTo(leftResult, rightResult);

        if (expression.Operator == Operator.Disjunction)
            return Logical.Disjoin(Casting.ToBool(leftResult), Casting.ToBool(rightResult));
        if (expression.Operator == Operator.Conjunction)
            return Logical.Conjoin(Casting.ToBool(leftResult), Casting.ToBool(rightResult));

        // TODO: check if it makes sense
        if (expression.Operator == Operator.PatternMatchingDisjunction)
            return Logical.Disjoin(Casting.ToBool(leftResult), Casting.ToBool(rightResult));
        if (expression.Operator == Operator.PatternMatchingConjunction)
            return Logical.Conjoin(Casting.ToBool(leftResult), Casting.ToBool(rightResult));

        if (expression.Operator == Operator.NullCoalescing)
            return leftResult ?? rightResult;
        if (expression.Operator == Operator.NullSafePipe)
            throw new NotImplementedException();

        if (expression.Operator is Operator.Assignment or Operator.AdditionAssignment or Operator.SubtractionAssignment
                or Operator.MultiplicationAssignment or Operator.DivisionAssignment or Operator.RemainderAssignment)
            return PerformAssignment(expression.Left, rightResult, expression.Operator, environmentStack);

        throw new ArgumentOutOfRangeException(nameof(expression.Operator), expression.Operator, "Invalid operator");
    }

    private object? PerformAssignment(Expression left, object? right, Operator assignmentOperator, EnvironmentStack environmentStack)
    {
        if (left is not IdentifierExpression identifierExpression)
            throw new NotImplementedException();

        var valueToAssign = assignmentOperator switch
        {
            Operator.Assignment => right,
            Operator.AdditionAssignment => Arithmetical.Add(environmentStack.Access(identifierExpression.Name), right),
            Operator.SubtractionAssignment => Arithmetical.Subtract(environmentStack.Access(identifierExpression.Name), right),
            Operator.MultiplicationAssignment => Arithmetical.Multiply(environmentStack.Access(identifierExpression.Name), right),
            Operator.DivisionAssignment => Arithmetical.Divide(environmentStack.Access(identifierExpression.Name), right),
            Operator.RemainderAssignment => Arithmetical.Remainder(environmentStack.Access(identifierExpression.Name), right),
            _ => throw new NotSupportedException()
        };
        environmentStack.Assign(identifierExpression.Name, valueToAssign);
        return valueToAssign;
    }

    private object? CalculateDynamic(UnaryExpression expression, EnvironmentStack environmentStack)
    {
        var innerResult = Calculate(expression.Expression, environmentStack);

        if (expression.Operator == Operator.NumberPromotion)
            return Casting.ToNumber(innerResult);
        if (expression.Operator == Operator.ArithmeticNegation)
            return Arithmetical.Negate(Casting.ToNumber(innerResult));

        if (expression.Operator == Operator.LogicalNegation)
            return Logical.Negate(Casting.ToBool(innerResult));

        // TODO: little sense here
        if (expression.Operator == Operator.PatternMatchingLessThanComparison)
            throw new NotImplementedException();
        if (expression.Operator == Operator.PatternMatchingLessOrEqualComparison)
            throw new NotImplementedException();
        if (expression.Operator == Operator.PatternMatchingGreaterThanComparison)
            throw new NotImplementedException();
        if (expression.Operator == Operator.PatternMatchingGreaterOrEqualComparison)
            throw new NotImplementedException();
        if (expression.Operator == Operator.PatternMatchingEqualComparison)
            throw new NotImplementedException();
        if (expression.Operator == Operator.PatternMatchingNotEqualComparison)
            throw new NotImplementedException();

        throw new ArgumentOutOfRangeException(nameof(expression.Operator), expression.Operator, "Invalid operator");
    }

    private object? CalculateDynamic(FunctionCallExpression expression, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }

    private object? CalculateDynamic(IdentifierExpression expression, EnvironmentStack environmentStack)
    {
        if (expression.NamespaceLevels.Count > 0)
            throw new NotImplementedException();
        return environmentStack.Access(expression.Name);
    }

    private object? CalculateDynamic(LiteralExpression expression, EnvironmentStack environmentStack)
    {
        if (expression.Value is ulong integerValue)
            /* at this point unsigned literal value can be no greater than 9223372036854775807
             * or 9223372036854775808 which overflows to -9223372036854775808 (in case of later negation)
             */
            return unchecked((long)integerValue);
        return expression.Value;
    }

    private object? CalculateDynamic(TypeCastExpression expression, EnvironmentStack environmentStack)
    {
        var value = Calculate(expression.Expression, environmentStack);
        return Casting.To(value, expression.Type);
    }

    private object? CalculateDynamic(TypeCheckExpression expression, EnvironmentStack environmentStack)
    {
        var value = Calculate(expression.Expression, environmentStack);
        return expression.IsInequalityCheck ^ value switch
        {
            null => expression.Type == DataType.Null,
            long => expression.Type == DataType.Integer,
            double => expression.Type == DataType.Float,
            string => expression.Type == DataType.String,
            bool => expression.Type == DataType.Bool,
            _ => throw new NotImplementedException()
        };
    }

    private object? CalculateDynamic(PatternTypeCheckExpression expression, EnvironmentStack environmentStack)
    {
        throw new NotImplementedException();
    }
}
