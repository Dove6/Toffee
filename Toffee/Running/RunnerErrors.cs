using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.Running;

public abstract record RunnerError(Position Position) : Error(Position);

public record ZeroDivision() : RunnerError(new Position());
public record BadCast() : RunnerError(new Position());

// TODO: move to parser
public record ReturnOutsideOfFunction() : RunnerError(new Position());
public record BreakOutsideOfLoop() : RunnerError(new Position());

public record VariableUndefined() : RunnerError(new Position());
public record VariableAlreadyDefined() : RunnerError(new Position());
public record AssignmentToConst() : RunnerError(new Position());
public record NullInForLoopRange() : RunnerError(new Position());
public record InvalidLvalue() : RunnerError(new Position());
public record BadArgumentCount() : RunnerError(new Position());
public record NonNullArgumentRequired() : RunnerError(new Position());

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
        { typeof(NullInForLoopRange), "Invalid l-value in assignment" },
        { typeof(BadArgumentCount), "Arguments and parameters mismatch" },
        { typeof(NonNullArgumentRequired), "A non-nullable function argument turned out to be null" }
    }.ToImmutableDictionary();

    public static string ToMessage(this RunnerError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Runtime error");
}
