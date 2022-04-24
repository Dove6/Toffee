namespace Toffee.LexicalAnalysis;

public interface ILexer
{
    bool HadError { get; }
    Token CurrentToken { get; }
    LexerError? CurrentError { get; }

    void Advance();
    void ResetError();
}
