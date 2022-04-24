namespace Toffee.LexicalAnalysis;

public abstract record LexerWarning(uint Offset);
public record UnknownEscapeSequence(char Specifier, uint Offset = 0) : LexerWarning(Offset);
public record HexCharCodeMissing(uint Offset = 0) : LexerWarning(Offset);
