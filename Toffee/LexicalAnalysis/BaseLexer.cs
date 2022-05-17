namespace Toffee.LexicalAnalysis;

public abstract class BaseLexer : ILexer
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

    public Token CurrentToken { get; protected set; }

    public bool HadError { get; protected set; }
    public LexerError? CurrentError { get; protected set; }

    protected BaseLexer(int? maxLexemeLength = null)
    {
        MaxLexemeLength = maxLexemeLength ?? int.MaxValue;
    }

    public abstract Token Advance();

    public void ResetError()
    {
        CurrentError = null;
        HadError = false;
    }
}
