using System.Collections.ObjectModel;

namespace Toffee.LexicalAnalysis;

public abstract record LexerWarning(uint Offset);
public record UnknownEscapeSequence(char Specifier, uint Offset = 0) : LexerWarning(Offset);
public record MissingHexCharCode(uint Offset = 0) : LexerWarning(Offset);

public static class LexerWarningExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>
    {
        { typeof(UnknownEscapeSequence), "Unknown escape sequence in string" },
        { typeof(MissingHexCharCode), "Hexadecimal character code missing in escape sequence in string" }
    });

    public static string ToMessage(this LexerWarning warning) =>
        MessageMap.GetValueOrDefault(warning.GetType(), "Lexical warning");
}
