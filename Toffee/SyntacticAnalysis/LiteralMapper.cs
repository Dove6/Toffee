using System.Collections.ObjectModel;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class LiteralMapper
{
    private static readonly ReadOnlyDictionary<TokenType, DataType> TypeMap = new(new Dictionary<TokenType, DataType>
    {
        { TokenType.LiteralInteger, DataType.Integer },
        { TokenType.LiteralFloat, DataType.Float },
        { TokenType.LiteralString, DataType.String },
        { TokenType.KeywordTrue, DataType.Bool },
        { TokenType.KeywordFalse, DataType.Bool },
        { TokenType.KeywordNull, DataType.Null }
    });

    public static LiteralExpression MapToLiteralExpression(Token literalToken)
    {
        var literalType = TypeMap[literalToken.Type];  // TODO: throws
        var literalValue = literalType switch
        {
            DataType.Bool => literalToken.Type == TokenType.KeywordTrue,
            DataType.Null => null,
            _             => literalToken.Content
        };
        return new LiteralExpression(literalType, literalValue);
    }
}
