namespace Toffee;

public enum TokenType
{
    EndOfText,
    Unknown,

    LineComment,
    MultilineComment,

    LiteralString,
    LiteralInteger,
    LiteralFloat,

    OperatorDot,
    OperatorQueryDot,
    OperatorCaret,
    OperatorPlus,
    OperatorMinus,
    OperatorBang,
    OperatorAsterisk,
    OperatorSlash,
    OperatorPercent,
    OperatorDotDot,
    OperatorLess,
    OperatorLessEquals,
    OperatorGreater,
    OperatorGreaterEquals,
    OperatorEqualsEquals,
    OperatorBangEquals,
    OperatorAndAnd,
    OperatorOrOr,
    OperatorQueryQuery,
    OperatorQueryGreater,

    LeftParenthesis,
    RightParenthesis,
    LeftBrace,
    RightBrace,
    Comma,
    Colon,
    Semicolon,

    KeywordInt,
    KeywordFloat,
    KeywordString,
    KeywordBool,
    KeywordFunction,
    KeywordNull,
    KeywordInit,
    KeywordConst,
    KeywordPull,
    KeywordIf,
    KeywordElif,
    KeywordElse,
    KeywordWhile,
    KeywordFor,
    KeywordBreak,
    KeywordBreakIf,
    KeywordFuncti,
    KeywordReturn,
    KeywordMatch,
    KeywordAnd,
    KeywordOr,
    KeywordIs,
    KeywordNot,
    KeywordDefault,
    KeywordFalse,
    KeywordTrue,

    Identifier
}

public static class KeywordMapper
{
    private static readonly Dictionary<string, TokenType> KeywordMap = new()
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
    };

    public static TokenType TellKeywordFromIdentifier(string name) =>
        KeywordMap.TryGetValue(name, out var keywordType) ? keywordType : TokenType.Identifier;
}
