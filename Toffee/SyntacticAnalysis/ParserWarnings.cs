using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserWarning(Position Position) : Warning(Position);

public static class ParserWarningExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = ImmutableDictionary<Type, string>.Empty;

    public static string ToMessage(this ParserWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Lexical warning");
}
