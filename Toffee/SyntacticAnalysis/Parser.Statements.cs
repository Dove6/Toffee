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
        if (_lexer.CurrentToken.Type != TokenType.Semicolon)
            /* TODO: error */; // not using Advance here means not blocking (waiting for another input line)
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

    // variable_initialization
    //     = [ KW_CONST ], IDENTIFIER, [ OP_EQUALS, expression ];
    private bool TryParseVariableInitialization(out VariableInitialization? variableInitialization)
    {
        variableInitialization = null;

        var isConst = _lexer.CurrentToken.Type == TokenType.KeywordConst;
        if (isConst)
            _lexer.Advance();

        if (_lexer.CurrentToken.Type != TokenType.Identifier)
            return false; // TODO: error

        var variableName = (string)_lexer.Advance().Content!;

        if (_lexer.CurrentToken.Type != TokenType.OperatorEquals)
        {
            variableInitialization = new VariableInitialization(variableName, null, isConst);
            return true;
        }

        _lexer.Advance();
        if (!TryParseExpression(out var initialValue))
            return false; // TODO: error
        variableInitialization = new VariableInitialization(variableName, initialValue, isConst);
        return true;
    }

    // break
    //     = KW_BREAK;
    private IStatement? ParseBreakStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordBreak)
            return null;

        throw new NotImplementedException();
    }

    // break_if
    //     = KW_BREAK_IF, parenthesized_expression;
    private IStatement? ParseBreakIfStatement()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordBreakIf)
            return null;

        throw new NotImplementedException();
    }

    // return
    //     = KW_RETURN, expression;
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
}
