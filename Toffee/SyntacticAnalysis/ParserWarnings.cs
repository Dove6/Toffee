using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserWarning(Position Position) : Warning(Position);

public record DefaultBranchMissing(Position Position) : ParserWarning(Position);

public static class ParserWarningExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = new Dictionary<Type, string>
    {
        { typeof(DefaultBranchMissing), "Default branch is missing" }
    }.ToImmutableDictionary();

    public static string ToMessage(this ParserWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Syntax warning");
}
