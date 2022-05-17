using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser : IParser
{
    private readonly ILexer _lexer;
    private readonly IParserErrorHandler? _errorHandler;

    public Statement? CurrentStatement { get; private set; }

    private delegate Statement? ParseStatementDelegate();
    private delegate Expression? ParseExpressionDelegate();

    public Parser(ILexer lexer, IParserErrorHandler? errorHandler = null)
    {
        _lexer = new CommentSkippingLexer(lexer);
        _errorHandler = errorHandler;
    }

    private bool TryEnsureToken(params TokenType[] expectedType) =>
        expectedType.Contains(_lexer.CurrentToken.Type);

    private void EnsureToken(params TokenType[] expectedType)
    {
        if (!TryEnsureToken(expectedType))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, expectedType));
    }

    private bool TryConsumeToken(out Token matchedToken, params TokenType[] expectedType)
    {
        matchedToken = new Token(TokenType.Unknown);
        if (!TryEnsureToken(expectedType))
            return false;
        matchedToken = _lexer.Advance();
        return true;
    }

    private Token ConsumeToken(params TokenType[] expectedType)
    {
        if (!TryConsumeToken(out var matchedToken, expectedType))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, expectedType));
        return matchedToken;
    }

    private void EmitError(ParserError error)
    {
        // TODO: actually use the method
        _errorHandler?.Handle(error);
    }

    private void EmitWarning(ParserWarning warning)
    {
        // TODO: actually use the method
        _errorHandler?.Handle(warning);
    }

    private void SkipSemicolons()
    {
        while (_lexer.CurrentToken.Type == TokenType.Semicolon)
            _lexer.Advance();
    }

    public Statement? Advance()
    {
        SkipSemicolons();

        if (_lexer.CurrentToken.Type == TokenType.EndOfText)
            CurrentStatement = null;
        else if (TryParseStatement(out var parsedStatement))
            CurrentStatement = parsedStatement;
        else
            throw new ParserException(new ExpectedStatement(_lexer.CurrentToken));

        return CurrentStatement;
    }
}
