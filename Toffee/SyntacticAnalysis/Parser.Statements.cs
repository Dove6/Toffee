using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
    // statement
    //     = unterminated_statement, SEMICOLON, { SEMICOLON };
    private bool TryParseStatement(out Statement? parsedStatement)
    {
        parsedStatement = null;
        if (!TryParseUnterminatedStatement(out parsedStatement))
            return false;
        // not consuming here means not blocking if line had a single trailing semicolon
        if (TryEnsureToken(TokenType.Semicolon))
            parsedStatement = parsedStatement! with { IsTerminated = true };
        return true;
    }

    // unterminated_statement
    //     = namespace_import
    //     | variable_initialization_list
    //     | break
    //     | break_if
    //     | return
    //     | expression;
    private bool TryParseUnterminatedStatement(out Statement? parsedStatement)
    {
        parsedStatement = null;
        var statementParsers = new List<ParseStatementDelegate>
        {
            ParseNamespaceImportStatement,
            ParseVariableInitializationListStatement,
            ParseBreakStatement,
            ParseBreakIfStatement,
            ParseReturnStatement,
            ParseExpressionStatement
        };

        foreach (var parser in statementParsers)
        {
            var parserResult = parser();
            if (parserResult is null)
                continue;
            parsedStatement = parserResult;
            return true;
        }
        return false;
    }

    // namespace_import
    //     = KW_PULL, namespace;
    // namespace
    //     = IDENTIFIER, { OP_DOT, IDENTIFIER };
    private Statement? ParseNamespaceImportStatement() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordPull))
            return null;

        var list = new List<IdentifierExpression>();
        var firstIdentifier = ConsumeToken(TokenType.Identifier);
        list.Add(new IdentifierExpression((string)firstIdentifier.Content!));

        while (TryConsumeToken(out _, TokenType.OperatorDot))
        {
            var nextIdentifier = ConsumeToken(TokenType.Identifier);
            list.Add(new IdentifierExpression((string)nextIdentifier.Content!));
        }

        return new NamespaceImportStatement(list);
    });

    // variable_initialization_list
    //     = KW_INIT, variable_initialization, { COMMA, variable_initialization };
    private Statement? ParseVariableInitializationListStatement() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordInit))
            return null;

        var list = new List<VariableInitialization>
        {
            ParseVariableInitialization()
        };
        while (TryConsumeToken(out _, TokenType.Comma))
            list.Add(ParseVariableInitialization());

        return new VariableInitializationListStatement(list);
    });

    // variable_initialization
    //     = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
    private VariableInitialization ParseVariableInitialization()
    {
        var isConst = TryConsumeToken(out _, TokenType.KeywordConst);

        if (!TryConsumeToken(out var identifier, TokenType.Identifier))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken,
                isConst ? new[] { TokenType.Identifier } : new[] { TokenType.KeywordConst, TokenType.Identifier }));
        var variableName = (string)identifier.Content!;

        if (!TryConsumeToken(out var assignmentToken, _assignmentTokenTypes))
            return new VariableInitialization(variableName, null, isConst);
        if (!TryParseExpression(out var initialValue))
            EmitError(new ExpectedExpression(_lexer.CurrentToken));
        if (TryConsumeToken(out _, TokenType.OperatorEquals))
            return new VariableInitialization(variableName, initialValue, isConst);
        EmitError(new UnexpectedToken(assignmentToken, TokenType.OperatorEquals));
        return new VariableInitialization(variableName, initialValue, isConst);
    }

    // break
    //     = KW_BREAK;
    private Statement? ParseBreakStatement() => SupplyPosition(() =>
        TryConsumeToken(out _, TokenType.KeywordBreak) ? new BreakStatement() : null);

    // break_if
    //     = KW_BREAK_IF, parenthesized_expression;
    private Statement? ParseBreakIfStatement() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordBreakIf))
            return null;

        var condition = ParseParenthesizedExpression();
        return new BreakIfStatement(condition);
    });

    // return
    //     = KW_RETURN, expression;
    private Statement? ParseReturnStatement() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordReturn))
            return null;

        return TryParseExpression(out var parsedExpression)
            ? new ReturnStatement(parsedExpression!)
            : new ReturnStatement();
    });

    private Statement? ParseExpressionStatement() => SupplyPosition(() =>
        TryParseExpression(out var parsedExpression) ? new ExpressionStatement(parsedExpression!) : null);

    private Statement? SupplyPosition(Func<Statement?> parseMethod)
    {
        var statementPosition = _lexer.CurrentToken.StartPosition;
        var statement = parseMethod();
        return statement is null ? null : statement with { Position = statementPosition };
    }
}
