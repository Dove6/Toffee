using System.Collections.Immutable;

namespace Toffee.LexicalAnalysis;

public static class KeywordOrIdentifierMapper
{
    private static readonly ImmutableDictionary<string, TokenType> KeywordMap = new Dictionary<string, TokenType>
    {
        { "int", TokenType.KeywordInt },
        { "float", TokenType.KeywordFloat },
        { "string", TokenType.KeywordString },
        { "bool", TokenType.KeywordBool },
        { "function", TokenType.KeywordFunction },
        { "null", TokenType.KeywordNull },
        { "init", TokenType.KeywordInit },
        { "const", TokenType.KeywordConst },
        { "pull", TokenType.KeywordPull },
        { "if", TokenType.KeywordIf },
        { "elif", TokenType.KeywordElif },
        { "else", TokenType.KeywordElse },
        { "while", TokenType.KeywordWhile },
        { "for", TokenType.KeywordFor },
        { "break", TokenType.KeywordBreak },
        { "break_if", TokenType.KeywordBreakIf },
        { "functi", TokenType.KeywordFuncti },
        { "return", TokenType.KeywordReturn },
        { "match", TokenType.KeywordMatch },
        { "and", TokenType.KeywordAnd },
        { "or", TokenType.KeywordOr },
        { "is", TokenType.KeywordIs },
        { "not", TokenType.KeywordNot },
        { "default", TokenType.KeywordDefault },
        { "false", TokenType.KeywordFalse },
        { "true", TokenType.KeywordTrue }
    }.ToImmutableDictionary();

    public static Token MapToKeywordOrIdentifier(string name) =>
        new(KeywordMap.GetValueOrDefault(name, TokenType.Identifier), name);
}

public static class OperatorMapper
{
    private static readonly ImmutableDictionary<string, TokenType> OperatorMap = new Dictionary<string, TokenType>
    {
        { ".", TokenType.OperatorDot },
        { "^", TokenType.OperatorCaret },
        { "+", TokenType.OperatorPlus },
        { "-", TokenType.OperatorMinus },
        { "!", TokenType.OperatorBang },
        { "*", TokenType.OperatorAsterisk },
        { "/", TokenType.OperatorSlash },
        { "%", TokenType.OperatorPercent },
        { "..", TokenType.OperatorDotDot },
        { "<", TokenType.OperatorLess },
        { "<=", TokenType.OperatorLessEquals },
        { ">", TokenType.OperatorGreater },
        { ">=", TokenType.OperatorGreaterEquals },
        { "==", TokenType.OperatorEqualsEquals },
        { "!=", TokenType.OperatorBangEquals },
        { "&&", TokenType.OperatorAndAnd },
        { "||", TokenType.OperatorOrOr },
        { "??", TokenType.OperatorQueryQuery },
        { "?>", TokenType.OperatorQueryGreater },
        { "=", TokenType.OperatorEquals },
        { "+=", TokenType.OperatorPlusEquals },
        { "-=", TokenType.OperatorMinusEquals },
        { "*=", TokenType.OperatorAsteriskEquals },
        { "/=", TokenType.OperatorSlashEquals },
        { "%=", TokenType.OperatorPercentEquals },
        { "(", TokenType.LeftParenthesis },
        { ")", TokenType.RightParenthesis },
        { "{", TokenType.LeftBrace },
        { "}", TokenType.RightBrace },
        { ",", TokenType.Comma },
        { ":", TokenType.Colon },
        { ";", TokenType.Semicolon }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, TokenType> CommentMap = new Dictionary<string, TokenType>
    {
        { "//", TokenType.LineComment },
        { "/*", TokenType.BlockComment }
    }.ToImmutableDictionary();

    public static bool IsTransitionExistent(string currentContent, char input) =>
        CommentMap.Keys.All(x => x != currentContent) &&
        OperatorMap.Keys.Concat(CommentMap.Keys).Any(x => x.StartsWith(currentContent + input));

    public static Token MapToToken(string content) => CommentMap.ContainsKey(content)
        ? new Token(CommentMap[content])
        : new Token(OperatorMap.GetValueOrDefault(content, TokenType.Unknown), content);
}
