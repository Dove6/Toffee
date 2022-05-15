﻿using System;
using FluentAssertions.Equivalency;
using Toffee.LexicalAnalysis;

namespace Toffee.Tests.SyntacticAnalysis;

public partial class ParserTests
{
    private static EquivalencyAssertionOptions<T> ProvideOptions<T>(EquivalencyAssertionOptions<T> options) =>
        options.RespectingRuntimeTypes();

    private static string MapTokenTypeToContent(TokenType type)
    {
        return type switch
        {
            TokenType.OperatorDot => ".",
            TokenType.OperatorCaret => "^",
            TokenType.OperatorPlus => "+",
            TokenType.OperatorMinus => "-",
            TokenType.OperatorAsterisk => "*",
            TokenType.OperatorSlash => "/",
            TokenType.OperatorPercent => "%",
            TokenType.OperatorDotDot => "..",
            TokenType.OperatorLess => "<",
            TokenType.OperatorLessEquals => "<=",
            TokenType.OperatorGreater => ">",
            TokenType.OperatorGreaterEquals => ">=",
            TokenType.OperatorEqualsEquals => "==",
            TokenType.OperatorBangEquals => "!=",
            TokenType.OperatorAndAnd => "&&",
            TokenType.OperatorOrOr => "||",
            TokenType.OperatorQueryQuery => "??",
            TokenType.OperatorQueryGreater => "?>",
            TokenType.OperatorEquals => "=",
            TokenType.OperatorPlusEquals => "+=",
            TokenType.OperatorMinusEquals => "-=",
            TokenType.OperatorAsteriskEquals => "*=",
            TokenType.OperatorSlashEquals => "/=",
            TokenType.OperatorPercentEquals => "%=",
            TokenType.EndOfText => "ETX",
            TokenType.OperatorBang => "!",
            TokenType.LeftParenthesis => "(",
            TokenType.RightParenthesis => ")",
            TokenType.LeftBrace => "{",
            TokenType.RightBrace => "}",
            TokenType.Comma => ",",
            TokenType.Colon => ":",
            TokenType.Semicolon => ";",
            TokenType.KeywordInt => "int",
            TokenType.KeywordFloat => "float",
            TokenType.KeywordString => "string",
            TokenType.KeywordBool => "bool",
            TokenType.KeywordFunction => "function",
            TokenType.KeywordNull => "null",
            TokenType.KeywordInit => "init",
            TokenType.KeywordConst => "const",
            TokenType.KeywordPull => "pull",
            TokenType.KeywordIf => "if",
            TokenType.KeywordElif => "elif",
            TokenType.KeywordElse => "else",
            TokenType.KeywordWhile => "while",
            TokenType.KeywordFor => "for",
            TokenType.KeywordBreak => "break",
            TokenType.KeywordBreakIf => "break_if",
            TokenType.KeywordFuncti => "functi",
            TokenType.KeywordReturn => "return",
            TokenType.KeywordMatch => "match",
            TokenType.KeywordAnd => "and",
            TokenType.KeywordOr => "or",
            TokenType.KeywordIs => "is",
            TokenType.KeywordNot => "not",
            TokenType.KeywordDefault => "default",
            TokenType.KeywordFalse => "false",
            TokenType.KeywordTrue => "true",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
