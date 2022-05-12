using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public partial class Parser
{
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
        throw new NotImplementedException(); // TODO: error
    }

    private IExpression? ParseBlockExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.LeftBrace)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParseConditionalExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordIf)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParseForLoopExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordFor)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParseWhileLoopExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordWhile)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParseFunctionDefinitionExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordFuncti)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParsePatternMatchingExpression()
    {
        if (_lexer.CurrentToken.Type != TokenType.KeywordMatch)
            return null;

        throw new NotImplementedException();
    }

    private IExpression? ParseAssignmentExpression()
    {
        return null;

        throw new NotImplementedException();
    }
}
