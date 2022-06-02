using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public class CommentSkippingLexer : ILexer
{
    private readonly ILexer _lexer;

    public int MaxLexemeLength
    {
        get => _lexer.MaxLexemeLength;
        set => _lexer.MaxLexemeLength = value;
    }

    public Token CurrentToken => _lexer.CurrentToken;
    public LexerError? CurrentError => _lexer.CurrentError;

    public CommentSkippingLexer(ILexer lexer)
    {
        _lexer = lexer;
        SkipComments();
    }

    private void SkipComments()
    {
        while (_lexer.CurrentToken.Type is TokenType.BlockComment or TokenType.LineComment)
            _lexer.Advance();
    }

    public Token Advance()
    {
        var returnedToken = _lexer.Advance();
        SkipComments();
        return returnedToken;
    }
}
