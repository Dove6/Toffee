using System.Collections.ObjectModel;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class TypeMapper
{
    private static readonly ReadOnlyDictionary<TokenType, DataType> TypeMap = new(new Dictionary<TokenType, DataType>
    {
        { TokenType.KeywordInt, DataType.Integer },
        { TokenType.KeywordFloat, DataType.Float },
        { TokenType.KeywordString, DataType.String },
        { TokenType.KeywordBool, DataType.Bool },
        { TokenType.KeywordNull, DataType.Null }
    });

    public static TypeExpression MapToTypeExpression(TokenType type) => new(TypeMap[type]);  // TODO: throws
}
