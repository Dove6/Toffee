namespace Toffee.LexicalAnalysis;

public interface ILexer
{
    Token CurrentToken { get; }

    void Advance();
}
