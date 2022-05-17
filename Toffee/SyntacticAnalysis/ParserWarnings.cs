using System.Collections.ObjectModel;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserWarning(Position Position) : Warning(Position);

public static class ParserWarningExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>());

    public static string ToMessage(this ParserWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Lexical warning");
}
