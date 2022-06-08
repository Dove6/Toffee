using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserWarning(Position Position) : Warning(Position);

public record DefaultBranchMissing(Position Position) : ParserWarning(Position);
public record SuperfluousNullInitialValue(Position Position, string Name) : ParserWarning(Position)
{
    public SuperfluousNullInitialValue(VariableInitialization initialization)
        : this(initialization.Position ?? new Position(), initialization.Name)
    { }
}
public record IgnoredResultExpression(Position Position) : ParserWarning(Position)
{
    public IgnoredResultExpression(Expression result) : this(result.StartPosition)
    { }
}

public static class ParserWarningExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = new Dictionary<Type, string>
    {
        { typeof(DefaultBranchMissing), "Default branch is missing" },
        { typeof(SuperfluousNullInitialValue), "Initial value is not needed as non-const variables are initialized to null by default" },
        { typeof(IgnoredResultExpression), "Result expressions in loop blocks are ignored" }
    }.ToImmutableDictionary();

    public static string ToMessage(this ParserWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Syntax warning");
}
