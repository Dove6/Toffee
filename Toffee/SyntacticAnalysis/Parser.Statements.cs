using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
    // statement
    //     = unterminated_statement, SEMICOLON, { SEMICOLON };
    private bool TryParseStatement(out IStatement? parsedStatement)
    {
        parsedStatement = null;
        if (!TryParseUnterminatedStatement(out parsedStatement))
            return false;
        // not consuming here means not blocking if line had a single trailing semicolon
        EnsureToken(TokenType.Semicolon);
        return true;
    }

    // unterminated_statement
    //     = namespace_import
    //     | variable_initialization_list
    //     | break
    //     | break_if
    //     | return
    //     | expression;
    private bool TryParseUnterminatedStatement(out IStatement? parsedStatement)
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

    // variable_initialization_list
    //     = KW_INIT, variable_initialization, { COMMA, variable_initialization };
    private IStatement? ParseVariableInitializationListStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordInit))
            return null;

        var list = new List<VariableInitialization>();
        while (TryParseVariableInitialization(out var parsedVariable))
            list.Add(parsedVariable!);
        return new VariableInitializationListStatement(list);
    }

    // variable_initialization
    //     = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
    private bool TryParseVariableInitialization(out VariableInitialization? variableInitialization)
    {
        variableInitialization = null;

        var isConst = TryConsumeToken(out _, TokenType.KeywordConst);

        var identifier = ConsumeToken(TokenType.Identifier);
        var variableName = (string)identifier.Content!;

        if (!TryConsumeToken(out _, TokenType.OperatorEquals))
        {
            variableInitialization = new VariableInitialization(variableName, null, isConst);
            return true;
        }

        if (!TryParseExpression(out var initialValue))
            throw new NotImplementedException();
        variableInitialization = new VariableInitialization(variableName, initialValue, isConst);
        return true;
    }

    // break
    //     = KW_BREAK;
    private IStatement? ParseBreakStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordBreak))
            return null;

        return new BreakStatement();
    }

    // break_if
    //     = KW_BREAK_IF, parenthesized_expression;
    private IStatement? ParseBreakIfStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordBreakIf))
            return null;

        var condition = ParseParenthesizedExpression();
        return new BreakIfStatement(condition);
    }

    // return
    //     = KW_RETURN, expression;
    private IStatement? ParseReturnStatement()
    {
        if (!TryConsumeToken(out _, TokenType.KeywordReturn))
            return null;

        if (!TryParseExpression(out var parsedExpression))
            return new ReturnStatement();

        return new ReturnStatement(parsedExpression!);
    }

    private IStatement? ParseExpressionStatement()
    {
        if (!TryParseExpression(out var parsedExpression))
            return null;

        return new ExpressionStatement(parsedExpression!);
    }
}
