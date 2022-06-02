using System.Collections.Immutable;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public abstract record LexerWarning(Position Position) : Warning(Position);

public record UnknownEscapeSequence(Position Position, char Specifier) : LexerWarning(Position);
public record MissingHexCharCode(Position Position) : LexerWarning(Position);

public static class LexerWarningExtensions
{
    private static readonly ImmutableDictionary<Type, string> MessageMap = new Dictionary<Type, string>
    {
        { typeof(UnknownEscapeSequence), "Unknown escape sequence in string" },
        { typeof(MissingHexCharCode), "Hexadecimal character code missing in escape sequence in string" }
    }.ToImmutableDictionary();

    public static string ToMessage(this LexerWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Lexical warning");
}
