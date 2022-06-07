using Toffee.Running.Functions;
using Toffee.Running.Operations;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public partial class Runner
{
    public object? Calculate(Expression expression, EnvironmentStack? environmentStack = null)
    {
        _currentPosition = expression.StartPosition;
        var environmentStackBackup = _environmentStack;
        if (environmentStack is not null)
            _environmentStack = environmentStack;
        try
        {
            var result = CalculateDynamic(expression as dynamic);
            return result;
        }
        catch (RunnerException e)
        {
            EmitError(e.Error with { Position = _currentPosition });
        }
        catch (Exception e)
        {
            EmitError(new ExceptionThrown(e.Message) { Position = _currentPosition });
        }
        finally
        {
            _environmentStack = environmentStackBackup;
        }
        return null;
    }

    private object? CalculateDynamic(BlockExpression expression)
    {
        using var environmentGuard = _environmentStack.PushGuard();
        foreach (var statement in expression.Statements)
        {
            Run(statement);
            if (ExecutionInterrupted)
                return null;
        }
        var result = expression.ResultExpression is not null
            ? Calculate(expression.ResultExpression)
            : null;
        return result;
    }

    private object? CalculateWithoutPushingEnvironment(BlockExpression expression)
    {
        foreach (var statement in expression.Statements)
        {
            Run(statement);
            if (ExecutionInterrupted)
                return null;
        }
        var result = expression.ResultExpression is not null
            ? Calculate(expression.ResultExpression)
            : null;
        return result;
    }

    private object? CalculateDynamic(ConditionalExpression expression)
    {
        var consequentToRun = expression.Branches.FirstOrDefault(x =>
        {
            var conditionValue = Calculate(x.Condition!);
            return Casting.ToBool(conditionValue) is true;
        })?.Consequent;
        consequentToRun ??= expression.ElseBranch;
        return consequentToRun is not null
            ? Calculate(consequentToRun)
            : null;
    }

    private object? CalculateDynamic(ForLoopExpression expression)
    {
        object? startValue = 0L;
        object? stepValue = 1L;
        var stopValue = Casting.ToNumber(Calculate(expression.Range.PastTheEnd));

        if (expression.Range.Start is not null)
            startValue = Casting.ToNumber(Calculate(expression.Range.Start));
        if (expression.Range.Step is not null)
            stepValue = Casting.ToNumber(Calculate(expression.Range.Step));
        if (startValue is double || stepValue is double || stopValue is double)
        {
            startValue = Casting.ToFloat(startValue);
            stepValue = Casting.ToFloat(stepValue);
            stopValue = Casting.ToFloat(stopValue);
        }

        if (startValue is null || stepValue is null || stopValue is null)
            throw new RunnerException(new NullInForLoopRange(startValue is null
                ? nameof(expression.Range.Start)
                : stopValue is null
                    ? nameof(expression.Range.PastTheEnd)
                    : nameof(expression.Range.Step)));

        var counter = startValue;
        Func<object?, object?, bool?> rangePredicate = Relational.IsGreaterThan(stepValue, 0L) is true
            ? Relational.IsLessThan
            : Relational.IsGreaterThan;

        using var environmentGuard = _environmentStack.PushGuard(EnvironmentType.Loop);

        if (expression.CounterName is not null)
            _environmentStack.Initialize(expression.CounterName, isConst: true);

        while (rangePredicate(counter, stopValue) is true)
        {
            if (expression.CounterName is not null)
                _environmentStack.Assign(expression.CounterName, counter, true);
            CalculateWithoutPushingEnvironment(expression.Body);
            if (ExecutionInterrupted)
                return counter;
            counter = Arithmetical.Add(counter, stepValue);
        }

        return counter;
    }

    private object? CalculateDynamic(WhileLoopExpression expression)
    {
        object? conditionValue;
        using var environmentGuard = _environmentStack.PushGuard(EnvironmentType.Loop);
        while (Casting.ToBool(conditionValue = Calculate(expression.Condition)) is true)
        {
            CalculateWithoutPushingEnvironment(expression.Body);
            if (ExecutionInterrupted)
                return conditionValue;
        }
        return conditionValue;
    }

    private IFunction CalculateDynamic(FunctionDefinitionExpression expression)
    {
        return new UserFunction(expression, _environmentStack.Clone());
    }

    private object? CalculateDynamic(GroupingExpression expression)
    {
        return Calculate(expression.Expression);
    }

    private object? CalculateDynamic(BinaryExpression expression)
    {
        // TODO: describe order of evaluation
        var leftResult = Calculate(expression.Left);
        var rightResult = new Lazy<object?>(() => Calculate(expression.Right));

        if (expression.Operator == Operator.Addition)
            return Arithmetical.Add(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));
        if (expression.Operator == Operator.Subtraction)
            return Arithmetical.Subtract(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));
        if (expression.Operator == Operator.Multiplication)
            return Arithmetical.Multiply(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));
        if (expression.Operator == Operator.Division)
            return Arithmetical.Divide(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));
        if (expression.Operator == Operator.Remainder)
            return Arithmetical.Remainder(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));
        if (expression.Operator == Operator.Exponentiation)
            return Arithmetical.Exponentiate(Casting.ToNumber(leftResult), Casting.ToNumber(rightResult.Value));

        if (expression.Operator == Operator.Concatenation)
            return Character.Concatenate(Casting.ToString(leftResult), Casting.ToString(rightResult.Value));

        if (expression.Operator == Operator.Disjunction)
            return Logical.Disjoin(Casting.ToBool(leftResult), new Lazy<bool?>(() => Casting.ToBool(rightResult.Value)));
        if (expression.Operator == Operator.Conjunction)
            return Logical.Conjoin(Casting.ToBool(leftResult), new Lazy<bool?>(() => Casting.ToBool(rightResult.Value)));

        if (expression.Operator == Operator.NullCoalescing)
            return leftResult ?? rightResult.Value;
        if (expression.Operator == Operator.NullSafePipe)
            return leftResult is not null
                ? Casting.ToFunction(rightResult.Value).Call(this, new List<object?> { leftResult })
                : null;

        if (expression.Operator is Operator.Assignment or Operator.AdditionAssignment or Operator.SubtractionAssignment
                or Operator.MultiplicationAssignment or Operator.DivisionAssignment or Operator.RemainderAssignment)
            return PerformAssignment(expression.Left, rightResult.Value, expression.Operator);

        throw new ArgumentOutOfRangeException(nameof(expression.Operator), expression.Operator, "Invalid operator");
    }

    private object? PerformAssignment(Expression left, object? right, Operator assignmentOperator)
    {
        if (left is not IdentifierExpression identifierExpression)
            throw new RunnerException(new InvalidLvalue(left.GetType()));

        var valueToAssign = assignmentOperator switch
        {
            Operator.Assignment => right,
            Operator.AdditionAssignment => Arithmetical.Add(_environmentStack.Access(identifierExpression.Name), right),
            Operator.SubtractionAssignment => Arithmetical.Subtract(_environmentStack.Access(identifierExpression.Name), right),
            Operator.MultiplicationAssignment => Arithmetical.Multiply(_environmentStack.Access(identifierExpression.Name), right),
            Operator.DivisionAssignment => Arithmetical.Divide(_environmentStack.Access(identifierExpression.Name), right),
            Operator.RemainderAssignment => Arithmetical.Remainder(_environmentStack.Access(identifierExpression.Name), right),
            _ => throw new NotSupportedException()
        };
        _environmentStack.Assign(identifierExpression.Name, valueToAssign);
        return valueToAssign;
    }

    private object? CalculateDynamic(UnaryExpression expression)
    {
        var innerResult = Calculate(expression.Expression);

        if (expression.Operator == Operator.NumberPromotion)
            return Casting.ToNumber(innerResult);
        if (expression.Operator == Operator.ArithmeticNegation)
            return Arithmetical.Negate(Casting.ToNumber(innerResult));

        if (expression.Operator == Operator.LogicalNegation)
            return Logical.Negate(Casting.ToBool(innerResult));

        throw new ArgumentOutOfRangeException(nameof(expression.Operator), expression.Operator, "Invalid operator");
    }

    private object? CalculateDynamic(FunctionCallExpression expression)
    {
        var function = Casting.ToFunction(Calculate(expression.Callee));
        return function.Call(this, expression.Arguments.Select(x => Calculate(x)).ToList());
    }

    private object? CalculateDynamic(IdentifierExpression expression)
    {
        if (expression.NamespaceLevels.Count > 0)
            throw new ArgumentException(null, nameof(expression));
        return _environmentStack.Access(expression.Name);
    }

    private object? CalculateDynamic(LiteralExpression expression)
    {
        if (expression.Value is ulong integerValue)
            /* at this point unsigned literal value can be no greater than 9223372036854775807
             * or 9223372036854775808 which overflows to -9223372036854775808 (in case of later negation)
             */
            return unchecked((long)integerValue);
        return expression.Value;
    }

    private object? CalculateDynamic(TypeCastExpression expression)
    {
        var value = Calculate(expression.Expression);
        return Casting.To(value, expression.Type);
    }

    private object? CalculateDynamic(TypeCheckExpression expression)
    {
        var value = Calculate(expression.Expression);
        return expression.IsInequalityCheck ^ value switch
        {
            null => expression.Type == DataType.Null,
            long => expression.Type == DataType.Integer,
            double => expression.Type == DataType.Float,
            string => expression.Type == DataType.String,
            bool => expression.Type == DataType.Bool,
            IFunction => expression.Type == DataType.Function
        };
    }

    private bool? CalculateDynamic(ComparisonExpression expression)
    {
        if (expression.Comparisons.Count == 0)
            throw new ArgumentException(null, nameof(expression));
        var operatorList = expression.Comparisons.Select(x => x.Operator);
        var valueList = expression.Comparisons.Select(x => x.Right).Prepend(expression.Left).Select(x => Calculate(x))
            .ToList();
        var resultList = valueList.Zip(valueList.Skip(1), (a, b) => (a, b))
            .Zip(operatorList,
                (values, @operator) => @operator switch
                {
                    Operator.LessThanComparison => Relational.IsLessThan(values.a, values.b),
                    Operator.LessOrEqualComparison => Relational.IsLessOrEqual(values.a, values.b),
                    Operator.GreaterThanComparison => Relational.IsGreaterThan(values.a, values.b),
                    Operator.GreaterOrEqualComparison => Relational.IsGreaterOrEqual(values.a, values.b),
                    Operator.EqualComparison => Relational.IsEqualTo(values.a, values.b),
                    Operator.NotEqualComparison => Relational.IsNotEqualTo(values.a, values.b)
                }).ToList();
        return resultList.Aggregate((acc, next) => Logical.Conjoin(acc, new Lazy<bool?>(next)));
    }
}
