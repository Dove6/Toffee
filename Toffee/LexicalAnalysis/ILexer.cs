namespace Toffee.LexicalAnalysis;

public interface ILexer
{
    int MaxLexemeLength { get; set; }

    Token CurrentToken { get; }

    bool HadError { get; }
    LexerError? CurrentError { get; }

    /// <summary>
    /// Advances the position of the lexer in the token stream.
    /// </summary>
    /// <returns>Superseded token - the current one from before the method was called</returns>
    Token Advance();

    void ResetError();
}
