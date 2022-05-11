using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public class Parser : IParser
{
    private BaseLexer _lexer;

    private delegate IStatement? ParseStatementDelegate();
    private readonly List<ParseStatementDelegate> _statementParsers;

    private delegate IExpression? ParseExpressionDelegate();
    private readonly List<ParseExpressionDelegate> _expressionParsers;

    public Parser(BaseLexer lexer)
    {
        _lexer = lexer;

        _statementParsers = new List<ParseStatementDelegate>
        {
            ParseVariableInitializationListStatement,
            ParseBreakStatement,
            ParseBreakIfStatement,
            ParseReturnStatement,
            ParseExpressionStatement
        };

        _expressionParsers = new List<ParseExpressionDelegate>();
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

    private bool TryParseStatement(out IStatement? parsedStatement)
    {
        parsedStatement = null;
        foreach (var parser in _statementParsers)
        {
            var parserResult = parser();
            if (parserResult is null)
                continue;
            parsedStatement = parserResult;
            while (_lexer.CurrentToken.Type == TokenType.Semicolon)
                _lexer.Advance();  // TODO: out bool
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

    public Program Parse()
    {
        var list = new List<IStatement>();

        while (TryParseStatement(out var parsedStatement))
            list.Add(parsedStatement!);

        return new Program(list);
    }
}
