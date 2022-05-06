namespace Toffee.LexicalAnalysis;

public abstract class LexerBase
{
    private int _maxLexemeLength;
    public int MaxLexemeLength
    {
        get => _maxLexemeLength;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxLexemeLength), MaxLexemeLength, null);
            _maxLexemeLength = value;
        }
    }

    public bool HadError { get; protected set; }
    public Token CurrentToken { get; protected set; }
    public LexerError? CurrentError { get; protected set; }

    protected LexerBase(int? maxLexemeLength = null)
    {
        MaxLexemeLength = maxLexemeLength ?? int.MaxValue;
    }

    /// <summary>
    /// Advances the position of the lexer in the token stream.
    /// </summary>
    /// <returns>Superseded token - the current one from before the method was called</returns>
    public abstract Token Advance();

    public void ResetError()
    {
        CurrentError = null;
        HadError = false;
    }
}
