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

    public abstract void Advance();

    public void ResetError()
    {
        CurrentError = null;
        HadError = false;
    }
}
