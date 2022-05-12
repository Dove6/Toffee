﻿using Toffee.ErrorHandling;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser : IParser
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
