namespace Toffee.LexicalAnalysis;

public abstract record LexerError(uint Offset);
public record UnexpectedEndOfText(uint Offset = 0) : LexerError(Offset);
public record MaxLexemeLengthExceeded(uint Offset = 0) : LexerError(Offset);
public record UnknownToken(uint Offset = 0) : LexerError(Offset);
public record NumberLiteralTooLarge(uint Offset = 0) : LexerError(Offset);
public record NonDecimalDigitsMissing(uint Offset = 0) : LexerError(Offset);
