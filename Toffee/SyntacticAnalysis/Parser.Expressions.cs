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
}
