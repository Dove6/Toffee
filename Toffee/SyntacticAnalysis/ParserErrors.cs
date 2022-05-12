using System.Collections.ObjectModel;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public abstract record ParserError(Position Position) : Error(Position);

public static class ParserErrorExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>());

    public static string ToMessage(this ParserError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Lexical error");
}
