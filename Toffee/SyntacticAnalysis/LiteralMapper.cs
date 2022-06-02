using System.Collections.Immutable;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class LiteralMapper
{
    private static readonly ImmutableDictionary<TokenType, DataType> TypeMap = new Dictionary<TokenType, DataType>
    {
        { TokenType.LiteralInteger, DataType.Integer },
        { TokenType.LiteralFloat, DataType.Float },
        { TokenType.LiteralString, DataType.String },
        { TokenType.KeywordTrue, DataType.Bool },
        { TokenType.KeywordFalse, DataType.Bool },
        { TokenType.KeywordNull, DataType.Null }
    }.ToImmutableDictionary();

    public static TokenType[] LiteralTokenTypes { get; } = TypeMap.Keys.ToArray();

    public static LiteralExpression MapToLiteralExpression(Token literalToken)
    {
        var literalType = TypeMap[literalToken.Type];
        var literalValue = literalType switch
        {
            DataType.Bool => literalToken.Type == TokenType.KeywordTrue,
            DataType.Null => null,
            _             => literalToken.Content
        };
        return new LiteralExpression(literalType, literalValue);
    }
}
