using System.Collections.ObjectModel;

namespace Toffee.LexicalAnalysis;

public abstract record LexerError(uint Offset);
public record UnexpectedEndOfText(uint Offset = 0) : LexerError(Offset);
public record ExceededMaxLexemeLength(uint Offset = 0) : LexerError(Offset);
public record UnknownToken(uint Offset = 0) : LexerError(Offset);
public record NumberLiteralTooLarge(uint Offset = 0) : LexerError(Offset);
public record MissingNonDecimalDigits(uint Offset = 0) : LexerError(Offset);

public static class LexerErrorExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>
    {
        { typeof(UnexpectedEndOfText), "Unexpected end of text" },
        { typeof(ExceededMaxLexemeLength), "Unexpected end of text" },
        { typeof(UnknownToken), "Unknown token" },
        { typeof(NumberLiteralTooLarge), "Overflow in number literal" },
        { typeof(MissingNonDecimalDigits), "No digits provided after non-decimal number prefix" }
    });

    public static string ToMessage(this LexerError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Lexical error");
}
