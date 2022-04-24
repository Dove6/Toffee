namespace Toffee.LexicalAnalysis;

public abstract class LexerBase
{
    public bool HadError { get; protected set; }
    public Token CurrentToken { get; protected set; }
    public LexerError? CurrentError { get; protected set; }

    public abstract void Advance();

    public void ResetError()
    {
        CurrentError = null;
        HadError = false;
    }
}
