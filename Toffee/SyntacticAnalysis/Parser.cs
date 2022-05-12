using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public class Parser : IParser
{
    private BaseLexer _lexer;
    private readonly IParserErrorHandler? _errorHandler;

    public IStatement? CurrentStatement { get; private set; }

    private delegate IStatement? ParseStatementDelegate();
    private readonly List<ParseStatementDelegate> _statementParsers;

    private delegate IExpression? ParseExpressionDelegate();
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

        _expressionParsers = new List<ParseExpressionDelegate>();

        Advance();
    }

    private void EmitError(ParserError error)
    {
        _errorHandler?.Handle(error);
    }

    private void EmitWarning(ParserWarning warning)
    {
        _errorHandler?.Handle(warning);
    }

    // TODO: grammar definition
    private IStatement? ParseVariableInitializationListStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordInit)
            return null;

        _lexer.Advance();

        var list = new List<VariableInitialization>();

        while (TryParseVariableInitialization(out var parsedVariable))
        {
            list.Add(parsedVariable!);
        }

        return new VariableInitializationListStatement(list);
    }

    private bool TryParseVariableInitialization(out VariableInitialization? variableInitialization)
    {
        variableInitialization = null;

        var isConst = _lexer.CurrentToken.Type == TokenType.KeywordConst;
        if (isConst)
            _lexer.Advance();

        if (_lexer.CurrentToken.Type != TokenType.Identifier)
            return false;  // TODO: error

        var variableName = (string)_lexer.Advance().Content!;

        if (_lexer.CurrentToken.Type != TokenType.OperatorEquals)
        {
            variableInitialization = new VariableInitialization(variableName, null, isConst);
            return true;
        }

        _lexer.Advance();
        if (!TryParseExpression(out var initialValue))
            return false;  // TODO: error
        variableInitialization = new VariableInitialization(variableName, initialValue, isConst);
        return true;
    }

    private IStatement? ParseBreakStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordBreak)
            return null;

        throw new NotImplementedException();
    }

    private IStatement? ParseBreakIfStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordBreakIf)
            return null;

        throw new NotImplementedException();
    }

    private IStatement? ParseReturnStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordReturn)
            return null;

        throw new NotImplementedException();
    }

    private IStatement? ParseExpressionStatement()
    {
        if (!TryParseExpression(out var parsedExpression))
            return null;

        return new ExpressionStatement(parsedExpression!);
    }

    private bool TryParseStatement(out IStatement? parsedStatement, out bool isTerminated)
    {
        parsedStatement = null;
        isTerminated = false;
        foreach (var parser in _statementParsers)
        {
            var parserResult = parser();
            if (parserResult is null)
                continue;
            parsedStatement = parserResult;
            if (_lexer.CurrentToken.Type == TokenType.Semicolon)
                isTerminated = true;  // not using Advance here means not blocking (waiting for another input line)
            return true;
        }
        return false;
        throw new NotImplementedException();  // TODO: error
    }

    private bool TryParseExpression(out IExpression? parsedExpression)
    {
        parsedExpression = null;
        foreach (var parser in _expressionParsers)
        {
            var parserResult = parser();
            if (parserResult is null)
                continue;
            parsedExpression = parserResult;
            return true;
        }
        return false;
        throw new NotImplementedException();  // TODO: error
    }

    private void SkipSemicolons()
    {
        while (_lexer.CurrentToken.Type == TokenType.Semicolon)
            _lexer.Advance();
    }

    public IStatement? Advance()
    {
        var supersededStatement = CurrentStatement;
        SkipSemicolons();
        CurrentStatement = TryParseStatement(out var parsedStatement, out _) ? parsedStatement : null;
        return supersededStatement;
    }
}
