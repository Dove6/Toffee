using System.Collections.ObjectModel;

namespace Toffee;

public enum TokenType
{
    EndOfText,
    Unknown,

    LineComment,
    BlockComment,

    LiteralString,
    LiteralInteger,
    LiteralFloat,

    UnknownOperator,
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
    OperatorEquals,
    OperatorPlusEquals,
    OperatorMinusEquals,
    OperatorAsteriskEquals,
    OperatorSlashEquals,
    OperatorPercentEquals,

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

public static class KeywordOrIdentifierMapper
{
    private static readonly ReadOnlyDictionary<string, TokenType> KeywordMap = new(new Dictionary<string, TokenType>
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
    });

    public static Token MapToKeywordOrIdentifier(string name) =>
        new(KeywordMap.GetValueOrDefault(name, TokenType.Identifier), name);
}

public static class OperatorMapper
{
    private static readonly ReadOnlyDictionary<string, TokenType> OperatorMap = new(new Dictionary<string, TokenType>
    {
        { ".", TokenType.OperatorDot },
        { "?.", TokenType.OperatorQueryDot },
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
        { "%=", TokenType.OperatorPercentEquals }
    });

    public static Token MapToToken(string content) =>
        new(OperatorMap.GetValueOrDefault(content, TokenType.UnknownOperator), content);
}
