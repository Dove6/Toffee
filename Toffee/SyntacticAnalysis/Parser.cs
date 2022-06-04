using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;
using Toffee.Scanning;

namespace Toffee.SyntacticAnalysis;

public partial class Parser : IParser
{
    private readonly ILexer _lexer;
    private readonly IParserErrorHandler? _errorHandler;

    private Position _lastTokenEndPosition = new();

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
        _lastTokenEndPosition = matchedToken.EndPosition;
        return true;
    }

    private Token ConsumeToken(params TokenType[] expectedType)
    {
        if (!TryConsumeToken(out var matchedToken, expectedType))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, expectedType));
        return matchedToken;
    }

    private void EmitError(ParserError error) => _errorHandler?.Handle(error);

    // TODO: actually use the method
    private void EmitWarning(ParserWarning warning) => _errorHandler?.Handle(warning);

    private void InterceptParserError(Action throwingAction)
    {
        try
        {
            throwingAction();
        }
        catch (ParserException e)
        {
            EmitError(e.Error);
        }
    }

    private T? InterceptParserError<T>(Func<T> throwingFunc)
    {
        try
        {
            return throwingFunc();
        }
        catch (ParserException e)
        {
            EmitError(e.Error);
        }
        return default;
    }

    private void SkipSemicolons()
    {
        while (_lexer.CurrentToken.Type == TokenType.Semicolon)
            _lastTokenEndPosition = _lexer.Advance().EndPosition;
    }

    private void SkipUntilNextStatement()
    {
        while (_lexer.CurrentToken.Type is not (TokenType.Semicolon or TokenType.EndOfText))
            _lastTokenEndPosition = _lexer.Advance().EndPosition;
    }

    public Statement? Advance()
    {
        SkipSemicolons();
        if (_lexer.CurrentToken.Type == TokenType.EndOfText)
            return CurrentStatement = null;

        Statement? parsedStatement;
        while ((parsedStatement = InterceptParserError(ParseStatement)) is null)
        {
            SkipUntilNextStatement();
            SkipSemicolons();
            if (_lexer.CurrentToken.Type == TokenType.EndOfText)
                return CurrentStatement = null;
        }

        CurrentStatement = parsedStatement;
        if (!CurrentStatement.IsTerminated)
            EmitError(new ExpectedSemicolon(_lexer.CurrentToken));

        return CurrentStatement;
    }
}
