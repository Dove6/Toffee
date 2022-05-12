using System.Collections.ObjectModel;
using Toffee.ErrorHandling;
using Toffee.Scanning;

namespace Toffee.LexicalAnalysis;

public abstract record LexerError(Position Position) : Error(Position);
public record UnexpectedEndOfText(Position Position, TokenType BuiltTokenType) : LexerError(Position);
public record ExceededMaxLexemeLength(Position Position, int MaxLexemeLength) : LexerError(Position);
public record UnknownToken(Position Position, string Content) : LexerError(Position);
public record NumberLiteralTooLarge(Position Position) : LexerError(Position);
public record InvalidNonDecimalPrefix(Position Position, char NonDecimalPrefix) : LexerError(Position);
public record MissingNonDecimalDigits(Position Position, char NonDecimalPrefix) : LexerError(Position);
public record MissingExponent(Position Position) : LexerError(Position);

public static class LexerErrorExtensions
{
    private static readonly ReadOnlyDictionary<Type, string> MessageMap = new(new Dictionary<Type, string>
    {
        { typeof(UnexpectedEndOfText), "Unexpected end of text" },
        { typeof(ExceededMaxLexemeLength), "Unexpected end of text" },
        { typeof(UnknownToken), "Unknown token" },
        { typeof(NumberLiteralTooLarge), "Overflow in number literal" },
        { typeof(InvalidNonDecimalPrefix), "Unknown non-decimal number prefix" },
        { typeof(MissingNonDecimalDigits), "No digits after non-decimal number prefix" },
        { typeof(MissingExponent), "No digits after scientific notation prefix" }
    });

    public static string ToMessage(this LexerError error) =>
        MessageMap.GetValueOrDefault(error.GetType(), "Lexical error");
}
