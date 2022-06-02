using System.Collections.Immutable;
using Toffee.LexicalAnalysis;

namespace Toffee.SyntacticAnalysis;

public static class TypeMapper
{
    private static readonly ImmutableDictionary<TokenType, DataType> CastingTypeMap = new Dictionary<TokenType, DataType>
    {
        { TokenType.KeywordInt, DataType.Integer },
        { TokenType.KeywordFloat, DataType.Float },
        { TokenType.KeywordString, DataType.String },
        { TokenType.KeywordBool, DataType.Bool }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<TokenType, DataType> TypeMap = CastingTypeMap.AddRange(new []
    {
        KeyValuePair.Create(TokenType.KeywordFunction, DataType.Function),
        KeyValuePair.Create(TokenType.KeywordNull, DataType.Null)
    });

    public static TokenType[] CastingTypeTokenTypes { get; } = CastingTypeMap.Keys.ToArray();
    public static TokenType[] TypeTokenTypes { get; } = TypeMap.Keys.ToArray();

    public static DataType MapToType(TokenType type) => TypeMap[type];
    public static DataType MapToCastingType(TokenType type) => CastingTypeMap[type];
}
