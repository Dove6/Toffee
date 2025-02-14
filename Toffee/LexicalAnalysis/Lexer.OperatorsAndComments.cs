﻿using System.Text;

namespace Toffee.LexicalAnalysis;

public sealed partial class Lexer
{
    private Token? MatchOperatorOrComment()
    {
        static bool IsSymbol(char? c) =>
            c is not (null or '"') && (char.IsSymbol(c.Value) || char.IsPunctuation(c.Value));
        static bool CanExtend(string s, char c) => OperatorMapper.IsTransitionExistent(s, c);

        if (!IsSymbol(_scanner.CurrentCharacter) || !CanExtend("", _scanner.CurrentCharacter!.Value))
            return null;
        var symbolString = "";
        var errorPosition = _scanner.CurrentPosition;

        while (IsSymbol(_scanner.CurrentCharacter) && CanExtend(symbolString, _scanner.CurrentCharacter!.Value))
            symbolString += _scanner.Advance()!.Value;

        var resultingToken = OperatorMapper.MapToToken(symbolString);
        if (resultingToken.Type is TokenType.Unknown)
            EmitError(new UnknownToken(errorPosition, symbolString));
        return resultingToken.Type switch
        {
            TokenType.LineComment  => ContinueMatchingLineComment(),
            TokenType.BlockComment => ContinueMatchingBlockComment(),
            _                      => resultingToken
        };
    }

    private Token ContinueMatchingBlockComment()
    {
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
        var matchedEnd = false;
        while (_scanner.CurrentCharacter is not null)
        {
            var errorPosition = _scanner.CurrentPosition;
            var buffer = _scanner.Advance()!.Value;
            if ((buffer, _scanner.CurrentCharacter) is ('*', '/'))
            {
                matchedEnd = true;
                _scanner.Advance();
                break;
            }
            AppendCharConsideringLengthLimit(contentBuilder, buffer, ref maxLengthExceeded, errorPosition);
        }
        if (!matchedEnd)
            EmitError(new UnexpectedEndOfText(_scanner.CurrentPosition, TokenType.BlockComment));
        return new Token(TokenType.BlockComment, contentBuilder.ToString());
    }

    private Token ContinueMatchingLineComment()
    {
        var contentBuilder = new StringBuilder();
        var maxLengthExceeded = false;
        while (_scanner.CurrentCharacter is not (null or '\n'))
            CollectCharConsideringLengthLimit(contentBuilder, ref maxLengthExceeded);
        return new Token(TokenType.LineComment, contentBuilder.ToString());
    }
}
