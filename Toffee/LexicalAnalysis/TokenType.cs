﻿namespace Toffee.LexicalAnalysis;

public enum TokenType
{
    EndOfText,
    Unknown,

    LineComment,
    BlockComment,

    LiteralString,
    LiteralInteger,
    LiteralFloat,

    OperatorDot,
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
    // TODO: ..=

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
