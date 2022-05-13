using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser : IParser
{
    private readonly BaseLexer _lexer;
    private readonly IParserErrorHandler? _errorHandler;

    public Statement? CurrentStatement { get; private set; }

    private delegate Statement? ParseStatementDelegate();
    private readonly List<ParseStatementDelegate> _statementParsers;

    private delegate Expression? ParseExpressionDelegate();
    private readonly List<ParseExpressionDelegate> _expressionParsers;

    public Parser(BaseLexer lexer, IParserErrorHandler? errorHandler = null)
    {
        _lexer = lexer;
        _errorHandler = errorHandler;

        _statementParsers = new List<ParseStatementDelegate>
        {
            ParseVariableInitializationListStatement,
            ParseBreakStatement,
            ParseBreakIfStatement,
            ParseReturnStatement,
            ParseExpressionStatement
        };

        _expressionParsers = new List<ParseExpressionDelegate>
        {
            ParseBlockExpression,
            ParseConditionalExpression,
            ParseForLoopExpression,
            ParseWhileLoopExpression,
            ParseFunctionDefinitionExpression,
            ParsePatternMatchingExpression,
            ParseAssignmentExpression
        };

        Advance();
    }

    private bool TryEnsureToken(params TokenType[] expectedType) =>
        expectedType.Contains(_lexer.CurrentToken.Type);

    private void EnsureToken(params TokenType[] expectedType)
    {
        if (!expectedType.Contains(_lexer.CurrentToken.Type))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, expectedType));
    }

    private bool TryConsumeToken(out Token matchedToken, params TokenType[] expectedType)
    {
        matchedToken = new Token(TokenType.Unknown);
        if (!expectedType.Contains(_lexer.CurrentToken.Type))
            return false;
        matchedToken = _lexer.Advance();
        return true;
    }

    private Token ConsumeToken(params TokenType[] expectedType)
    {
        if (!expectedType.Contains(_lexer.CurrentToken.Type))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, expectedType));
        return _lexer.Advance();
    }

    private void EmitError(ParserError error)
    {
        _errorHandler?.Handle(error);
    }

    private void EmitWarning(ParserWarning warning)
    {
        _errorHandler?.Handle(warning);
    }

    private void SkipSemicolons()
    {
        while (_lexer.CurrentToken.Type == TokenType.Semicolon)
            _lexer.Advance();
    }

    public Statement? Advance()
    {
        var supersededStatement = CurrentStatement;
        SkipSemicolons();

        if (_lexer.CurrentToken.Type == TokenType.EndOfText)
            CurrentStatement = null;
        else if (TryParseStatement(out var parsedStatement))
            CurrentStatement = parsedStatement;
        else
            throw new ParserException(new ExpectedStatement(_lexer.CurrentToken));

        return supersededStatement;
    }
}
