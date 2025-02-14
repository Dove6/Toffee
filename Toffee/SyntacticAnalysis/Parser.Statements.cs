﻿using Toffee.LexicalAnalysis;

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

    private Statement ParseStatement()
    {
        if (TryParseStatement(out var parsedStatement))
            return parsedStatement!;
        throw new ParserException(new ExpectedStatement(_lexer.CurrentToken));
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
        var statementParsers = new List<ParseStatementDelegate>
        {
            ParseNamespaceImportStatement,
            ParseVariableInitializationListStatement,
            ParseBreakStatement,
            ParseBreakIfStatement,
            ParseReturnStatement,
            ParseExpressionStatement
        };

        parsedStatement = statementParsers
            .Select(parser => parser())
            .FirstOrDefault(result => result is not null);
        return parsedStatement is not null;
    }

    // namespace_import
    //     = KW_PULL, namespace;
    // namespace
    //     = IDENTIFIER, { OP_DOT, IDENTIFIER };
    private Statement? ParseNamespaceImportStatement() => SupplyPosition(() =>
    {
        if (!TryConsumeToken(out _, TokenType.KeywordPull))
            return null;

        var list = new List<string>();
        var firstIdentifier = ConsumeToken(TokenType.Identifier);
        list.Add((string)firstIdentifier.Content!);

        while (TryConsumeToken(out _, TokenType.OperatorDot))
        {
            var nextIdentifier = ConsumeToken(TokenType.Identifier);
            list.Add((string)nextIdentifier.Content!);
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
            ValidateVariableInitialization(ParseVariableInitialization())
        };
        while (TryConsumeToken(out _, TokenType.Comma))
        {
            var initialization = ValidateVariableInitialization(ParseVariableInitialization());
            list.Add(initialization);
        }

        return new VariableInitializationListStatement(list);
    });

    // variable_initialization
    //     = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
    private VariableInitialization ParseVariableInitialization()
    {
        var isConst = TryConsumeToken(out var constToken, TokenType.KeywordConst);
        var assignmentLikeTokenTypes =
            OperatorMapper.AssignmentTokenTypes.Append(TokenType.OperatorEqualsEquals).ToArray();

        if (!TryConsumeToken(out var identifier, TokenType.Identifier))
            throw new ParserException(new UnexpectedToken(_lexer.CurrentToken,
                isConst ? new[] { TokenType.Identifier } : new[] { TokenType.KeywordConst, TokenType.Identifier }));
        var variableName = (string)identifier.Content!;

        var position = isConst ? constToken.StartPosition : identifier.StartPosition;

        if (!TryConsumeToken(out var assignmentToken, assignmentLikeTokenTypes))
            return new VariableInitialization(variableName, null, isConst, position);
        var initialValue = ParseExpression();
        if (assignmentToken.Type == TokenType.OperatorEquals)
            return new VariableInitialization(variableName, initialValue, isConst, position);
        EmitError(new UnexpectedToken(assignmentToken, TokenType.OperatorEquals));
        return new VariableInitialization(variableName, initialValue, isConst, position);
    }

    private VariableInitialization ValidateVariableInitialization(VariableInitialization initialization)
    {
        if (initialization.IsConst)
        {
            if (initialization.InitialValue is null)
                EmitError(new ImplicitConstInitialization(initialization));
        }
        else if (initialization.InitialValue is LiteralExpression { Type: DataType.Null })
            EmitWarning(new SuperfluousNullInitialValue(initialization));
        return initialization;
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
        return new BreakStatement(condition);
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
        return statement is null
            ? null
            : statement with { StartPosition = statementPosition, EndPosition = _lastTokenEndPosition };
    }
}
