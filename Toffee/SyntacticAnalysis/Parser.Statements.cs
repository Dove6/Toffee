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
        foreach (var parser in _statementParsers)
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
    private Statement? ParseNamespaceImportStatement()
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
    }

    // variable_initialization_list
    //     = KW_INIT, variable_initialization, { COMMA, variable_initialization };
    private Statement? ParseVariableInitializationListStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordInit))
            return null;

        var list = new List<VariableInitialization>();
        if (!TryParseVariableInitialization(out var firstInitialization))
            throw new ParserException(new ExpectedStatement(_lexer.CurrentToken, typeof(VariableInitialization)));
        list.Add(firstInitialization!);

        while (TryConsumeToken(out _, TokenType.Comma))
            if (TryParseVariableInitialization(out var nextInitialization))
                list.Add(nextInitialization!);
            else
                throw new ParserException(new ExpectedStatement(_lexer.CurrentToken, typeof(VariableInitialization)));

        return new VariableInitializationListStatement(list);
    }

    // variable_initialization
    //     = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
    private bool TryParseVariableInitialization(out VariableInitialization? variableInitialization)
    {
        variableInitialization = null;

        var isConst = TryConsumeToken(out _, TokenType.KeywordConst);
        if (!TryConsumeToken(out var identifier, TokenType.Identifier))
            return !isConst
                ? false
                : throw new ParserException(new UnexpectedToken(_lexer.CurrentToken, TokenType.Identifier));
        var variableName = (string)identifier.Content!;

        if (!TryConsumeToken(out _, TokenType.OperatorEquals))
        {
            variableInitialization = new VariableInitialization(variableName, null, isConst);
            return true;
        }

        if (!TryParseExpression(out var initialValue))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken));
        variableInitialization = new VariableInitialization(variableName, initialValue, isConst);
        return true;
    }

    // break
    //     = KW_BREAK;
    private Statement? ParseBreakStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordBreak))
            return null;

        return new BreakStatement();
    }

    // break_if
    //     = KW_BREAK_IF, parenthesized_expression;
    private Statement? ParseBreakIfStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordBreakIf))
            return null;

        var condition = ParseParenthesizedExpression();
        return new BreakIfStatement(condition);
    }

    // return
    //     = KW_RETURN, expression;
    private Statement? ParseReturnStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordReturn))
            return null;

        if (!TryParseExpression(out var parsedExpression))
            return new ReturnStatement();

        return new ReturnStatement(parsedExpression!);
    }

    private Statement? ParseExpressionStatement()
    {
        if (!TryParseExpression(out var parsedExpression))
            return null;

        return new ExpressionStatement(parsedExpression!);
    }
}
