using System.Collections.Immutable;
using System.Text;
using Toffee.ErrorHandling;
using Toffee.Scanning;
using Toffee.SyntacticAnalysis;

namespace Toffee.Running;

public abstract record RunnerError(Position Position) : Error(Position);

public record ZeroDivision() : RunnerError(new Position());
public record BadCast(params DataType[] TargetType) : RunnerError(new Position())
{
    protected override bool PrintMembers(StringBuilder stringBuilder)
    {
        if (base.PrintMembers(stringBuilder))
            stringBuilder.Append(", ");
        stringBuilder.Append($"{nameof(TargetType)} = ");
        if (TargetType.Length > 1)
            stringBuilder.Append("[ ");
        if (TargetType.Length > 0)
            stringBuilder.Append($"{TargetType[0]}");
        else
            stringBuilder.Append($"[]");
        foreach (var type in TargetType.Skip(1))
            stringBuilder.Append($", {type}");
        if (TargetType.Length > 1)
            stringBuilder.Append(" ]");
        return true;
    }
}

// TODO: move to parser
public record ReturnOutsideOfFunction() : RunnerError(new Position());
public record BreakOutsideOfLoop() : RunnerError(new Position());

public record VariableUndefined(string Name) : RunnerError(new Position());
public record VariableAlreadyDefined(string Name) : RunnerError(new Position());
public record AssignmentToConst(string Name) : RunnerError(new Position());
public record NullInForLoopRange(string Part) : RunnerError(new Position());
public record InvalidLvalue(Type Type) : RunnerError(new Position());
public record BadArgumentCount(int ActualCount, int ExpectedCount) : RunnerError(new Position());
public record NonNullArgumentRequired(string Name, int ParameterIndex) : RunnerError(new Position());
public record ExceptionThrown(string Message) : RunnerError(new Position());

public static class RunnerErrorExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = new Dictionary<Type, string>
    {
        { typeof(ZeroDivision), "Cannot divide by integral zero" },
        { typeof(BadCast), "Cannot cast" },
        { typeof(ReturnOutsideOfFunction), "Return statement used outside of a function" },
        { typeof(BreakOutsideOfLoop), "Break statement used outside of a loop" },
        { typeof(VariableUndefined), "Variable is not declared" },
        { typeof(VariableAlreadyDefined), "Cannot redeclare a variable" },
        { typeof(AssignmentToConst), "Cannot modify a const variable" },
        { typeof(NullInForLoopRange), "For loop range part evaluated to null" },
        { typeof(InvalidLvalue), "Invalid l-value in assignment" },
        { typeof(BadArgumentCount), "Arguments and parameters mismatch" },
        { typeof(NonNullArgumentRequired), "A non-nullable function argument turned out to be null" }
    }.ToImmutableDictionary();

    public static string ToMessage(this RunnerError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Runtime error");
}
